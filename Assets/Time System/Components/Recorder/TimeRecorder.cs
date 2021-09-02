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
        public ITimeRecorderBehaviour Behaviour { get; protected set; }

        public virtual void Load(ITimeRecorderBehaviour reference)
        {
            Behaviour = reference;

            Configure();
            Initialize();

            Behaviour.DestroyEvent += OnDestroy;
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

            TimeSystem.Playback.OnRewind += Rewind;
            TimeSystem.Playback.OnForward += Forward;

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

        protected virtual void Rewind(int frame) => ApplyFrame(frame);
        protected virtual void Forward(int frame) => ApplyFrame(frame);

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

            TimeSystem.Playback.OnRewind -= Rewind;
            TimeSystem.Playback.OnForward -= Forward;

            TimeSystem.Frame.OnRemove -= RemoveFrame;
        }

        //Static Utility

        public static void Load(ITimeRecorderBehaviour behaviour, params TimeRecorder[] recorders)
        {
            LoadAll(behaviour, recorders);
        }
        public static void Load(ITimeRecorderBehaviour behaviour, IList<TimeRecorder> recorders)
        {
            LoadAll(behaviour, recorders);
        }

        public static void LoadAll(ITimeRecorderBehaviour behaviour, IList<TimeRecorder> recorders)
        {
            for (int i = 0; i < recorders.Count; i++)
                recorders[i].Load(behaviour);
        }
    }
    public interface ITimeRecorderBehaviour
    {
        public MonoBehaviour Self { get; }

        public event Action DestroyEvent;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class TimeRecorderMenuAttribute : Attribute
    {
        public string Path { get; }

        public TimeRecorderMenuAttribute(string path)
        {
            this.Path = path;
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
}