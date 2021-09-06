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
	public class ParticleSystemTimeRecorder : TimeSnapshotRecorder<ParticleSystemTimeSnapshot>
	{
        [SerializeField]
        ParticleSystem target = default;
        public ParticleSystem Target => target;

        public float Time { get; protected set; }

        ParticleSystemTimeState ReadState()
        {
            if (target.isStopped) return ParticleSystemTimeState.Stopped;
            if (target.isPaused) return ParticleSystemTimeState.Paused;
            if (target.isPlaying) return ParticleSystemTimeState.Playing;

            throw new NotImplementedException();
        }

        protected override void Initialize()
        {
            base.Initialize();

            if (target == null)
                throw new Exception($"No Particle System Assigned to {this} Owned by {Owner}");

            if(target.main.playOnAwake)
                target.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

            target.useAutoRandomSeed = false;
            target.randomSeed = (uint)Random.Range(0, int.MaxValue);

            if (target.main.playOnAwake)
                target.Play(false);
        }

        protected override void Record(int frame, float delta)
        {
            base.Record(frame, delta);

            Time += delta;
        }

        public override void ReadSnapshot(ParticleSystemTimeSnapshot snapshot)
        {
            snapshot.Time = Time;
            snapshot.State = ReadState();
        }
        public override void ApplySnapshot(ParticleSystemTimeSnapshot snapshot)
        {
            Time = snapshot.Time;

            target.Simulate(0, false, true);
            target.Simulate(Time, false, false);
        }
        public override void CopySnapshot(ParticleSystemTimeSnapshot source, ParticleSystemTimeSnapshot destination)
        {
            destination.Time = source.Time;
            destination.State = source.State;
        }

        protected override void Pause()
        {
            base.Pause();

            target.Pause();
        }
        protected override void Resume()
        {
            base.Resume();

            switch (LastSnapshot.State)
            {
                case ParticleSystemTimeState.Paused:
                    target.Pause();
                    break;

                case ParticleSystemTimeState.Playing:
                    target.Play();
                    break;

                case ParticleSystemTimeState.Stopped:
                    target.Stop();
                    break;
            }
        }

        public ParticleSystemTimeRecorder(ParticleSystem target)
        {
            this.target = target;
        }
    }

    public class ParticleSystemTimeSnapshot
    {
        public float Time;
        public ParticleSystemTimeState State;
    }

    public enum ParticleSystemTimeState
    {
        Paused, Playing, Stopped
    }
}