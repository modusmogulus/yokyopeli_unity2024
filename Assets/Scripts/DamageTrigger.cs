using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Q3Movement;

public class DamageTrigger : MonoBehaviour
{
    public float damageAmount;
    public bool destroyOnDamage = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DealDamage(other.gameObject);
            if (destroyOnDamage) { Destroy(gameObject); }
        }
    }

    void DealDamage(GameObject player)
    {
        Q3PlayerController playerController = player.GetComponent<Q3PlayerController>();
        playerController.DealDamage(damageAmount, 1);
    }
}
