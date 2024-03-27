using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MGO = MainGameObject; //alias for the long name

public class MainGameObjectAPI : MonoBehaviour
{

    void Start()
    {
        
    }

    public void setReduceNausea()
    {
        MGO.Instance.s_reduceNausea = !MGO.Instance.s_reduceNausea;
    }
    public void setDisableHeadTilt()
    {
        MGO.Instance.s_disableHeadTilt = !MGO.Instance.s_disableHeadTilt;
    }

    public void setForceHardAirStrafe()
    {
        MGO.Instance.s_alwaysHardStrafeInAir = !MGO.Instance.s_alwaysHardStrafeInAir;
    }

    public void setDisableLoudNoises()
    {
        MGO.Instance.s_disableLoudNoises = !MGO.Instance.s_disableLoudNoises;
    }
}
