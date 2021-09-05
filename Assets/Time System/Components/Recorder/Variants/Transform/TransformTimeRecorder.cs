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
        ToggleValue<Transform> context;
        public ToggleValue<Transform> Context => context;

        public Transform Target => context.Evaluate(Behaviour.Self.transform);

        [SerializeField]
        Space space = Space.Self;
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

        public TransformTimeRecorder()
        {

        }

        public TransformTimeRecorder(Transform context) : this(context, Space.Self) { }
        public TransformTimeRecorder(Transform context, Space space)
        {
            this.context = new ToggleValue<Transform>(context);
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