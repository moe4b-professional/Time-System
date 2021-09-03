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
    public abstract class TimeRecorderBehaviour<TRecorder> : MonoBehaviour, ITimeRecorderBehaviour
        where TRecorder : TimeRecorder
    {
        [SerializeField]
        TRecorder instance = default;
        public TRecorder Instance => instance;

        public MonoBehaviour Self => this;

        void Start()
        {
            TimeRecorder.Load(this, instance);
        }

        public event Action DisposeEvent;
        public virtual void Dispose()
        {
            DisposeEvent?.Invoke();
        }

        public event Action DestroyEvent;
        protected virtual void OnDestroy()
        {
            DestroyEvent?.Invoke();
        }
    }
}