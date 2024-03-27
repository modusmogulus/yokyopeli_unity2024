using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;

[System.Serializable]
public class Character
{
    public string name = "";
    public Texture tex;
}

public class CharacterDisplay : MonoBehaviour
{


    [SerializeField]
    public Character[] characters;

    void Start()
    {
        MainGameObject.Instance.characterDisplayerObject = GetComponent<RawImage>() ;
        GetComponent<RawImage>().enabled = false;
    }

    public void SetCharacter(string name)
    {
        foreach (Character character in characters)
        {
            if (character.name == name)
            {
                GetComponent<RawImage>().texture = character.tex;
            }
        }
    
    }
}
