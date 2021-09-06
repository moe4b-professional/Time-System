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
    public class AnimatorLayerTimeRecorder : TimeSnapshotRecorder<AnimatorLayerSnapshot>
    {
        public Animator Animator { get; protected set; }
        public int Index { get; protected set; }

        public AnimatorLayerTimeRecorder(Animator animator, int index)
        {
            this.Animator = animator;
            this.Index = index;
        }

        public override void ReadSnapshot(AnimatorLayerSnapshot snapshot)
        {
            var state = Animator.GetCurrentAnimatorStateInfo(Index);

            snapshot.Hash = state.fullPathHash;
            snapshot.Time = state.normalizedTime;
        }
        public override void ApplySnapshot(AnimatorLayerSnapshot snapshot)
        {

        }
        public override void CopySnapshot(AnimatorLayerSnapshot source, AnimatorLayerSnapshot destination)
        {
            destination.Hash = source.Hash;
            destination.Time = source.Time;
        }

        protected override void Resume()
        {
            base.Resume();

            Animator.Play(LastSnapshot.Hash, Index, LastSnapshot.Time);
        }
    }

    public class AnimatorLayerSnapshot
    {
        public int Hash;
        public float Time;
    }
}