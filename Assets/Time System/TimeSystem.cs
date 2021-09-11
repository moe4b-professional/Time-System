using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

using UnityEngine.PlayerLoop;
using MB;

namespace MB.TimeSystem
{
	public static class TimeSystem
	{
		public static TimeSystemState State { get; private set; }

		public static bool IsRecording => State == TimeSystemState.Recording;
		public static bool IsPaused => State == TimeSystemState.Paused;

		public static class Frame
		{
			public static int Index { get; internal set; }

			public static Dictionary<int, float> DeltaTimes { get; }

			/// <summary>
			/// The amount of time currently recorded in seconds
			/// </summary>
			public static float Duration { get; private set; }

			public static int Min { get; private set; }
			public static int Max { get; private set; }

			public static class Rate
			{
				/// <summary>
				/// Value used to predict FPS in worst case scenario
				/// when v-sync is off and application target frame rate is unlimited,
				/// manually set this value to the max fps supported by your game to gain a little optimization
				/// in the previous aforementioned cases
				/// </summary>
				public static int Max { get; set; } = 60;

				/// <summary>
				/// Predicted FPS for the game, will be used to optimize internals
				/// </summary>
				public static int Predicted
				{
					get
					{
						if (QualitySettings.vSyncCount > 0)
							return Screen.currentResolution.refreshRate;

						if (Application.targetFrameRate > 0)
							return Application.targetFrameRate;

						return Max;
					}
				}
			}

			public static class Capacity
			{
				/// <summary>
				/// Theoritical frame capacity based on the max record duration,
				/// used in recorder's collections to optimize their sizes
				/// </summary>
				public static int Prediction => (Record.MaxDuration * Rate.Predicted) + ErrorCorrection;

				/// <summary>
				/// Extra value to add to the frame capacity prediciton to help correct for any possible floating point errors,
				/// because allocating more space to begin with is better than having to resize and clone
				/// </summary>
				public const int ErrorCorrection = 15;
			}

			internal static void Register()
			{
				if (Max == Index && Index > 0)
					throw new Exception($"Frame {Index} Already Regsitered");

				var delta = Time.unscaledDeltaTime;

				Duration += delta;
				DeltaTimes.Add(Index, delta);

				Max = Index;
			}

			public static event Delegate OnRemove;
			internal static void Remove(int frame)
			{
				DeltaTimes.Remove(frame);

				OnRemove?.Invoke(frame);
			}
			internal static void RemoveRange(int start, int end)
            {
				for (int i = start; i <= end; i++)
					Remove(i);
			}

			/// <summary>
			/// Resets all the recorded frames
			/// </summary>
			internal static void Reset()
			{
				RemoveRange(Min, Max);

				Index = 0;
				Min = 0;
				Max = 0;

				DeltaTimes.Clear();
			}

			/// <summary>
			/// Clears all frames from the start value specified
			/// </summary>
			/// <param name="start"></param>
			internal static void Clear(int start)
			{
				if (start < Min)
					throw new InvalidOperationException($"Cannot clear frames before the min frame of {Min}");

				RemoveRange(start, Max);
				Max = Math.Max(Min, start - 1);

				Duration = CalculateDuration(Min, Max);
			}

			internal static float CalculateDuration(int start, int end)
			{
				var value = 0f;

				if (start == end)
					return value;

				for (int i = start; i <= end; i++)
					value += DeltaTimes[i];

				return value;
			}

			internal static void Fit(float time)
			{
				while (Duration > time)
				{
					Duration -= DeltaTimes[Min];

					Remove(Min);
					Min += 1;
				}
			}

			static Frame()
			{
				Index = 0;
				Min = 0;
				Max = 0;

				DeltaTimes = new Dictionary<int, float>();
			}

			public delegate void Delegate(int frame);
		}

		public static class Record
		{
			/// <summary>
			/// Maximum recording time in seconds
			/// </summary>
			public static int MaxDuration { get; set; } = 30;

