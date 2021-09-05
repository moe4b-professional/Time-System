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
    [DefaultExecutionOrder(ExecutionOrder)]
    public class TimeObject : MonoBehaviour
    {
        public const int ExecutionOrder = -200;

        public List<ITimeRecorderBehaviour> Behaviours { get; protected set; }

        protected virtual void Awake()
        {
            Behaviours = new List<ITimeRecorderBehaviour>();
            GetComponentsInChildren(true, Behaviours);

            for (int i = 0; i < Behaviours.Count; i++)
                Behaviours[i].TimeObject = this;
        }

        protected virtual void Start()
        {

        }

        /// <summary>
        /// Event invoked when despawning this object, objects will despawn if they rewound out of the timeline
        /// </summary>
        public event Action OnDespawn;
        internal virtual void Despawn()
        {
            OnDespawn?.Invoke();
            Destroy(TimeObjectDestroyCause.Despawn);
        }

        /// <summary>
        /// Event invoked when disposing of the TimeObject,
        /// acts as if the object was destroyed but with the possibility of reversing the destruction
        /// </summary>
        public event Action DisposeEvent;
        internal virtual void Dispose()
        {
            DisposeEvent?.Invoke();
        }

        /// <summary>
        /// Handler for Object destruction, can be used to pool the object instead of having it garbage collected
        /// </summary>
        public DestroyDelegate DestroyMethod { get; set; } = (target, cause) => Object.Destroy(target);
        public delegate void DestroyDelegate(GameObject gameObject, TimeObjectDestroyCause cause);
        internal virtual void Destroy(TimeObjectDestroyCause cause)
        {
            DestroyMethod(gameObject, cause);
        }

        /// <summary>
        /// Event invoked when the TimeObject is actually destroyed & removed from the scene
        /// </summary>
        public event Action DestroyEvent;
        protected virtual void OnDestroy()
        {
            DestroyEvent?.Invoke();
        }
    }

    public enum TimeObjectDestroyCause
    {
        /// <summary>
        /// Object got rewound out of the timeline
        /// </summary>
        Despawn,

        /// <summary>
        /// Object was disposed out of the timeline
        /// </summary>
        Dispose,
    }
}