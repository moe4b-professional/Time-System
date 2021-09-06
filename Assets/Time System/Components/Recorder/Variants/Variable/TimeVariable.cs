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
    public class TimeVariable<TValue> : TimeSnapshotRecorder<TimeValueSnapshot<TValue>>
    {
        [SerializeField]
        TValue value = default;
        public TValue Value
        {
            get => value;
            set => this.value = value;
        }

        public override void ReadSnapshot(TimeValueSnapshot<TValue> snapshot)
        {
            snapshot.Value = value;
        }
        public override void ApplySnapshot(TimeValueSnapshot<TValue> snapshot)
        {
            value = snapshot.Value;
        }
        public override void CopySnapshot(TimeValueSnapshot<TValue> source, TimeValueSnapshot<TValue> destination)
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

    public class TimeField<TValue> : TimeSnapshotRecorder<TimeValueSnapshot<TValue>>
    {
        public TValue Value
        {
            get => Getter();
            set => Setter(value);
        }

        public GetterDelegate Getter { get; protected set; }
        public delegate TValue GetterDelegate();

        public SetterDelegate Setter { get; protected set; }
        public delegate void SetterDelegate(TValue value);

        public override void ReadSnapshot(TimeValueSnapshot<TValue> snapshot)
        {
            snapshot.Value = Value;
        }
        public override void ApplySnapshot(TimeValueSnapshot<TValue> snapshot)
        {
            Value = snapshot.Value;
        }
        public override void CopySnapshot(TimeValueSnapshot<TValue> source, TimeValueSnapshot<TValue> destination)
        {
            destination.Value = source.Value;
        }

        public TimeField(GetterDelegate getter, SetterDelegate setter)
        {
            this.Getter = getter;
            this.Setter = setter;
        }
    }
}