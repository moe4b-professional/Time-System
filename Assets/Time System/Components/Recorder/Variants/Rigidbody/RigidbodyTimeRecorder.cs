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
        [SerializeField]
        Rigidbody target = default;
        public Rigidbody Target => target;

        bool isKinematic;

        public override void ReadState(RigidbodyTimeState state)
        {
            state.Position = target.position;
            state.Velocity = target.velocity;

            state.Rotation = target.rotation;
            state.AngularVelocity = target.angularVelocity;
        }
        public override void ApplyState(RigidbodyTimeState state)
        {
            target.position = state.Position;
            target.velocity = state.Velocity;

            target.rotation = state.Rotation;
            target.angularVelocity = state.AngularVelocity;
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

            target = Owner.GetComponent<Rigidbody>();

            if (Target == null)
                throw new Exception($"No Rigidbody Assigned to {this} Owned by {Owner}");
        }

        protected override void Pause()
        {
            base.Pause();

            isKinematic = target.isKinematic;
            target.isKinematic = true;
        }
        protected override void Resume()
        {
            base.Resume();

            target.isKinematic = isKinematic;
        }

        public RigidbodyTimeRecorder(Rigidbody target)
        {
            this.target = target;
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