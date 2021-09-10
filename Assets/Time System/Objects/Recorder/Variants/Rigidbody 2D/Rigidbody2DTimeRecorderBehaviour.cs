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
    public class Rigidbody2DTimeRecorderBehaviour : TimeRecorderBehaviour<Rigidbody2DTimeRecorder>
    {
        protected override void Reset()
        {
            base.Reset();

            var target = GetComponent<Rigidbody2D>();
            instance = new Rigidbody2DTimeRecorder(target);
        }
    }
}