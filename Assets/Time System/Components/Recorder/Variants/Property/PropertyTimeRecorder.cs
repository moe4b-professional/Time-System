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
    public class PropertyTimeRecorder : MonoBehaviour, ITimeBehaviour
    {
        [SerializeField]
        List<UnityObjectProperty> entries = default;
        public List<UnityObjectProperty> Entries => entries;

        public List<TimeRecorder> Recorders { get; protected set; }
        public class Recorder<TValue> : TimeSnapshotRecorder<TimeValueSnapshot<TValue>>
        {
            public Object Target { get; protected set; }
            public ICallback Callback { get; protected set; }

            public string Name { get; protected set; }

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

                if (Callback != null) Callback.OnPropertyRewind(Name);
            }
            public override void CopySnapshot(TimeValueSnapshot<TValue> source, TimeValueSnapshot<TValue> destination)
            {
                destination.Value = source.Value;
            }

            public Recorder(Object target, PropertyInfo property)
            {
                this.Target = target;
                Callback = target as ICallback;

                Name = property.Name;

                Getter = property.GetMethod.CreateDelegate<GetDelegate>(target);
                Setter = property.SetMethod.CreateDelegate<SetDelegate>(target);
            }
        }

        public interface ICallback
        {
            void OnPropertyRewind(string name);
        }

        public TimeObject TimeObject { get; set; }

        void Start()
        {
            Recorders = new List<TimeRecorder>(entries.Count);

            for (int i = 0; i < entries.Count; i++)
            {
                var property = entries[i].LoadProperty();

                if (property == null)
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