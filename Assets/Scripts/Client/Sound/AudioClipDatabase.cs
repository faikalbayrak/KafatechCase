using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "AudioClipDatabase", menuName = "ScriptableObjects/AudioClipDatabase", order = 0)]
public class AudioClipDatabase : ScriptableObject
{
    #region Serializable Fields

    [Header("Music")] [SerializeField] private AudioClip gameMusic;

    #endregion


    [Serializable]
    public class AudioClipData
    {
        public string clipName;
        public AudioClip clip;
    }

    [Header("One Shots ")] public List<AudioClipData> audioClips;
}
