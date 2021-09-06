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

namespace Default
{
    [Serializable]
    public abstract class TimeRecorder
    {
        public TimeObject Owner { get; protected set; }

        public virtual void Load(TimeObject reference)
        {
            if (reference == null)
                throw new ArgumentNullException(nameof(reference), $"Invalid TimeObject Passed for {this}");

            Owner = reference;

            Configure();
            Initialize();

            Owner.DestroyEvent += OnDestroy;
        }

        protected virtual void Configure()
        {
            
        }
        protected virtual void Initialize()
        {
            if (TimeSystem.IsPaused) Pause();

            TimeSystem.OnRecord += Record;

            TimeSystem.OnPause += Pause;
            TimeSystem.OnResume += Resume;

            TimeSystem.Playback.OnSeek += Seek;

            TimeSystem.Frame.OnRemove += RemoveFrame;
        }

        protected virtual void Record(int frame, float delta)
        {
            
        }

        protected virtual void Pause()
        {
            
        }
        protected virtual void Resume()
        {
            
        }

        void Seek(int frame) => ApplyFrame(frame);

        protected virtual void ApplyFrame(int frame)
        {
            
        }
        protected virtual void RemoveFrame(int frame)
        {
            
        }

        protected virtual void OnDestroy()
        {
            TimeSystem.OnRecord -= Record;

            TimeSystem.OnPause -= Pause;
            TimeSystem.OnResume -= Resume;

            TimeSystem.Playback.OnSeek -= Seek;

            TimeSystem.Frame.OnRemove -= RemoveFrame;
        }

        //Static Utility

        public static void Load(TimeObject owner, params TimeRecorder[] recorders)
        {
            LoadCollection(owner, recorders);
        }
        public static void Load(TimeObject owner, IList<TimeRecorder> recorders)
        {
            LoadCollection(owner, recorders);
        }
        public static void LoadCollection(TimeObject owner, IList<TimeRecorder> recorders)
        {
            for (int i = 0; i < recorders.Count; i++)
                Load(owner, recorders[i]);
        }

        public static void Load(TimeObject owner, TimeRecorder recorder)
        {
            recorder.Load(owner);
        }
    }
    
    [Serializable]
    public abstract class TimeSnapshotRecorder<TSnapshot> : TimeRecorder
        where TSnapshot : new()
    {
        public Dictionary<int, TSnapshot> Snapshots { get; private set; }

        /// <summary>
        /// The last applied snapshot, useful for recalling conditional state
        /// like recalling a rigidbody's velocity after it's made non kinematic
        /// </summary>
        protected TSnapshot LastSnapshot;

        protected override void Configure()
        {
            base.Configure();

            Snapshots = new Dictionary<int, TSnapshot>(TimeSystem.Frame.Capacity);

            LastSnapshot = SnapshotPool.Lease();
        }

        public abstract void ReadSnapshot(TSnapshot snapshot);
        public abstract void ApplySnapshot(TSnapshot snapshot);
        public abstract void CopySnapshot(TSnapshot source, TSnapshot destination);

        protected override void Record(int frame, float delta)
        {
            base.Record(frame, delta);

            var snapshot = SnapshotPool.Lease();
            ReadSnapshot(snapshot);
            Snapshots.Add(frame, snapshot);
        }

        protected override void Pause()
        {
            base.Pause();

            ReadSnapshot(LastSnapshot);
        }
        protected override void Resume()
        {
            base.Resume();

            ApplySnapshot(LastSnapshot);
        }

        protected override void ApplyFrame(int frame)
        {
            base.ApplyFrame(frame);

            if (Snapshots.TryGetValue(frame, out var snapshot) == false)
            {
                //No snapshot recorded for frame for whatever reason
                return;
            }

            CopySnapshot(snapshot, LastSnapshot);
            ApplySnapshot(snapshot);
        }
        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            if (Snapshots.TryGetValue(frame, out var snapshot) == false) return;

            SnapshotPool.Return(snapshot);

            Snapshots.Remove(frame);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            SnapshotPool.Return(LastSnapshot);

            foreach (var snapshot in Snapshots.Values)
                SnapshotPool.Return(snapshot);
        }

        //Static Utility

        public static class SnapshotPool
        {
            static Queue<TSnapshot> Queue;

            public static Func<TSnapshot> Constructor { get; set; } = () => new TSnapshot();

            public static TSnapshot Lease()
            {
                if (Queue.Count == 0)
                    return Constructor();

                return Queue.Dequeue();
            }

            public static void Return(TSnapshot snapshot)
            {
                Queue.Enqueue(snapshot);
            }

            static SnapshotPool()
            {
                Queue = new Queue<TSnapshot>(TimeSystem.Frame.Capacity);
            }
        }
    }

    public interface ITimeBehaviour
    {
        TimeObject TimeObject { get; set; }
    }

    /// <summary>
    /// Helper class for recording a single value
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class TimeValueSnapshot<TValue>
    {
        public TValue Value;
    }
}