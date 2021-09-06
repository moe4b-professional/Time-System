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
using System.Reflection;

using MB;

using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Default
{
	public class PropertyRecorder : MonoBehaviour, ITimeBehaviour
	{
        [SerializeField]
        List<Entry> entries = default;
        public List<Entry> Entries => entries;
        [Serializable]
        public class Entry
        {
            [SerializeField]
            Object target = default;
            public Object Target => target;

            [SerializeField]
            string name = default;
            public string Name => name;

            public PropertyInfo LoadProperty()
            {
                var type = target.GetType();

                var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

                var property = type.GetProperty(Name, flags);

                return property;
            }
        }

        public List<TimeRecorder> Recorders { get; protected set; }
        public class Recorder<TValue> : TimeSnapshotRecorder<TimeValueSnapshot<TValue>>
        {
            public TValue Value
            {
                get => Getter();
                set => Setter(value);
            }

            public GetDelegate Getter { get; protected set; }
            public delegate TValue GetDelegate();

            public SetDelegate Setter { get; protected set; }
            public delegate void SetDelegate(TValue value);

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

            public Recorder(Object target, PropertyInfo property)
            {
                Getter = property.GetMethod.CreateDelegate<GetDelegate>(target);
                Setter = property.SetMethod.CreateDelegate<SetDelegate>(target);
            }
        }

        public TimeObject TimeObject { get; set; }

        void Start()
        {
            Recorders = new List<TimeRecorder>(entries.Count);

            for (int i = 0; i < entries.Count; i++)
            {
                var property = entries[i].LoadProperty();

                if(property == null)
                {
                    Debug.LogError($"Invalid Entry Specified for '{entries[i].Target}->{entries[i].Name}'");
                    continue;
                }

                var entry = CreateRecorder(entries[i].Target, property);

                Recorders.Add(entry);
            }

            TimeRecorder.Load(TimeObject, Recorders);
        }

        //Static Utility

        public static TimeRecorder CreateRecorder(Object target, PropertyInfo property)
        {
            var type = typeof(Recorder<>).MakeGenericType(property.PropertyType);

            var recorder = Activator.CreateInstance(type, target, property) as TimeRecorder;

            return recorder;
        }
    }
}