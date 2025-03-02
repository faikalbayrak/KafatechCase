using System;
using System.Collections.Generic;
using Interfaces;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class AudioManager : NetworkBehaviour, IAudioService
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
        
        #endregion

        
        #region Public Properties
        
        public bool IsSoundsOn => isSoundsOn;
        
        public event Action<bool> OnSoundChanged;
        
        #endregion

        #region Unity Methods

        private void Awake()
        {
            InitializeClipDictionary();
        }

        #endregion


        #region Public Methods

        public void PlayOneShot(string clipName)
        {
            if (!isSoundsOn)
                return;
            
            if (clipDictionary.TryGetValue(clipName, out var clip))
            {
                oneShotAudioSource.PlayOneShot(clip);
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
            
            if(!musicAudioSource.isPlaying)
                musicAudioSource.Play();
            else
            {
                musicAudioSource.volume = 0.2f;
                oneShotAudioSource.volume = 0.2f;
            }
        }
        
        public void StopMusic()
        {
            musicAudioSource.volume = 0;
            oneShotAudioSource.volume = 0;
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

