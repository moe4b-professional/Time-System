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
    public class RigidbodyTimeRecorder : TimeSnapshotRecorder<RigidbodyTimeSnapshot>
    {
        [SerializeField]
        Rigidbody target = default;
        public Rigidbody Target => target;

        public override void ReadSnapshot(RigidbodyTimeSnapshot snapshot)
        {
            snapshot.Position = target.position;
            snapshot.Velocity = target.velocity;

            snapshot.Rotation = target.rotation;
            snapshot.AngularVelocity = target.angularVelocity;

            snapshot.IsKinematic = target.isKinematic;
        }
        public override void ApplySnapshot(RigidbodyTimeSnapshot snapshot)
        {
            target.position = snapshot.Position;
            target.velocity = snapshot.Velocity;

            target.rotation = snapshot.Rotation;
            target.angularVelocity = snapshot.AngularVelocity;
        }
        public override void CopySnapshot(RigidbodyTimeSnapshot source, RigidbodyTimeSnapshot destination)
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
                throw new Exception($"No Rigidbody Assigned to {this} Owned by {Owner}");
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

        public RigidbodyTimeRecorder(Rigidbody target)
        {
            this.target = target;
        }
    }

    public class RigidbodyTimeSnapshot
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public Quaternion Rotation;
        public Vector3 AngularVelocity;

        public bool IsKinematic;
    }
}