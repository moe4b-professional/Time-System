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
using AnimatorControllerParameter = UnityEngine.AnimatorControllerParameter;

namespace Default
{
    [Serializable]
    public class AnimatorTimeRecorder : TimeRecorder
    {
        [SerializeField]
        Animator target = default;
        public Animator Target => target;

        bool enabled;

        public List<BoneProperty> Bones { get; protected set; }
        public class BoneProperty
        {
            public Transform Transform { get; protected set; }
            public GameObject GameObject => Transform.gameObject;

            public TransformTimeRecorder Recorder { get; protected set; }

            public void Load(TimeObject owner)
            {
                TimeRecorder.Load(owner, Recorder);
            }

            public BoneProperty(Transform transform)
            {
                this.Transform = transform;

                Recorder = new TransformTimeRecorder(transform, Space.Self);
            }
        }

        public List<VariableProperty> Variables { get; protected set; }
        public class VariableProperty
        {
            public string ID { get; protected set; }

            public TimeRecorder Recorder { get; protected set; }

            public void Load(TimeObject owner)
            {
                TimeRecorder.Load(owner, Recorder);
            }

            public VariableProperty(Animator context, AnimatorControllerParameter parameter)
            {
                ID = parameter.name;
                Recorder = Create(context, parameter);
            }

            public static TimeRecorder Create(Animator context, AnimatorControllerParameter parameter)
            {
                switch (parameter.type)
                {
                    case UnityEngine.AnimatorControllerParameterType.Float:
                        return new AnimatorFloatVariableTimeRecorder(context, parameter.name);

                    case UnityEngine.AnimatorControllerParameterType.Int:
                        return new AnimatorIntVariableTimeRecorder(context, parameter.name);

                    case UnityEngine.AnimatorControllerParameterType.Bool:
                        return new AnimatorBoolVariableTimeRecorder(context, parameter.name);
                }

                throw new NotImplementedException();
            }
        }

        protected override void Configure()
        {
            base.Configure();

            target = Owner.GetComponent<Animator>();

            if (target == null)
                throw new Exception($"No Animator Found on {Owner}");
        }

        protected override void Initialize()
        {
            base.Initialize();

            ParseVariables();
            ParseBones();
        }

        void ParseVariables()
        {
            var parameters = target.parameters;

            Variables = new List<VariableProperty>(parameters.Length);

            for (int i = 0; i < parameters.Length; i++)
            {
                var entry = new VariableProperty(target, parameters[i]);
                entry.Load(Owner);
                Variables.Add(entry);
            }
        }
        void ParseBones()
        {
            var meshes = target.GetComponentsInChildren<SkinnedMeshRenderer>();

            var count = 0;

            for (int i = 0; i < meshes.Length; i++)
                count += meshes[i].bones.Length;

            Bones = new List<BoneProperty>(count);

            for (int x = 0; x < meshes.Length; x++)
            {
                for (int y = 0; y < meshes[x].bones.Length; y++)
                {
                    var bone = new BoneProperty(meshes[x].bones[y]);
                    bone.Load(Owner);
                    Bones.Add(bone);
                }
            }
        }

        protected override void Pause()
        {
            base.Pause();

            enabled = target.enabled;
            target.enabled = false;
        }
        protected override void Resume()
        {
            base.Resume();

            target.enabled = enabled;
        }

        public AnimatorTimeRecorder(Animator target)
        {
            this.target = target;
        }
    }
}