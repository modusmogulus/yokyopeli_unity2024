using UnityEngine;
using Q3Movement;
using Yarn.Unity;
public class HideOnPlay : MonoBehaviour
{
    public bool hideOnPlay = true;
    public bool alsoDisableOnStart = false;

    private void Start()
    {
        
        GetComponent<MeshRenderer>().enabled = !hideOnPlay;
        gameObject.active = !alsoDisableOnStart;

    }
}
