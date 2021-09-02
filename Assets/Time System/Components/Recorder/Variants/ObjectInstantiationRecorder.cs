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
    [Serializable]
    [TimeRecorderMenu("Object/Instantiation")]
    public class ObjectInstantiationRecorder : TimeRecorder
	{
        public int Frame { get; protected set; }
        bool CheckState(int target) => target >= Frame;

        public GameObject Target => Behaviour.Self.gameObject;

        protected override void Initialize()
        {
            base.Initialize();

            //Record the frame this object was spawned in
            Frame = TimeSystem.Frame.Index;
        }

        protected override void ApplyFrame(int frame)
        {
            base.ApplyFrame(frame);

            var state = CheckState(frame);

            Target.SetActive(state);
        }

        protected override void Resume()
        {
            base.Resume();

            var state = CheckState(TimeSystem.Frame.Index);

            if (state == false) Destroy();
        }

        public event Action OnDestory;
        void Destroy()
        {
            foreach (var callback in Target.GetComponents<ICallback>())
                callback.OnTimeDestory();

            OnDestory?.Invoke();

            Object.Destroy(Target);
        }

        public interface ICallback
        {
            void OnTimeDestory();
        }
    }
}