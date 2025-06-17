using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSoundLibrary", menuName = "Data/Audio/SoundLibrary", order = 0)]
public class SO_SoundLibrary : ScriptableObject
{
    [field: SerializeField] public SoundData[] soundData {  get; private set; }
}
