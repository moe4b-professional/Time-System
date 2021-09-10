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

namespace Default
{
    [DefaultExecutionOrder(ExecutionOrder)]
    public class TimeObject : MonoBehaviour
    {
        public const int ExecutionOrder = -200;

        public List<ITimeBehaviour> Behaviours { get; protected set; }

        [SerializeField]
        LifetimeProperty lifetime = default;
        public LifetimeProperty Lifetime => lifetime;
        [Serializable]
        public class LifetimeProperty
        {
            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            bool record = false;
            public bool Record => record;

            public ObjectLifetimeRecorder Recorder { get; protected set; }

            public void Initialize(TimeObject owner)
            {
                if (record)
                {
                    Recorder = new ObjectLifetimeRecorder();
                    TimeRecorder.Load(owner, Recorder);
                }
            }
        }

        protected virtual void Awake()
        {
            Behaviours = new List<ITimeBehaviour>();
            GetComponentsInChildren(true, Behaviours);

            for (int i = 0; i < Behaviours.Count; i++)
                Behaviours[i].TimeObject = this;
        }

        protected virtual void Start()
        {
            lifetime.Initialize(this);
        }

        /// <summary>
        /// Event invoked when despawning this object, objects will despawn if they rewound out of the timeline
        /// </summary>
        public event Action OnDespawn;
        public virtual void Despawn()
        {
            OnDespawn?.Invoke();
            Destroy(TimeObjectDestroyCause.Despawn);
        }

        public delegate void SetActiveDelegate(bool active);
        public event SetActiveDelegate OnSetActive;
        public virtual void SetActive(bool active)
        {
            OnSetActive?.Invoke(active);
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

        //Static Utility

        public static TimeObject EnsureOwnerFor(UObjectSurrogate surrogate)
        {
            var gameObject = surrogate.GameObject;

            var target = QueryComponent.InParents<TimeObject>(gameObject);

            if (target == null)
            {
#if UNITY_EDITOR
                target = Undo.AddComponent<TimeObject>(gameObject);

                ComponentUtility.MoveComponentUp(target);

                Undo.CollapseUndoOperations(2);
#endif
            }

            return target;
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