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
    public class TrailRendererTimeRecorder : TimeSnapshotRecorder<TrailRendererTimeSnapshot>
    {
        [SerializeField]
        TrailRenderer target = default;
        public TrailRenderer Target => target;

        public void GetPositions(List<Vector3> list)
        {
            list.Clear();

            if (list.Capacity < target.positionCount)
                list.Capacity = target.positionCount;

            for (int i = 0; i < target.positionCount; i++)
                list.Add(target.GetPosition(i));
        }
        public void SetPositions(List<Vector3> list)
        {
            target.Clear();

            for (int i = 0; i < list.Count; i++)
                target.AddPosition(list[i]);
        }

        public override void ReadSnapshot(TrailRendererTimeSnapshot snapshot)
        {
            snapshot.Time = target.time;
            snapshot.Emitting = target.emitting;
            GetPositions(snapshot.Positions);
        }
        public override void ApplySnapshot(TrailRendererTimeSnapshot snapshot)
        {
            target.emitting = true;
            SetPositions(snapshot.Positions);
            target.emitting = false;
        }

        protected override void Pause()
        {
            base.Pause();

            target.time = Mathf.Infinity;
            target.emitting = false;
        }
        protected override void Resume(TrailRendererTimeSnapshot snapshot)
        {
            base.Resume(snapshot);

            target.time = snapshot.Time;
            target.emitting = snapshot.Emitting;
        }

        public TrailRendererTimeRecorder(TrailRenderer target)
        {
            this.target = target;
        }
    }

    public class TrailRendererTimeSnapshot
	{
        public float Time;
        public bool Emitting;
        public List<Vector3> Positions;

        public TrailRendererTimeSnapshot()
        {
            Positions = new List<Vector3>();
        }
    }
}