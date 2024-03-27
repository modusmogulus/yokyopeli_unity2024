using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BottleReceiver : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bottle"))
        {
            Destroy(other.gameObject);
            AudioManager.Instance.PlayAudio("SFX_Epic");
            MainGameObject.Instance.money = MainGameObject.Instance.bottles;
        }
    }
}
