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
    public class AnimatorBoneRotationTimeRecorder : TimeSnapshotRecorder<AnimatorBoneRotationTimeSnapshot>
    {
        [SerializeField]
        Transform target = default;
        public Transform Target => target;

        public override void ReadSnapshot(AnimatorBoneRotationTimeSnapshot snapshot)
        {
            snapshot.Rotation = target.localRotation;
        }
        public override void ApplySnapshot(AnimatorBoneRotationTimeSnapshot snapshot)
        {
            target.localRotation = snapshot.Rotation;
        }

        public AnimatorBoneRotationTimeRecorder(Transform target)
        {
            this.target = target;
        }
    }
    public class AnimatorBoneRotationTimeSnapshot
    {
        public Quaternion Rotation;
    }

    public class AnimatorBoneCoordinatesTimeRecorder : TimeSnapshotRecorder<AnimatorBoneCoordinatesTimeSnapshot>
    {
        [SerializeField]
        Transform target = default;
        public Transform Target => target;

        public override void ReadSnapshot(AnimatorBoneCoordinatesTimeSnapshot snapshot)
        {
            snapshot.Position = target.localPosition;
            snapshot.Rotation = target.localRotation;
        }
        public override void ApplySnapshot(AnimatorBoneCoordinatesTimeSnapshot snapshot)
        {
            target.localPosition = snapshot.Position;
            target.localRotation = snapshot.Rotation;
        }

        public AnimatorBoneCoordinatesTimeRecorder(Transform target)
        {
            this.target = target;
        }
    }
    public class AnimatorBoneCoordinatesTimeSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}