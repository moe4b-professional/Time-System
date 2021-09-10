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
using UnityEngine.Events;

namespace MB.TimeSystem
{
	public class TimePlaybackUnityEvents : MonoBehaviour
	{
        void Start()
        {
            TimeSystem.OnPause += TimePauseCallback;
            TimeSystem.OnResume += TimeResumeCallback;
        }

        [SerializeField]
        UnityEvent onPause = default;
        public UnityEvent OnPause => onPause;
        void TimePauseCallback()
        {
            onPause?.Invoke();
        }

        [SerializeField]
        UnityEvent onResume = default;
        public UnityEvent OnResume => onResume;
        void TimeResumeCallback()
        {
            onResume?.Invoke();
        }

        void OnDestroy()
        {
            TimeSystem.OnPause -= TimePauseCallback;
            TimeSystem.OnResume -= TimeResumeCallback;
        }
    }
}