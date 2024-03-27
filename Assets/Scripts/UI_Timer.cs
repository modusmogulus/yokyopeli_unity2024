using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Timer : MonoBehaviour
{
    int time;
    void Start()
    {
        StartCoroutine(WaitAndPrint(1));
    }
    private IEnumerator WaitAndPrint(int waitTime)
    {
        while (true)
        {

            GetComponent<TMPro.TMP_Text>().text = time.ToString();
            time += 1;
            yield return new WaitForSeconds(waitTime);
        }
    }
}