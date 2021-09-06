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
    public class Rigidbody2DTimeRecorder : TimeSnapshotRecorder<Rigidbody2DTimeSnapshot>
    {
        [SerializeField]
        Rigidbody2D target = default;
        public Rigidbody2D Target => target;

        public override void ReadSnapshot(Rigidbody2DTimeSnapshot snapshot)
        {
            snapshot.Position = target.position;
            snapshot.Velocity = target.velocity;

            snapshot.Rotation = target.rotation;
            snapshot.AngularVelocity = target.angularVelocity;

            snapshot.IsKinematic = target.isKinematic;
        }
        public override void ApplySnapshot(Rigidbody2DTimeSnapshot snapshot)
        {
            target.position = snapshot.Position;
            target.velocity = snapshot.Velocity;

            target.rotation = snapshot.Rotation;
            target.angularVelocity = snapshot.AngularVelocity;
        }
        public override void CopySnapshot(Rigidbody2DTimeSnapshot source, Rigidbody2DTimeSnapshot destination)
        {
            destination.Position = source.Position;
            destination.Velocity = source.Velocity;

            destination.Rotation = source.Rotation;
            destination.AngularVelocity = source.AngularVelocity;

            destination.IsKinematic = source.IsKinematic;
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

            target.isKinematic = true;
        }
        protected override void Resume()
        {
            base.Resume();

            target.isKinematic = LastSnapshot.IsKinematic;
        }

        public Rigidbody2DTimeRecorder(Rigidbody2D target)
        {
            this.target = target;
        }
    }

    public class Rigidbody2DTimeSnapshot
    {
        public Vector2 Position;
        public Vector2 Velocity;

        public float Rotation;
        public float AngularVelocity;

        public bool IsKinematic;
    }
}