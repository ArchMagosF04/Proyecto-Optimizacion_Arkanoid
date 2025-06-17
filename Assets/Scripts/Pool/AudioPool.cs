using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioPool
{
    private int startingSpawnAmount;

    public Queue<AudioSource> sourcesOnReserve = new Queue<AudioSource>(); //Contains the audio sources that are deactivated.
    public List<AudioSource> sourcesInUse = new List<AudioSource>(); //Contains the audio sources that are in use.

    public AudioPool(int startAmount)
    {
        if (startAmount <= 0) return;
        for (int i = 0; i < startAmount; i++)
        {
            CreateAudioSource();
        }
    }

    public AudioSource CreateAudioSource() //Creates a new object.
    {
        AudioSource newSource = UpdateManager.Instance.SpawnAudioSource();
        newSource.gameObject.SetActive(false);
        sourcesOnReserve.Enqueue(newSource);

        return newSource;
    }

    public AudioSource GetAudioSource() //Gets am audio source from the reserve queue, and if it's empty, then it creates a new one.
    {
        AudioSource source;

        if (sourcesOnReserve.Count > 0)
        {
            source = sourcesOnReserve.Dequeue();
        }
        else
        {
            source = CreateAudioSource();
        }
        source.gameObject.SetActive(true);
        sourcesInUse.Add(source);
        return source;
    }

    public void ReturnSource(AudioSource source) //Puts an audio source from the inUse list to the reserve.
    {
        if (sourcesInUse.Contains(source))
        {
            sourcesInUse.Remove(source);
            source.gameObject.SetActive(false);
            sourcesOnReserve.Enqueue(source);
        }
    }

    public void ReturnAll() //Returns all activate audio sources to the reserve queue.
    {
        for (int i = 0; i < sourcesInUse.Count; i++)
        {
            sourcesInUse[i].gameObject.SetActive(false);
            sourcesOnReserve.Enqueue(sourcesInUse[i]);
        }

        sourcesInUse.Clear();
    }
}
