using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class SoundData
{
    public string Name;
    public AudioClip Clip;
    public AudioMixerGroup MixerGroup;
    [Range(0f, 1f)] public float Volume = 1f;
    public bool WithRandomPitch;
}
