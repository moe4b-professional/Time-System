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
    public class RigidbodyTimeRecorder : TimeStateRecorder<RigidbodyTimeState>
    {
        public Rigidbody Target { get; protected set; }
        bool isKinematic;

        public override void ReadState(RigidbodyTimeState state)
        {
            state.Position = Target.position;
            state.Velocity = Target.velocity;

            state.Rotation = Target.rotation;
            state.AngularVelocity = Target.angularVelocity;
        }
        public override void ApplyState(RigidbodyTimeState state)
        {
            Target.position = state.Position;
            Target.velocity = state.Velocity;

            Target.rotation = state.Rotation;
            Target.angularVelocity = state.AngularVelocity;
        }
        public override void CopyState(RigidbodyTimeState source, RigidbodyTimeState destination)
        {
            destination.Position = source.Position;
            destination.Velocity = source.Velocity;

            destination.Rotation = source.Rotation;
            destination.AngularVelocity = source.AngularVelocity;
        }

        protected override void Configure()
        {
            base.Configure();

            Target = Behaviour.Self.GetComponent<Rigidbody>();

            if (Target == null)
            {
                Debug.LogError($"No Rigidbody Found on {Behaviour.Self}");
                return;
            }
        }

        protected override void Pause()
        {
            base.Pause();

            isKinematic = Target.isKinematic;
            Target.isKinematic = true;
        }

        protected override void Resume()
        {
            base.Resume();

            Target.isKinematic = isKinematic;
        }
    }

    public class RigidbodyTimeState
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public Quaternion Rotation;
        public Vector3 AngularVelocity;

        public RigidbodyTimeState()
        {

        }
    }
}