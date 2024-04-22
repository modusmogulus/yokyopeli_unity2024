using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UsedSoundDestroyer : MonoBehaviour
{
    
    void Update()
    {
        if (GetComponent<AudioSource>().isPlaying == false) {
            Destroy(gameObject);
        }
    }
}
