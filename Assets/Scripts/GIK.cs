using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NaughtyAttributes;
using UnityEditor;

[System.Serializable]
public class GIK
{
    public string name;
    [TextArea(3, 20)]
    public string description;
    public byte value;
    [ShowAssetPreview]
    public GameObject whoCalled;
    [Button("Open most recent caller")]
    public void OpenMostRecentCaller()
    {
        if (whoCalled) { AssetDatabase.OpenAsset(whoCalled); }
    }
}
