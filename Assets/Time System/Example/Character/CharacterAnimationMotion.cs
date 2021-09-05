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
	public class CharacterAnimationMotion : MonoBehaviour
	{
        [SerializeField]
        AnimationCurve curve;

        [SerializeField]
        string parameter;

        Animator animator;

        float timer;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            if (animator == null) return;

            var eval = curve.Evaluate(timer);

            animator.SetFloat(parameter, eval);

            timer += Time.deltaTime;
        }
    }
}