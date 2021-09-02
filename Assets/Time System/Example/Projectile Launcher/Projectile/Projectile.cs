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
    public class Projectile : MonoBehaviour, ObjectInstantiationRecorder.ICallback
    {
        public void OnTimeDestory()
        {
            Debug.Log($"Projectile {name} Removed Because Time Was Rewound To Before it Was Created");
        }
    }
}