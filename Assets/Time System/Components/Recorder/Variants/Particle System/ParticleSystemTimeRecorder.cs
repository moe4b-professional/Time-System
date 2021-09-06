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
	public class ParticleSystemTimeRecorder : TimeSnapshotRecorder<ParticleSystemTimeSnapshot>
	{
        [SerializeField]
        ParticleSystem target = default;
        public ParticleSystem Target => target;

        ParticleSystemTimeState ReadState()
        {
            if (target.isStopped) return ParticleSystemTimeState.Stopped;
            if (target.isPaused) return ParticleSystemTimeState.Paused;
            if (target.isPlaying) return ParticleSystemTimeState.Playing;

            throw new NotImplementedException();
        }

        public override void ReadSnapshot(ParticleSystemTimeSnapshot snapshot)
        {
            snapshot.Time = target.time;
            snapshot.State = ReadState();
        }
        public override void ApplySnapshot(ParticleSystemTimeSnapshot snapshot)
        {
            target.time = snapshot.Time;
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