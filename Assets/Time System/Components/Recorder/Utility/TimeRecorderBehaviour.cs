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

using MB;

using System.Reflection;

namespace Default
{
    public class TimeRecorderBehaviour : MonoBehaviour, ITimeRecorderBehaviour
    {
        public MonoBehaviour Self => this;

        [SerializeReference]
        TimeRecorder reference = default;
        public TimeRecorder Reference => reference;

        public bool IsAssigned => reference != null;

        void Start()
        {
            if(IsAssigned == false)
            {
                Debug.LogError($"No Recorder Assigned to {this}");
                return;
            }

            TimeRecorder.Load(this, reference);
        }

        public event Action DestroyEvent;
        protected virtual void OnDestroy()
        {
            DestroyEvent?.Invoke();
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(TimeRecorderBehaviour))]
        public class Inspector : Editor
        {
            new TimeRecorderBehaviour target;
            new TimeRecorderBehaviour[] targets;

            void OnEnable()
            {
                target = base.target as TimeRecorderBehaviour;
                targets = Array.ConvertAll(base.targets, x => x as TimeRecorderBehaviour);
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                DrawSelection();
            }

            void DrawSelection()
            {
                if (GUILayout.Button("Set Reference"))
                    Selection.Show(Handler);

                void Handler(Type type)
                {
                    for (int i = 0; i < targets.Length; i++)
                    {
                        targets[i].reference = Activator.CreateInstance(type) as TimeRecorder;
                        EditorUtility.SetDirty(targets[i]);
                    }
                }
            }

            public static class Selection
            {
                public static GenericMenu Menu { get; private set; }

                public static void Show(Action<Type> handler)
                {
                    Menu.ShowAsContext();

                    OnSelect += Callback;
                    void Callback(Type selection)
                    {
                        OnSelect -= Callback;
                        handler(selection);
                    }
                }

                static event Action<Type> OnSelect;
                static void Select(object argument)
                {
                    var type = argument as Type;

                    OnSelect(type);
                }

                static Selection()
                {
                    Menu = new GenericMenu();

                    var query = TypeQuery.FindAll<TimeRecorder>();

                    for (int i = 0; i < query.Count; i++)
                    {
                        if (query[i].IsAbstract) continue;
                        if (query[i].IsGenericType) continue;

                        var attribute = query[i].GetCustomAttribute<TimeRecorderMenuAttribute>();
                        if (attribute == null) return;

                        var content = new GUIContent(attribute.Path);
                        Menu.AddItem(content, false, Select, query[i]);
                    }
                }
            }
        }
#endif
    }
}