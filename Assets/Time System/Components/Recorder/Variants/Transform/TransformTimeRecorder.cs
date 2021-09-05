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

using MB;

namespace Default
{
    [Serializable]
    public class TransformTimeRecorder : TimeStateRecorder<TransformTimeState>
	{
        [SerializeField]
        Transform target;
        public Transform Target => target;

        [SerializeField]
        Space space;
        public Space Space => space;

        public override void ReadState(TransformTimeState state)
        {
            switch (space)
            {
                case Space.World:
                    state.Position = Target.position;
                    state.Rotation = Target.rotation;
                    break;

                case Space.Self:
                    state.Position = Target.localPosition;
                    state.Rotation = Target.localRotation;
                    break;
            }
        }
        public override void ApplyState(TransformTimeState state)
        {
            switch (space)
            {
                case Space.World:
                    Target.position = state.Position;
                    Target.rotation = state.Rotation;
                    break;

                case Space.Self:
                    Target.localPosition = state.Position;
                    Target.localRotation = state.Rotation;
                    break;
            }
        }
        public override void CopyState(TransformTimeState source, TransformTimeState destination)
        {
            destination.Position = source.Position;
            destination.Rotation = source.Rotation;
        }

        public TransformTimeRecorder(Transform target) : this(target, Space.Self) { }
        public TransformTimeRecorder(Transform target, Space space)
        {
            this.target = target;
            this.space = space;
        }
    }

    public class TransformTimeState
    {
        public Vector3 Position;
        public Quaternion Rotation;

        public TransformTimeState()
        {

        }
    }
}