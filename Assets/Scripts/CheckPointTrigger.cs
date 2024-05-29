using UnityEngine;
using UnityEngine.UI;
using Q3Movement;

public class CheckPointTrigger : MonoBehaviour
{
    public bool loadCheckpointInstead = false;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (loadCheckpointInstead) MainGameObject.Instance.CallLoadCheckpointOnPlayer();
            else MainGameObject.Instance.CallSaveCheckpointOnPlayer();

        }
    }
}