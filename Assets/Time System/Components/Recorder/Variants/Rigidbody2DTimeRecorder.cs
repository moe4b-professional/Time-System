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
    [TimeRecorderMenu("Physics 2D/Rigidbody")]
    public class Rigidbody2DTimeRecorder : TimeStateRecorder<Rigidbody2DTimeState>
    {
        public Rigidbody2D Target { get; protected set; }
        bool isKinematic;

        public override void ReadState(Rigidbody2DTimeState state)
        {
            state.Position = Target.position;
            state.Velocity = Target.velocity;

            state.Rotation = Target.rotation;
            state.AngularVelocity = Target.angularVelocity;
        }
        public override void ApplyState(Rigidbody2DTimeState state)
        {
            Target.position = state.Position;
            Target.velocity = state.Velocity;

            Target.rotation = state.Rotation;
            Target.angularVelocity = state.AngularVelocity;
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

            Target = Behaviour.Self.GetComponent<Rigidbody2D>();

            if(Target == null)
            {
                Debug.LogError($"No Rigidbody2D Found on {Behaviour.Self}");
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