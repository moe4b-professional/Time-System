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

using MB;

namespace MB.TimeSystem
{
    [Serializable]
    public class AnimatorTimeRecorder : TimeSnapshotRecorder<AnimatorTimeSnapshot>
    {
        [SerializeField]
        Animator target = default;
        public Animator Target => target;

        [SerializeField]
        BonesProperty bones = default;
        public BonesProperty Bones => bones;
        [Serializable]
        public class BonesProperty
        {
            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            bool enable = true;
            public bool Enable => enable;

            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            UHashSet<Transform> exclusions = default;
            public UHashSet<Transform> Exclusion => exclusions;

            public List<Controller> List { get; protected set; }
            public class Controller
            {
                public Transform Transform { get; protected set; }
                public GameObject GameObject => Transform.gameObject;

                public TimeRecorder Recorder { get; protected set; }
                public void Load(TimeObject owner)
                {
                    TimeRecorder.Load(owner, Recorder);
                }

                public Controller(Transform transform, Type type)
                {
                    this.Transform = transform;

                    Recorder = CreateRecorder(transform, type);
                }

                public static TimeRecorder CreateRecorder(Transform transform, Type type)
                {
                    switch (type)
                    {
                        case Type.Joint:
                            return new AnimatorBoneRotationTimeRecorder(transform);

                        case Type.Hips:
                        case Type.Generic:
                            return new AnimatorBoneCoordinatesTimeRecorder(transform);
                    }

                    throw new NotImplementedException();
                }
            }

            public enum Type
            {
                Joint, Hips, Generic
            }

            internal void Parse(AnimatorTimeRecorder context)
            {
                if (enable == false) return;

                var animator = context.target;
                var meshes = context.target.GetComponentsInChildren<SkinnedMeshRenderer>();

                var count = 0;

                for (int i = 0; i < meshes.Length; i++)
                    count += meshes[i].bones.Length;

                List = new List<Controller>(count);

                for (int x = 0; x < meshes.Length; x++)
                {
                    for (int y = 0; y < meshes[x].bones.Length; y++)
                    {
                        if (exclusions.Contains(meshes[x].bones[y])) continue;

                        var type = DetectType(animator, meshes[x].bones[y]);
                        var bone = new Controller(meshes[x].bones[y], type);
                        bone.Load(context.Owner);

                        List.Add(bone);
                    }
                }
            }

            internal static Type DetectType(Animator animator, Transform transform)
            {
                if (animator.isHuman == false) return Type.Generic;

                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                if (transform == hips) return Type.Hips;

                return Type.Joint;
            }
        }

        [SerializeField]
        VariablesProperty variables = default;
        public VariablesProperty Variables => variables;
        [Serializable]
        public class VariablesProperty
        {
            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            bool enable = true;
            public bool Enable => enable;

            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            UHashSet<string> exclusions = default;
            public UHashSet<string> Exclusions => exclusions;

            public List<Controller> List { get; protected set; }
            public class Controller
            {
                public string ID { get; protected set; }

                public TimeRecorder Recorder { get; protected set; }
                public void Load(TimeObject owner)
                {
                    TimeRecorder.Load(owner, Recorder);
                }

                public Controller(Animator context, AnimatorControllerParameter parameter)
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

            internal void Parse(AnimatorTimeRecorder context)
            {
                if (enable == false) return;

                var animator = context.target;
                var parameters = animator.parameters;

                List = new List<Controller>(parameters.Length);

                for (int i = 0; i < parameters.Length; i++)
                {
                    if (exclusions.Contains(parameters[i].name)) continue;
                    if (animator.IsParameterControlledByCurve(parameters[i].nameHash)) continue;
                    if (parameters[i].type == UnityEngine.AnimatorControllerParameterType.Trigger) continue;

                    var entry = new Controller(animator, parameters[i]);
                    entry.Load(context.Owner);

                    List.Add(entry);
                }
            }
        }

        [SerializeField]
        LayersProperty layers = default;
        public LayersProperty Layers => layers;
        [Serializable]
        public class LayersProperty
        {
            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            bool enable = true;
            public bool Enable => enable;

            [ReadOnly(ReadOnlyPlayMode.PlayMode)]
            [SerializeField]
            UHashSet<string> exclusions = default;
            public UHashSet<string> Exclusions => exclusions;

            public List<Controller> List { get; protected set; }
            public class Controller
            {
                public int Index { get; protected set; }

                public AnimatorLayerTimeRecorder Recorder { get; protected set; }
                public void Load(TimeObject owner)
                {
                    TimeRecorder.Load(owner, Recorder);
                }

                public Controller(Animator context, int index)
                {
                    this.Index = index;
                    Recorder = new AnimatorLayerTimeRecorder(context, index);
                }
            }

            internal void Parse(AnimatorTimeRecorder context)
            {
                if (enable == false) return;

                var animator = context.target;

                List = new List<Controller>(animator.layerCount);

                for (int i = 0; i < animator.layerCount; i++)
                {
                    var name = animator.GetLayerName(i);

                    if (exclusions.Contains(name)) continue;

                    var layer = new Controller(animator, i);
                    layer.Load(context.Owner);

                    List.Add(layer);
                }
            }
        }

        public AnimatorRootMotionRecorder RootMotion { get; protected set; }

        public override void ReadSnapshot(AnimatorTimeSnapshot snapshot)
        {
            snapshot.Enabled = target.enabled;
        }
        public override void ApplySnapshot(AnimatorTimeSnapshot snapshot)
        {
        }

        protected override void Configure()
        {
            base.Configure();

            if (target == null)
                throw new Exception($"No Animator Assigned to {this} Owned by {Owner}");
        }
        protected override void Initialize()
        {
            base.Initialize();

            variables.Parse(this);
            layers.Parse(this);
            bones.Parse(this);

            if(target.applyRootMotion)
            {
                RootMotion = new AnimatorRootMotionRecorder(target);
                TimeRecorder.Load(Owner, RootMotion);
            }
        }

        protected override void Pause()
        {
            base.Pause();

            target.enabled = false;
        }
        protected override void Resume(AnimatorTimeSnapshot snapshot)
        {
            base.Resume(snapshot);

            target.enabled = snapshot.Enabled;
        }

        public AnimatorTimeRecorder(Animator target)
        {
            this.target = target;
        }
    }

    public class AnimatorTimeSnapshot
    {
        public bool Enabled;
    }
}