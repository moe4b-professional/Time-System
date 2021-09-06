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
    public class Projectile : MonoBehaviour
    {
        void Awake()
        {
            GetComponent<TimeObject>().OnDespawn += OnTimeDespawn;
        }

        void OnCollisionEnter(Collision collision)
        {
            TimeSystem.Objects.Dispose(collision.gameObject);
            TimeSystem.Objects.Dispose(gameObject);
        }

        void OnTimeDespawn()
        {
            Debug.Log($"{name} Despawned from Time");
        }
    }
}