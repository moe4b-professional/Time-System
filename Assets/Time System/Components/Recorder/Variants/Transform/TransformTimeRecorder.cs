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
    public class TransformTimeRecorder : TimeStateRecorder<TransformTimeState>
	{
        public Transform Target => Behaviour.Self.transform;

        public override void ReadState(TransformTimeState state)
        {
            state.Position = Target.position;
            state.Rotation = Target.rotation;
        }
        public override void ApplyState(TransformTimeState state)
        {
            Target.position = state.Position;
            Target.rotation = state.Rotation;
        }
        public override void CopyState(TransformTimeState source, TransformTimeState destination)
        {
            destination.Position = source.Position;
            destination.Rotation = source.Rotation;
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