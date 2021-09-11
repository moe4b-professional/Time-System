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

namespace MB.TimeSystem
{
    [Serializable]
    public class ObjectLifetimeRecorder : TimeSnapshotRecorder<ObjectLifeTimeSnapshot>
    {
        public int SpawnFrame { get; private set; }

        public int DisposeFrame { get; private set; }
        public bool IsMarkedForDisposal => DisposeFrame > -1;

        ObjectLifeTimeState CheckState(int frame, ObjectLifeTimeSnapshot snapshot)
        {
            if (frame < SpawnFrame) return ObjectLifeTimeState.Despawned;
            if (IsMarkedForDisposal && frame >= DisposeFrame) return ObjectLifeTimeState.Disposed;

            if (snapshot == null) return ObjectLifeTimeState.Despawned;

            return snapshot.Active ? ObjectLifeTimeState.Active : ObjectLifeTimeState.UnActive;
        }

        public GameObject Target => Owner.gameObject;

        protected override void Initialize()
        {
            base.Initialize();

            SpawnFrame = TimeSystem.Frame.Index;
            DisposeFrame = -1;
        }

        public override void ReadSnapshot(ObjectLifeTimeSnapshot snapshot)
        {
            snapshot.Active = Target.activeSelf;
        }
        public override void ApplySnapshot(ObjectLifeTimeSnapshot snapshot)
        {
            var state = CheckState(TimeSystem.Frame.Index, snapshot);

            Target.SetActive(state == ObjectLifeTimeState.Active);
        }

        /// <summary>
        /// Event invoked when despawning this object, objects will despawn if they rewound out of the timeline
        /// </summary>
        public event Action OnDespawn;
        public virtual void Despawn()
        {
            OnDespawn?.Invoke();
            Release(TimeObjectReleaseCause.Despawned);
        }

        /// <summary>
        /// Event invoked when disposing of the TimeObject,
        /// acts as if the object was destroyed but with the possibility of reversing the destruction
        /// </summary>
        public event Action DisposeEvent;
        internal virtual void Dispose()
        {
            DisposeFrame = TimeSystem.Frame.Index;
            Target.SetActive(false);
            DisposeEvent?.Invoke();
        }

        /// <summary>
        /// Event invoked when un-disposing of the TimeObject,
        /// means the object is returned to the timeline
        /// </summary>
        public event Action OnUnDispose;
        void UnDispose()
        {
            DisposeFrame = -1;

            OnUnDispose?.Invoke();
        }

        protected override void Resume(ObjectLifeTimeSnapshot snapshot)
        {
            base.Resume(snapshot);

            var state = CheckState(TimeSystem.Frame.Index, snapshot);

            if (state == ObjectLifeTimeState.Despawned)
                Despawn();

            if (state != ObjectLifeTimeState.Disposed)
                UnDispose();
        }

        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            if (IsMarkedForDisposal && DisposeFrame == frame) Release(TimeObjectReleaseCause.Disposed);
        }

        /// <summary>
        /// Handler for Object release, invoked for releasing objects that are despawned or disposed,
        /// will destroy object by default,
        /// can be used to pool the object instead of having it destroyed
        /// </summary>
        public ReleaseDelegate ReleaseMethod { get; set; } = (target, cause) => Object.Destroy(target);
        public delegate void ReleaseDelegate(GameObject gameObject, TimeObjectReleaseCause cause);
        internal virtual void Release(TimeObjectReleaseCause cause)
        {
            ReleaseMethod(Target, cause);
        }
    }

    public enum ObjectLifeTimeState
    {
        Despawned,
        Active,
        UnActive,
        Disposed
    }

    public class ObjectLifeTimeSnapshot
    {
        public bool Active;
    }
}