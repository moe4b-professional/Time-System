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

        public Transform Transform => target.transform;

        [SerializeField]
        RigidbodyTimeRecorderCoordinateSource coordinateSource = RigidbodyTimeRecorderCoordinateSource.Transform;
        public RigidbodyTimeRecorderCoordinateSource CoordinateSource => coordinateSource;

        public override void ReadSnapshot(RigidbodyTimeSnapshot snapshot)
        {
            switch (coordinateSource)
            {
                case RigidbodyTimeRecorderCoordinateSource.Transform:
                    snapshot.Position = Transform.position;
                    snapshot.Rotation = Transform.rotation;
                    break;

                case RigidbodyTimeRecorderCoordinateSource.Rigidbody:
                    snapshot.Position = target.position;
                    snapshot.Rotation = target.rotation;
                    break;
            }

            snapshot.Velocity = target.velocity;
            snapshot.AngularVelocity = target.angularVelocity;
        }
        public override void ApplySnapshot(RigidbodyTimeSnapshot snapshot)
        {
            switch (coordinateSource)
            {
                case RigidbodyTimeRecorderCoordinateSource.Transform:
                    Transform.position = snapshot.Position;
                    Transform.rotation = snapshot.Rotation;
                    break;

                case RigidbodyTimeRecorderCoordinateSource.Rigidbody:
                    target.position = snapshot.Position;
                    target.rotation = snapshot.Rotation;
                    break;
            }

            target.velocity = snapshot.Velocity;
            target.angularVelocity = snapshot.AngularVelocity;
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

            target.isKinematic = false;
        }

        public RigidbodyTimeRecorder(Rigidbody target)
        {
            this.target = target;
        }
    }

    public enum RigidbodyTimeRecorderCoordinateSource
    {
        Transform, Rigidbody
    }

    public class RigidbodyTimeSnapshot
    {
        public Vector3 Position;
        public Vector3 Velocity;

        public Quaternion Rotation;
        public Vector3 AngularVelocity;
    }
}