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
    public class TransformTimeRecorder : TimeSnapshotRecorder<TransformTimeSnapshot>
	{
        [SerializeField]
        Transform target;
        public Transform Target => target;

        [SerializeField]
        Space space;
        public Space Space => space;

        public override void ReadSnapshot(TransformTimeSnapshot snapshot)
        {
            switch (space)
            {
                case Space.World:
                    snapshot.Position = Target.position;
                    snapshot.Rotation = Target.rotation;
                    break;

                case Space.Self:
                    snapshot.Position = Target.localPosition;
                    snapshot.Rotation = Target.localRotation;
                    break;
            }
        }
        public override void ApplySnapshot(TransformTimeSnapshot snapshot)
        {
            switch (space)
            {
                case Space.World:
                    Target.position = snapshot.Position;
                    Target.rotation = snapshot.Rotation;
                    break;

                case Space.Self:
                    Target.localPosition = snapshot.Position;
                    Target.localRotation = snapshot.Rotation;
                    break;
            }
        }

        public TransformTimeRecorder(Transform target) : this(target, Space.Self) { }
        public TransformTimeRecorder(Transform target, Space space)
        {
            this.target = target;
            this.space = space;
        }
    }

    public class TransformTimeSnapshot
    {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}