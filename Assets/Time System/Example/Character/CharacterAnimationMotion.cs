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
	public class CharacterAnimationMotion : MonoBehaviour, ITimeRecorderBehaviour
	{
        [SerializeField]
        AnimationCurve curve;

        [SerializeField]
        string parameter;

        Animator animator;

        [SerializeField]
        TimeVariable<float> timer;

        public TimeObject TimeObject { get; set; }

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Start()
        {
            TimeRecorder.Load(TimeObject, timer);
        }

        void Update()
        {
            if (TimeSystem.IsPaused) return;

            float a = timer;

            var eval = curve.Evaluate(timer);

            animator.SetFloat(parameter, eval);

            timer.Value += Time.deltaTime;
        }
    }
}