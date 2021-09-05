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
    public abstract class AnimatorVariableTimeRecorder<TValue> : TimeStateRecorder<AnimatorVariableTimeState<TValue>>
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

        public override void CopyState(AnimatorVariableTimeState<TValue> source, AnimatorVariableTimeState<TValue> destination)
        {
            destination.Value = source.Value;
        }

        public AnimatorVariableTimeRecorder(Animator context, string id)
        {
            this.context = context;
            this.ID = id;
        }
    }

    public class AnimatorVariableTimeState<TValue>
        where TValue : struct
    {
        public TValue Value;
    }

    [Serializable]
    public class AnimatorIntVariableTimeRecorder : AnimatorVariableTimeRecorder<int>
    {
        public override void ReadState(AnimatorVariableTimeState<int> state)
        {
            state.Value = Context.GetInteger(Hash);
        }
        public override void ApplyState(AnimatorVariableTimeState<int> state)
        {
            Context.SetInteger(Hash, state.Value);
        }

        public AnimatorIntVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }

    [Serializable]
    public class AnimatorFloatVariableTimeRecorder : AnimatorVariableTimeRecorder<float>
    {
        public override void ReadState(AnimatorVariableTimeState<float> state)
        {
            state.Value = Context.GetFloat(Hash);
        }
        public override void ApplyState(AnimatorVariableTimeState<float> state)
        {
            Context.SetFloat(Hash, state.Value);
        }

        public AnimatorFloatVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }

    [Serializable]
    public class AnimatorBoolVariableTimeRecorder : AnimatorVariableTimeRecorder<bool>
    {
        public override void ReadState(AnimatorVariableTimeState<bool> state)
        {
            state.Value = Context.GetBool(Hash);
        }
        public override void ApplyState(AnimatorVariableTimeState<bool> state)
        {
            Context.SetBool(Hash, state.Value);
        }

        public AnimatorBoolVariableTimeRecorder(Animator animator, string id) : base(animator, id) { }
    }
}