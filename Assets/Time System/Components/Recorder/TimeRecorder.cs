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

        protected bool TryGetSnapshot(int frame, out TSnapshot snapshot) => Snapshots.TryGetValue(frame, out snapshot);

        protected override void Configure()
        {
            base.Configure();

            Snapshots = new Dictionary<int, TSnapshot>(TimeSystem.Frame.Capacity);
        }

        public abstract void ReadSnapshot(TSnapshot snapshot);
        public abstract void ApplySnapshot(TSnapshot snapshot);
        public virtual void CopySnapshot(TSnapshot source, TSnapshot destination, bool caller) { }

        protected override void Record(int frame, float delta)
        {
            base.Record(frame, delta);

            var snapshot = SnapshotPool.Lease();
            ReadSnapshot(snapshot);
            Snapshots.Add(frame, snapshot);
        }

        protected override void Resume()
        {
            base.Resume();

            TryGetSnapshot(TimeSystem.Frame.Index, out var snapshot);
            Resume(snapshot);
        }
        protected virtual void Resume(TSnapshot snapshot)
        {
            if (snapshot != null) ApplySnapshot(snapshot);
        }

        protected override void ApplyFrame(int frame)
        {
            base.ApplyFrame(frame);

            TryGetSnapshot(frame, out var snapshot);
            ApplyFrame(frame, snapshot);
        }
        protected virtual void ApplyFrame(int frame, TSnapshot snapshot)
        {
            if (snapshot != null) ApplySnapshot(snapshot);
        }

        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            Snapshots.TryGetValue(frame, out var snapshot);
            RemoveFrame(frame, snapshot);
        }
        protected virtual void RemoveFrame(int frame, TSnapshot snapshot)
        {
            if (snapshot != null)
            {
                Snapshots.Remove(frame);
                SnapshotPool.Return(snapshot);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

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