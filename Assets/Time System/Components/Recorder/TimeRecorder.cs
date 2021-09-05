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

        protected virtual void Record(int frame)
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
    public abstract class TimeStateRecorder<TState> : TimeRecorder
        where TState : new()
    {
        public Dictionary<int, TState> Dictionary { get; private set; }

        protected TState CacheState;

        protected override void Configure()
        {
            base.Configure();

            Dictionary = new Dictionary<int, TState>(TimeSystem.Frame.Capacity);

            CacheState = StatePool.Lease();
        }

        public abstract void ReadState(TState state);
        public abstract void ApplyState(TState state);
        public abstract void CopyState(TState source, TState destination);

        protected override void Record(int frame)
        {
            base.Record(frame);

            var state = StatePool.Lease();

            ReadState(state);

            Dictionary.Add(frame, state);
        }

        protected override void Pause()
        {
            base.Pause();

            ReadState(CacheState);
        }

        protected override void Resume()
        {
            base.Resume();

            ApplyState(CacheState);
        }

        protected override void ApplyFrame(int frame)
        {
            base.ApplyFrame(frame);

            if (Dictionary.TryGetValue(frame, out var state) == false)
            {
                //No state recorded for frame for whatever reason
                return;
            }

            CopyState(state, CacheState);
            ApplyState(state);
        }

        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            if (Dictionary.TryGetValue(frame, out var state) == false) return;

            StatePool.Return(state);

            Dictionary.Remove(frame);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            StatePool.Return(CacheState);

            foreach (var state in Dictionary.Values)
                StatePool.Return(state);
        }

        //Static Utility

        public static class StatePool
        {
            static Queue<TState> Queue;

            public static Func<TState> Constructor { get; set; } = () => new TState();

            public static TState Lease()
            {
                if (Queue.Count == 0)
                    return Constructor();

                return Queue.Dequeue();
            }

            public static void Return(TState state)
            {
                Queue.Enqueue(state);
            }

            static StatePool()
            {
                Queue = new Queue<TState>(TimeSystem.Frame.Capacity);
            }
        }
    }

    public interface ITimeRecorderBehaviour
    {
        TimeObject TimeObject { get; set; }
    }
}