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
	public class ParticleSystemTimeRecorderBehaviour : TimeRecorderBehaviour<ParticleSystemTimeRecorder>
	{
        protected override void Reset()
        {
            base.Reset();

            var target = GetComponent<ParticleSystem>();
            instance = new ParticleSystemTimeRecorder(target);
        }
    }
}