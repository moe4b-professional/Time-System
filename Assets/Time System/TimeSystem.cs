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

using MB;
using UnityEngine.PlayerLoop;

namespace Default
{
	public static class TimeSystem
	{
		public static TimeSystemState State { get; private set; }

		public static bool IsRecording => State == TimeSystemState.Recording;
		public static bool IsPaused => State == TimeSystemState.Paused;

		/// <summary>
		/// Maximum recording time in seconds
		/// </summary>
		public static int MaxRecordDuration { get; set; } = 30;

		/// <summary>
		/// Value used to predict FPS in worst case scenario
		/// </summary>
		public static int MaxFPS { get; set; } = 144;

		/// <summary>
		/// Predicted FPS for the game, will be used to optimize internals
		/// </summary>
		public static int PredictedFPS
		{
			get
			{
				if (Application.targetFrameRate > 0)
					return Application.targetFrameRate;

				if (QualitySettings.vSyncCount > 0)
					return Screen.currentResolution.refreshRate;

				return MaxFPS;
			}
		}

		public static class Frame
        {
			public static int Index { get; internal set; }

			static List<Stamp> Stamps { get; }
			public struct Stamp
			{
				public int Index { get; }
				public float Delta { get; }

				public override string ToString() => $"Frame: {Index}, Delta: {Delta}";

				public Stamp(int frame, float delta)
				{
					this.Index = frame;
					this.Delta = delta;
				}
			}

			public static Stamp Min => Stamps.FirstOrDefault();
			public static Stamp Max => Stamps.LastOrDefault();

			internal static float CalculateDuration()
			{
				var duration = 0f;

				for (int i = 0; i < Stamps.Count; i++)
					duration += Stamps[i].Delta;

				return duration;
			}

			/// <summary>
			/// Theoritical Frame Capacity based on MaxRecordDuration, Used in Recorder's Collections
			/// </summary>
			public static int Capacity => (MaxRecordDuration * PredictedFPS) + CapacityErrorCorrection;

			/// <summary>
			/// Extra value to add to the frame capacity to help correct for any possible floating point errors,
			/// because allocating more space to begin with is better than having to resize and clone
			/// </summary>
			const int CapacityErrorCorrection = 30;

			internal static void Register()
			{
				if (Max.Index == Index && Index > 0)
					throw new Exception($"Frame {Index} Already Regsitered");

				var entry = new Stamp(Index, Time.unscaledDeltaTime);
				Stamps.Add(entry);
			}

			public static event Delegate OnRemove;
			public static void RemoveAt(int index)
            {
				var frame = Stamps[index].Index;

				Stamps.RemoveAt(index);

				OnRemove?.Invoke(frame);
			}

			/// <summary>
			/// Removes all regsitered frames
			/// </summary>
			internal static void Clear() => Clear(Min.Index);
			/// <summary>
			/// Removes all registered frames starting from marker
			/// </summary>
			/// <param name="startFrame"></param>
			internal static void Clear(int startFrame)
			{
				var index = startFrame - Min.Index;

				for (int i = Stamps.Count; i-- > index;)
					RemoveAt(i);
			}

			internal static void Fit(float time)
			{
				var duration = CalculateDuration();

				while (duration > time)
				{
					duration -= Min.Delta;

					RemoveAt(0);
				}
			}

			static Frame()
            {
				Index = 0;
				Stamps = new List<Stamp>();
			}

			public delegate void Delegate(int frame);
		}

		public static class Scenes
        {
			public static bool ClearOnLoad { get; set; } = true;

			internal static void Configure()
            {
				SceneManager.sceneLoaded += LoadCalback;
			}

			static void LoadCalback(Scene scene, LoadSceneMode mode)
			{
				if (ClearOnLoad && mode == LoadSceneMode.Single) Clear();
			}
		}

		public static class Objects
		{
			/// <summary>
			/// Destroys a GameObject with the possibility to rewind it back
			/// if that GameObject has a TimeObject component that records its lifetime
			/// </summary>
			/// <param name="target"></param>
			public static bool Dispose(GameObject target)
			{
				var context = target.GetComponent<TimeObject>();

				if (context == null) return false;
				if (context.Lifetime.Record == false) return false;

				Dispsoe(context);
				return true;
			}
			/// <summary>
			/// Destroys an object with the possibility to rewind it back
			/// </summary>
			/// <param name="target"></param>
			public static void Dispsoe(TimeObject target)
			{
				target.Dispose();
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

				if (destination > Frame.Max.Index) return false;
				if (destination < Frame.Min.Index) return false;

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

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnLoad()
        {
			State = TimeSystemState.Recording;

			MUtility.RegisterPlayerLoop<PostLateUpdate>(Process);

			Scenes.Configure();
		}

        static void Process()
		{
			if (IsRecording) Record();
		}

		public delegate void RecordDelegate(int frame, float delta);
		public static event RecordDelegate OnRecord;
		static void Record()
		{
			Frame.Register();
			Frame.Fit(MaxRecordDuration);

			OnRecord?.Invoke(Frame.Index, Time.deltaTime);

			Frame.Index += 1;
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

		public static void Clear()
		{
			Frame.Clear();
		}
	}

	public enum TimeSystemState
    {
		Recording, Paused
    }
}