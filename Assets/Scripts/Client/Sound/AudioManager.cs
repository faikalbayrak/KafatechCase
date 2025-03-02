using System;
using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Managers
{
    public class AudioManager : MonoBehaviour, IAudioService
    {
        #region Serializable Fields
        
        [SerializeField] private AudioClipDatabase audioClipDatabase;

        [Header("Game Musics")]
        [SerializeField] private AudioSource musicAudioSource;
        
        [Header("One Shot Effects")]
        [SerializeField] private AudioSource oneShotAudioSource;

        #endregion


        #region Fields

        private Dictionary<string, AudioClip> clipDictionary;
        
        private bool isSoundsOn = true;
        
        private float musicVolumeMultiplier = 1;
        private float sfxVolumeMultiplier = 1;
        private float oneShotAudioSourceVolume;

        #endregion

        
        #region Public Properties
        
        public bool IsSoundsOn => isSoundsOn;
        
        public event Action<bool> OnSoundChanged;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            oneShotAudioSourceVolume = oneShotAudioSource.volume;
            InitializeClipDictionary();
        }

        #endregion


        #region Public Methods

        public void PlayOneShot(string clipName,float volumeScale = 1)
        {
            if (!isSoundsOn)
                return;
            
            if (clipDictionary.TryGetValue(clipName, out var clip))
            {
                oneShotAudioSource.PlayOneShot(clip,volumeScale);
            }
            else
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found!");
            }
        }
        
        public void PlayMusic()
        {
            if (!isSoundsOn)
                return;
            
            musicAudioSource.Play();
        }
        
        public void StopMusic()
        {
            musicAudioSource.Stop();
        }
        
        public void SetSoundState(bool state)
        {
            isSoundsOn = state;
            OnSoundChanged?.Invoke(isSoundsOn);

            if (state)
            {
                PlayMusic();
            }
            else
            {
                StopMusic();
            }
        }
        
        #endregion
        
        
        #region Private Methods
        
        private void InitializeClipDictionary()
        {
            clipDictionary = new Dictionary<string, AudioClip>();
            foreach (var clipData in audioClipDatabase.audioClips)
            {
                if (!clipDictionary.ContainsKey(clipData.clipName) && clipData.clip != null)
                {
                    clipDictionary[clipData.clipName] = clipData.clip;
                }
            }
        }
        
        #endregion
    }
}

