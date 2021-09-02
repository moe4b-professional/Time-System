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
		public static class Frame
        {
			public static int Index { get; internal set; }

			public static List<Stamp> Stamps { get; }
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
			public static int Capacity => MaxRecordDuration * 60;

			internal static void Register()
			{
				var entry = new Stamp(Index, Time.unscaledDeltaTime);
				Stamps.Add(entry);
			}

			public static event FrameDelegate OnRemove;
			public static void RemoveAt(int index)
            {
				var entry = Stamps[index];

				Stamps.RemoveAt(index);

				OnRemove?.Invoke(entry.Index);
			}

			internal static void Clean(int marker)
			{
				var index = marker - Min.Index;

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
		}

		public static TimeSystemState State { get; private set; }

		public static bool IsRecording => State == TimeSystemState.Recording;
		public static bool IsPaused => State == TimeSystemState.Paused;

		/// <summary>
		/// Time in Seconds
		/// </summary>
		public static int MaxRecordDuration { get; set; } = 30;

		public delegate void FrameDelegate(int frame);

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void OnLoad()
        {
			State = TimeSystemState.Recording;

			MUtility.RegisterPlayerLoop<Update>(Update);
		}

		static void Update()
		{
			if (IsRecording) Record();
		}

		public static class Playback
        {
			public static event FrameDelegate OnRewind;
			public static int Rewind(int steps)
			{
				for (int i = 0; i < steps; i++)
					if (Rewind() == false)
						return i + 1;

				return steps;
			}
			public static bool Rewind()
			{
				if (IsRecording)
				{
					Debug.LogError("Cannot Rewind while Recording");
					return false;
				}

				if (Frame.Index <= Frame.Min.Index)
					return false;

				Frame.Index -= 1;

				OnRewind?.Invoke(Frame.Index);
				return true;
			}

			/// <summary>
			/// Moves the timeline to the current frame
			/// </summary>
			/// <param name="frame"></param>
			public static bool Seek(int frame)
			{
				if (frame == Frame.Index)
					return true;

				if (frame > Frame.Index)
				{
					var steps = frame - Frame.Index;
					return Forward(steps) == steps;
				}

				if (frame < Frame.Index)
				{
					var steps = Frame.Index - frame;
					return Rewind(steps) == steps;
				}

				throw new NotImplementedException();
			}

			public static event FrameDelegate OnForward;
			public static int Forward(int steps)
			{
				for (int i = 0; i < steps; i++)
					if (Forward() == false)
						return i + 1;

				return steps;
			}
			public static bool Forward()
			{
				if (IsRecording)
				{
					Debug.LogError("Cannot Forward while Recording");
					return false;
				}

				if (Frame.Index >= Frame.Max.Index)
					return false;

				Frame.Index += 1;

				OnForward?.Invoke(Frame.Index);
				return true;
			}
		}

		public static event FrameDelegate OnRecord;
		static void Record()
		{
			Frame.Register();
			Frame.Fit(MaxRecordDuration);

			OnRecord?.Invoke(Frame.Index);

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

			Frame.Clean(Frame.Index);
		}
	}

	public enum TimeSystemState
    {
		Recording, Paused
    }
}