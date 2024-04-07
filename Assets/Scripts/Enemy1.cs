using UnityEngine;
using Q3Movement;
using static DamageTypes;
public class BillboardAndDamage : MonoBehaviour
{
    public float moveSpeed = 5f;
    public int damageAmount = 10;
    public bool destroyOnDamage = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("player nut not found found");
        }
    }

    void Update()
    {
        transform.LookAt(player);

        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);
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
        playerController.DealDamage(damageAmount, DamageTypes.DEFAULT);
    }
}
