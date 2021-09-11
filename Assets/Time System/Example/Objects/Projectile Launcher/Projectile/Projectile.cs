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
    public class Projectile : MonoBehaviour
    {
        void Awake()
        {
            var timeObject = GetComponent<TimeObject>();

            if (timeObject.Lifetime.Record)
                timeObject.Lifetime.Recorder.OnDespawn += OnTimeDespawn;
            else
                Debug.LogWarning($"Projectile TimeObject not set to Record Lifetime");
        }

        void OnCollisionEnter(Collision collision)
        {
            TimeSystem.Objects.TryDestroy(collision.gameObject);
            TimeSystem.Objects.TryDestroy(gameObject);
        }

        void OnTimeDespawn()
        {
            Debug.Log($"{name} Despawned from Time");
        }
    }
}