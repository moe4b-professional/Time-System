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
    public class AnimatorLayerTimeRecorder : TimeSnapshotRecorder<AnimatorLayerSnapshot>
    {
        public Animator Animator { get; protected set; }
        public int Index { get; protected set; }

        /// <summary>
        /// Is this the base layer?
        /// </summary>
        public bool IsBase => Index == 0;

        public AnimatorLayerTimeRecorder(Animator animator, int index)
        {
            this.Animator = animator;
            this.Index = index;
        }

        public override void ReadSnapshot(AnimatorLayerSnapshot snapshot)
        {
            var state = Animator.GetCurrentAnimatorStateInfo(Index);
            snapshot.State = state.fullPathHash;
            snapshot.Time = state.normalizedTime;

            if (IsBase == false) snapshot.Weight = Animator.GetLayerWeight(Index);
        }
        public override void ApplySnapshot(AnimatorLayerSnapshot snapshot)
        {
            if (IsBase == false) Animator.SetLayerWeight(Index, snapshot.Weight);
        }

        protected override void Resume(AnimatorLayerSnapshot snapshot)
        {
            base.Resume(snapshot);

            Animator.Play(snapshot.State, Index, snapshot.Time);
        }
    }

    public class AnimatorLayerSnapshot
    {
        public int State;
        public float Time;

        public float Weight;
    }
}