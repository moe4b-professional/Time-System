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
    public class AnimatorTimeRecorder : TimeStateRecorder<AnimatorTimeState>
    {
        public Animator Target { get; protected set; }
        bool enabled;
        float speed;

        public List<BoneProperty> Bones { get; protected set; }
        public class BoneProperty
        {
            public Transform Transform { get; protected set; }
            public GameObject GameObject => Transform.gameObject;

            public TransformTimeRecorder Recorder { get; protected set; }

            public void Load(ITimeRecorderBehaviour behaviour)
            {
                TimeRecorder.Load(behaviour, Recorder);
            }

            public BoneProperty(Transform transform)
            {
                this.Transform = transform;

                Recorder = new TransformTimeRecorder(transform, Space.Self);
            }
        }

        public override void ReadState(AnimatorTimeState state)
        {
            
        }
        public override void CopyState(AnimatorTimeState source, AnimatorTimeState destination)
        {

        }
        public override void ApplyState(AnimatorTimeState state)
        {
            
        }

        protected override void Configure()
        {
            base.Configure();

            Target = Behaviour.Self.gameObject.GetComponent<Animator>();

            if (Target == null)
                throw new Exception($"No Animator Found on {Behaviour.Self}");
        }

        protected override void Initialize()
        {
            base.Initialize();

            ParseBones();
        }

        protected override void Pause()
        {
            base.Pause();

            enabled = Target.enabled;
            Target.enabled = false;

            speed = Target.speed;
            Target.speed = 0f;
        }

        protected override void Resume()
        {
            base.Resume();

            Target.enabled = enabled;
            Target.speed = speed;
        }

        void ParseBones()
        {
            var meshes = Target.GetComponentsInChildren<SkinnedMeshRenderer>();

            var count = 0;

            for (int i = 0; i < meshes.Length; i++)
                count += meshes[i].bones.Length;

            Bones = new List<BoneProperty>(count);

            for (int x = 0; x < meshes.Length; x++)
            {
                for (int y = 0; y < meshes[x].bones.Length; y++)
                {
                    var bone = new BoneProperty(meshes[x].bones[y]);

                    Bones.Add(bone);

                    bone.Load(Behaviour);
                }
            }
        }
    }

    public class AnimatorTimeState
    {

    }
}