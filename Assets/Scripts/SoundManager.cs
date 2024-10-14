using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource playerSoundSource;
    [SerializeField] private AudioClip jumpSound;
    

    
    public void PlayJumpSound()
    {
        playerSoundSource.PlayOneShot(jumpSound);
    }
}
