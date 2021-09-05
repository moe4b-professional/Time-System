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
    public abstract class TimeRecorderBehaviour<TRecorder> : MonoBehaviour, ITimeBehaviour
        where TRecorder : TimeRecorder
    {
        [SerializeField]
        protected TRecorder instance = default;
        public TRecorder Instance => instance;

        public TimeObject TimeObject { get; set; }

        protected virtual void Reset()
        {

        }

        protected virtual void Start()
        {
            if (TimeObject == null)
            {
                Debug.LogError($"TimeObject not Set for '{this}' Does this Behaviour Has a Time Recorder");
                return;
            }

            TimeRecorder.Load(TimeObject, instance);
        }
    }
}