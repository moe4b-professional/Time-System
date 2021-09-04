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
    public class ObjectLifetimeRecorder : TimeRecorder
	{
        public int SpawnFrame { get; protected set; }

        public int DisposeFrame { get; protected set; }
        public bool IsMarkedForDisposal => DisposeFrame != int.MaxValue;

        ObjectLifeTimeState CheckState(int frame)
        {
            if (frame < SpawnFrame) return ObjectLifeTimeState.Despawned;
            if (frame >= DisposeFrame) return ObjectLifeTimeState.Disposed;

            return ObjectLifeTimeState.Alive;
        }

        public Action<GameObject> DestoryMethod { get; protected set; } = Object.Destroy;

        public GameObject Target => Behaviour.Self.gameObject;

        protected override void Initialize()
        {
            base.Initialize();

            //Record the frame this object was spawned in
            SpawnFrame = TimeSystem.Frame.Index;

            //Record an initial value for the dispose frame
            DisposeFrame = int.MaxValue;

            Behaviour.DisposeEvent += Dispose;

            foreach (var callback in Target.GetComponents<ICallback>())
                callback.Set(this);
        }

        void Dispose()
        {
            DisposeFrame = TimeSystem.Frame.Index;
            Target.SetActive(false);
        }

        void UnDispose()
        {
            DisposeFrame = int.MaxValue;
            Target.SetActive(true);
        }

        protected override void ApplyFrame(int frame)
        {
            base.ApplyFrame(frame);

            var state = CheckState(frame);

            Target.SetActive(state == ObjectLifeTimeState.Alive);
        }

        protected override void Resume()
        {
            base.Resume();

            var state = CheckState(TimeSystem.Frame.Index);

            switch (state)
            {
                case ObjectLifeTimeState.Despawned:
                    Despawn();
                    break;

                case ObjectLifeTimeState.Alive:
                    if (IsMarkedForDisposal) UnDispose();
                    break;

                case ObjectLifeTimeState.Disposed:
                    //No need to do anything, object will be destroyed when it's frame is removed
                    break;
            }
        }

        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            if (frame == DisposeFrame) DestoryMethod(Target);
        }

        public event Action OnDespawn;
        void Despawn()
        {
            OnDespawn?.Invoke();
            DestoryMethod(Target);
        }

        public interface ICallback
        {
            void Set(ObjectLifetimeRecorder reference);
        }
    }

    public enum ObjectLifeTimeState
    {
        Despawned,
        Alive,
        Disposed
    }
}