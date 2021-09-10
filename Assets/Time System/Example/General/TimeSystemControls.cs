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
	public class TimeSystemControls : MonoBehaviour
	{
        [SerializeField]
        Slider slider = default;

        [SerializeField]
        int maxRecordDuration = 30;

        [SerializeField]
        int speedModifier = 2;

        public static bool IsRecording => TimeSystem.IsRecording;
        public static bool IsPaused => TimeSystem.IsPaused;

        [SerializeField]
        float TransitionSpeed = 3f;
        bool InTransition = false;

        void OnValidate()
        {
            TimeSystem.Record.MaxDuration = maxRecordDuration;
        }

        void Start()
        {
            TimeSystem.Record.MaxDuration = maxRecordDuration;

            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(SliderValueChange);
            slider.gameObject.SetActive(false);
        }

        void Update()
        {
            if (InTransition) return;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsRecording)
                {
                    Pause();
                }
                else
                {
                    Resume();
                }
            }

            if (IsPaused)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    Rewind();
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    Forward();
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SceneManager.LoadScene(gameObject.scene.buildIndex);
                }
            }
        }

        void Pause()
        {
            StartCoroutine(Procedure());
            IEnumerator Procedure()
            {
                InTransition = true;

                yield return TransitionTimeScale(0f, TransitionSpeed);
                Time.timeScale = 1;

                TimeSystem.Pause();

                slider.maxValue = TimeSystem.Frame.Max;
                slider.value = TimeSystem.Frame.Index;
                slider.minValue = TimeSystem.Frame.Min;

                slider.gameObject.SetActive(true);

                InTransition = false;
            }
        }

        void Resume()
        {
            StartCoroutine(Procedure());
            IEnumerator Procedure()
            {
                InTransition = true;

                slider.gameObject.SetActive(false);
                TimeSystem.Resume();

                Time.timeScale = 0f;
                yield return TransitionTimeScale(1f, TransitionSpeed);

                InTransition = false;
            }
        }

        void Forward()
        {
            TimeSystem.Playback.Forward(speedModifier);
            slider.value = TimeSystem.Frame.Index;
        }

        void Rewind()
        {
            TimeSystem.Playback.Rewind(speedModifier);
            slider.value = TimeSystem.Frame.Index;
        }

        IEnumerator TransitionTimeScale(float target, float speed)
        {
            while(true)
            {
                Time.timeScale = Mathf.MoveTowards(Time.timeScale, target, speed * Time.unscaledDeltaTime);

                Time.fixedDeltaTime = Mathf.Lerp(1f / 120, 1f / 50, Time.timeScale);

                yield return new WaitForEndOfFrame();

                if (Mathf.Approximately(Time.timeScale, target)) break;
            }
        }

        void SliderValueChange(float value)
        {
            var frame = (int)value;

            TimeSystem.Playback.Seek(frame);
        }
    }
}