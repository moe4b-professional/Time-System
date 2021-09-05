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
    public class AnimatorRootMotionRecorder : TimeStateRecorder<AnimatorRootMotionState>
    {
        public Animator Animator { get; protected set; }
        public Transform Transform => Animator.transform;

        public override void ReadState(AnimatorRootMotionState state)
        {
            state.Position = Animator.rootPosition;
            state.Rotation = Animator.rootRotation;
        }
        public override void ApplyState(AnimatorRootMotionState state)
        {
            Animator.rootPosition = state.Position;
            Animator.rootRotation = state.Rotation;
        }
        public override void CopyState(AnimatorRootMotionState source, AnimatorRootMotionState destination)
        {
            destination.Position = source.Position;
            destination.Rotation = source.Rotation;
        }

        public AnimatorRootMotionRecorder(Animator animator)
        {
            this.Animator = animator;
        }
    }

    public class AnimatorRootMotionState
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}