using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class SoundHandler : MonoBehaviour
{
    [SerializeField] private List<AudioClip> audioClips;
    [SerializeField] private AudioSource audioSource;

    public void playAudioClip(int index)
    {
        if (index >= 0 && index < audioClips.Count)
        {
            audioSource.PlayOneShot(audioClips[index]);
        }
    }
}
