using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Managers
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [Serializable]
        public struct SoundData
        {
            public string name;
            public AudioClip clip;
        }

        [Header("SFX List")]
        public List<SoundData> sounds = new();
        public AudioSource audioSource;

        public void Play(string name, float volumeScale)
        {
            foreach (var sound in sounds.Where(sound => sound.name == name))
            {
                audioSource.PlayOneShot(sound.clip, volumeScale);
                return;
            }
        }
        
        public void PlayPitch(string name, float volumeScale)
        {
            float min = Mathf.Min(0.8f, 1f);
            float max = Mathf.Max(0.8f, 1f);
            audioSource.pitch = Random.Range(min, max);

            foreach (var sound in sounds.Where(sound => sound.name == name))
            {
                audioSource.PlayOneShot(sound.clip, volumeScale);
                return;
            }
        }
    }
}