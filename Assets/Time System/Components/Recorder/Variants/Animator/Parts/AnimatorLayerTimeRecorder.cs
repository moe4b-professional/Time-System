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
    public class AnimatorLayerTimeRecorder : TimeStateRecorder<AnimatorLayerState>
    {
        public Animator Animator { get; protected set; }
        public int Index { get; protected set; }

        public AnimatorLayerTimeRecorder(Animator animator, int index)
        {
            this.Animator = animator;
            this.Index = index;
        }

        public override void ReadState(AnimatorLayerState state)
        {
            var info = Animator.GetCurrentAnimatorStateInfo(Index);

            state.Hash = info.fullPathHash;
            state.Time = info.normalizedTime;
        }
        public override void ApplyState(AnimatorLayerState state)
        {
            Animator.Play(state.Hash, Index, state.Time);
        }
        public override void CopyState(AnimatorLayerState source, AnimatorLayerState destination)
        {
            destination.Hash = source.Hash;
            destination.Time = source.Time;
        }
    }

    public class AnimatorLayerState
    {
        public int Hash;
        public float Time;
    }
}