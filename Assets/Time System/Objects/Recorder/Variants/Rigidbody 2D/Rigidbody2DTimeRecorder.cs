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

namespace MB.TimeSystem
{
    [Serializable]
    public class Rigidbody2DTimeRecorder : TimeSnapshotRecorder<Rigidbody2DTimeSnapshot>
    {
        [SerializeField]
        Rigidbody2D target = default;
        public Rigidbody2D Target => target;

        public Transform Transform => target.transform;

        [SerializeField]
        RigidbodyTimeRecorderCoordinateSource coordinateSource = RigidbodyTimeRecorderCoordinateSource.Transform;
        public RigidbodyTimeRecorderCoordinateSource CoordinateSource => coordinateSource;

        public override void ReadSnapshot(Rigidbody2DTimeSnapshot snapshot)
        {
            switch (coordinateSource)
            {
                case RigidbodyTimeRecorderCoordinateSource.Transform:
                    snapshot.Position = Transform.position;
                    snapshot.Rotation = Transform.eulerAngles.z;
                    break;

                case RigidbodyTimeRecorderCoordinateSource.Rigidbody:
                    snapshot.Position = target.position;
                    snapshot.Rotation = target.rotation;
                    break;
            }

            snapshot.Velocity = target.velocity;
            snapshot.AngularVelocity = target.angularVelocity;

            snapshot.IsKinematic = target.isKinematic;
        }
        public override void ApplySnapshot(Rigidbody2DTimeSnapshot snapshot)
        {
            switch (coordinateSource)
            {
                case RigidbodyTimeRecorderCoordinateSource.Transform:
                    Transform.position = snapshot.Position;
                    Transform.eulerAngles = Vector3.forward * snapshot.Rotation;
                    break;

                case RigidbodyTimeRecorderCoordinateSource.Rigidbody:
                    target.position = snapshot.Position;
                    target.rotation = snapshot.Rotation;
                    break;
            }
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
        protected override void Resume(Rigidbody2DTimeSnapshot snapshot)
        {
            base.Resume(snapshot);

            target.isKinematic = snapshot.IsKinematic;
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