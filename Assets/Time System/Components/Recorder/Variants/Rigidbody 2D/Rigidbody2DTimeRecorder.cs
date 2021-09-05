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
    public class Rigidbody2DTimeRecorder : TimeStateRecorder<Rigidbody2DTimeState>
    {
        [SerializeField]
        Rigidbody2D target = default;
        public Rigidbody2D Target => target;

        bool isKinematic;

        public override void ReadState(Rigidbody2DTimeState state)
        {
            state.Position = target.position;
            state.Velocity = target.velocity;

            state.Rotation = target.rotation;
            state.AngularVelocity = target.angularVelocity;
        }
        public override void ApplyState(Rigidbody2DTimeState state)
        {
            target.position = state.Position;
            target.velocity = state.Velocity;

            target.rotation = state.Rotation;
            target.angularVelocity = state.AngularVelocity;
        }
        public override void CopyState(Rigidbody2DTimeState source, Rigidbody2DTimeState destination)
        {
            destination.Position = source.Position;
            destination.Velocity = source.Velocity;

            destination.Rotation = source.Rotation;
            destination.AngularVelocity = source.AngularVelocity;
        }

        protected override void Configure()
        {
            base.Configure();

            if (target == null)
                throw new Exception($"No Rigidbody2D Assigned to {this} Owned by {Owner}");
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

        public Rigidbody2DTimeRecorder(Rigidbody2D target)
        {
            this.target = target;
        }
    }

    public class Rigidbody2DTimeState
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public float Rotation;
        public float AngularVelocity;

        public Rigidbody2DTimeState()
        {

        }
    }
}