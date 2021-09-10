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

            Owner.DisposeEvent += Dispose;
            Owner.OnSetActive += SetActive;
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

        void Dispose()
        {
            DisposeFrame = TimeSystem.Frame.Index;
            Target.SetActive(false);
        }
        void UnDispose()
        {
            DisposeFrame = -1;
        }

        void SetActive(bool active)
        {
            if (TimeSystem.IsPaused)
            {
                Debug.LogError("Cannot Invoke SetActive when Time is Paused");
                return;
            }

            if (IsMarkedForDisposal)
            {
                Debug.LogError("Cannot Invoke SetActive for a Disposed Object");
                return;
            }

            Target.SetActive(active);
        }

        protected override void Resume(ObjectLifeTimeSnapshot snapshot)
        {
            base.Resume(snapshot);

            var state = CheckState(TimeSystem.Frame.Index, snapshot);

            if (state == ObjectLifeTimeState.Despawned)
                Owner.Despawn();

            if (state != ObjectLifeTimeState.Disposed)
                UnDispose();
        }

        protected override void RemoveFrame(int frame)
        {
            base.RemoveFrame(frame);

            if (IsMarkedForDisposal && DisposeFrame == frame) Owner.Destroy(TimeObjectDestroyCause.Dispose);
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