			public delegate void TickDelegate(int frame, float delta);
			public static event TickDelegate OnTick;
			internal static void Tick()
			{
				Frame.Register();
				Frame.Fit(MaxDuration);

				OnTick?.Invoke(Frame.Index, Time.deltaTime);

				Frame.Index += 1;
			}
		}

		public static class Playback
		{
			public static bool Rewind() => Rewind(1);
			public static bool Rewind(int steps)
			{
				var destination = Frame.Index - steps;

				return Seek(destination);
			}

			public static event Frame.Delegate OnSeek;
			/// <summary>
			/// Moves the timeline to the current frame
			/// </summary>
			/// <param name="destination"></param>
			public static bool Seek(int destination)
			{
				if (IsRecording)
				{
					Debug.LogError("Cannot Seek while Recording");
					return false;
				}

				if (destination == Frame.Index) return true;

				if (destination > Frame.Max) return false;
				if (destination < Frame.Min) return false;

				Frame.Index = destination;
				OnSeek?.Invoke(Frame.Index);
				return true;
			}

			public static bool Forward() => Forward(1);
			public static bool Forward(int steps)
			{
				var destination = Frame.Index + steps;

				return Seek(destination);
			}
		}

		public static class Scenes
		{
			public static bool ResetOnLoad { get; set; } = true;

			internal static void Configure()
			{
				SceneManager.sceneLoaded += LoadCalback;
			}

			static void LoadCalback(Scene scene, LoadSceneMode mode)
			{
				if (ResetOnLoad && mode == LoadSceneMode.Single) Reset();
			}
		}

		public static class Objects
		{
			#region Destroy
			/// <summary>
			/// Destroys a GameObject with the possibility to rewind it back
			/// if that GameObject has a TimeObject component that records its lifetime
			/// or destroys the object using Object.Destroy otherwise
			/// </summary>
			/// <param name="target"></param>
			public static void Destroy(GameObject target) => Destroy(target, Object.Destroy);
			/// <summary>
			/// Destroys a GameObject with the possibility to rewind it back
			/// if that GameObject has a TimeObject component that records its lifetime
			/// or destroys the object using the specified fallback method
			/// </summary>
			/// <param name="target"></param>
			public static void Destroy(GameObject target, Action<GameObject> fallback)
			{
				if (TryDestroy(target) == false)
					fallback(target);
			}

			/// <summary>
			/// Destroys a GameObject with the possibility to rewind it back
			/// if that GameObject has a TimeObject component that records its lifetime,
			/// returns true if the object could be destroyed as a time object
			/// </summary>
			/// <param name="target"></param>
			public static bool TryDestroy(GameObject target)
			{
				var context = target.GetComponent<TimeObject>();

				if (context == null) return false;
				if (context.Lifetime.Record == false) return false;

				Dispose(context);
				return true;
			}
			#endregion

			/// <summary>
			/// Destroys an object with the possibility to rewind it back
			/// </summary>
			/// <param name="target"></param>
			public static void Dispose(TimeObject target) => target.Dispose();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnLoad()
		{
			State = TimeSystemState.Recording;

			MUtility.RegisterPlayerLoop<PostLateUpdate>(Process);

			Scenes.Configure();
		}

		static void Process()
		{
			if (IsRecording) Record.Tick();
		}

		public static event Action OnPause;
		public static void Pause()
		{
			if (IsPaused)
			{
				Debug.LogError("Time System Already Paused");
				return;
			}

			State = TimeSystemState.Paused;

			OnPause?.Invoke();
		}

		public static event Action OnResume;
		public static void Resume()
		{
			if (IsRecording)
			{
				Debug.Log("Time System is Already Resumed");
				return;
			}

			State = TimeSystemState.Recording;

			OnResume?.Invoke();

			Frame.Clear(Frame.Index);
		}

		/// <summary>
		/// Resets the TimeSystem
		/// </summary>
		public static void Reset()
		{
			Frame.Reset();
		}
	}

	public enum TimeSystemState
	{
		Recording, Paused
	}
}