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
	public class TimeSystemControls : MonoBehaviour
	{
        [SerializeField]
        Slider slider = default;

        [SerializeField]
        int maxRecordDuration = 30;

        [SerializeField]
        int speedModifier = 2;

        public static bool IsRecording => TimeSystem.IsRecording;
        public static bool IsPaused => IsRecording == false;

        void OnValidate()
        {
            TimeSystem.MaxRecordDuration = maxRecordDuration;
        }

        void Start()
        {
            TimeSystem.MaxRecordDuration = maxRecordDuration;

            slider.wholeNumbers = true;
            slider.onValueChanged.AddListener(SliderValueChange);
            slider.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (IsRecording)
                {
                    TimeSystem.Pause();

                    slider.maxValue = TimeSystem.Frame.Max.Index;
                    slider.value = TimeSystem.Frame.Index;
                    slider.minValue = TimeSystem.Frame.Min.Index;

                    slider.gameObject.SetActive(true);
                }
                else
                {
                    slider.gameObject.SetActive(false);
                    TimeSystem.Resume();
                }
            }

            if (IsPaused)
            {
                if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
                {
                    TimeSystem.Playback.Rewind(speedModifier);
                    slider.value = TimeSystem.Frame.Index;
                }
                else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
                {
                    TimeSystem.Playback.Forward(speedModifier);
                    slider.value = TimeSystem.Frame.Index;
                }
            }
            else
            {
                if(Input.GetKeyDown(KeyCode.R))
                {
                    var target = MB.QueryComponents.InGlobal<TimeRecorderBehaviour<ObjectLifetimeRecorder>>()
                        .FirstOrDefault();

                    target?.Dispose();
                }
            }
        }

        void SliderValueChange(float value)
        {
            var frame = (int)value;

            TimeSystem.Playback.Seek(frame);
        }
    }
}