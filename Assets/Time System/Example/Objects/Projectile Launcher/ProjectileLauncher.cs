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
#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
	public class ProjectileLauncher : MonoBehaviour
    {
        [SerializeField]
        GameObject prefab;

        [SerializeField]
        float force;

        Camera camera;

        void Start()
        {
            camera = Camera.main;
        }

        int index = 0;

        void Update()
        {
            if (TimeSystem.IsPaused) return;

            if (Input.GetKeyDown(KeyCode.Mouse0) == false) return;

            var ray = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) == false) return;

            var instance = Instantiate(prefab, transform.position, transform.rotation);
            instance.name = $"{prefab.name} {index}";

            var rigidbody = instance.GetComponent<Rigidbody>();
            var direction = (hit.point - instance.transform.position).normalized;
            rigidbody.velocity = direction * force;

            index += 1;
        }
    }
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
}