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
    public class AnimatorRootMotionRecorder : TimeSnapshotRecorder<AnimatorRootMotionSnapshot>
    {
        public Animator Animator { get; protected set; }
        public Transform Transform => Animator.transform;

        public override void ReadSnapshot(AnimatorRootMotionSnapshot snapshot)
        {
            snapshot.Position = Animator.rootPosition;
            snapshot.Rotation = Animator.rootRotation;
        }
        public override void ApplySnapshot(AnimatorRootMotionSnapshot snapshot)
        {
            Animator.rootPosition = snapshot.Position;
            Animator.rootRotation = snapshot.Rotation;
        }

        public AnimatorRootMotionRecorder(Animator animator)
        {
            this.Animator = animator;
        }
    }

    public class AnimatorRootMotionSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}