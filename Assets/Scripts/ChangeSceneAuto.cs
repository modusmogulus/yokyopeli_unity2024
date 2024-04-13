using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyTransition;
using NaughtyAttributes;
public class ChangeSceneAuto : MonoBehaviour
{
    [Scene]
    public string sceneToLoad;
    public bool isVideo = false;
    public float delay = 0.0f;
    public TransitionSettings transition;
    void Start()
    {
        if (!isVideo) { TransitionManager.Instance().Transition(sceneToLoad, transition, delay); }
    }
    
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        if (isVideo) {
            TransitionManager.Instance().Transition(sceneToLoad, transition, delay);
        }
    }

    public void ChangeScene() //for use in canvas skip button etc
    {
        TransitionManager.Instance().Transition(sceneToLoad, transition, 0);
    }
}
