using UnityEngine;
using Q3Movement;
using Yarn.Unity;
public class HideOnPlay : MonoBehaviour
{

    private void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }
}
