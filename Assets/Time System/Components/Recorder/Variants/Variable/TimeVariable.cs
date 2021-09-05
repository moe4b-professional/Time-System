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
    public class TimeVariable<TValue> : TimeStateRecorder<TimeVariableState<TValue>>
    {
        [SerializeField]
        TValue value = default;
        public TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public override void ReadState(TimeVariableState<TValue> state)
        {
            state.Value = value;
        }
        public override void ApplyState(TimeVariableState<TValue> state)
        {
            value = state.Value;
        }
        public override void CopyState(TimeVariableState<TValue> source, TimeVariableState<TValue> destination)
        {
            destination.Value = source.Value;
        }

        public TimeVariable() : this(default) { }
        public TimeVariable(TValue value)
        {
            this.value = value;
        }

        public static implicit operator TValue(TimeVariable<TValue> target) => target.value;
    }

    public class TimeVariableState<TValue>
    {
		public TValue Value;
    }
}