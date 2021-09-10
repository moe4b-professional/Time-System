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

namespace MB.TimeSystem
{
    public class AnimatorTimeRecorderBehaviour : TimeRecorderBehaviour<AnimatorTimeRecorder>
    {
        public Animator Target => instance.Target;

        protected override void Reset()
        {
            base.Reset();

            var target = GetComponent<Animator>();
            instance = new AnimatorTimeRecorder(target);
        }
    }
}