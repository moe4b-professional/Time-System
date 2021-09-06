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
    public abstract class AnimatorVariableTimeRecorder<TValue> : TimeSnapshotRecorder<TimeValueSnapshot<TValue>>
        where TValue : struct
    {
        [SerializeField]
        protected Animator context = default;
        public Animator Context => context;

        [SerializeField]
        string _ID = default;
        public string ID
        {
            get => _ID;
            set => _ID = value;
        }

        public int Hash { get; protected set; }

        protected override void Configure()
        {
            base.Configure();

            Hash = Animator.StringToHash(ID);
        }

        public override void CopySnapshot(TimeValueSnapshot<TValue> source, TimeValueSnapshot<TValue> destination)
        {
            destination.Value = source.Value;
        }

        public AnimatorVariableTimeRecorder(Animator context, string id)
        {
            this.context = context;
            this.ID = id;
        }
    }

    [Serializable]
    public class AnimatorIntVariableTimeRecorder : AnimatorVariableTimeRecorder<int>
    {
        public override void ReadSnapshot(TimeValueSnapshot<int> snapshot)
        {
            snapshot.Value = Context.GetInteger(Hash);
        }
        public override void ApplySnapshot(TimeValueSnapshot<int> snapshot)
        {
            Context.SetInteger(Hash, snapshot.Value);
        }

        public AnimatorIntVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }

    [Serializable]
    public class AnimatorFloatVariableTimeRecorder : AnimatorVariableTimeRecorder<float>
    {
        public override void ReadSnapshot(TimeValueSnapshot<float> snapshot)
        {
            snapshot.Value = Context.GetFloat(Hash);
        }
        public override void ApplySnapshot(TimeValueSnapshot<float> snapshot)
        {
            Context.SetFloat(Hash, snapshot.Value);
        }

        public AnimatorFloatVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }

    [Serializable]
    public class AnimatorBoolVariableTimeRecorder : AnimatorVariableTimeRecorder<bool>
    {
        public override void ReadSnapshot(TimeValueSnapshot<bool> snapshot)
        {
            snapshot.Value = Context.GetBool(Hash);
        }
        public override void ApplySnapshot(TimeValueSnapshot<bool> snapshot)
        {
            Context.SetBool(Hash, snapshot.Value);
        }

        public AnimatorBoolVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }
}