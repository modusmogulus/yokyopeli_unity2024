﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using static GIK;
//A.K.A GIKManager
//Game int keys might be a weird name for a keychain manager lol...

public class GIKS : MonoBehaviour
{
    [SerializeField]
    List<GIK> GIKClasses;

    private static GIKS _instance;
    public static GIKS Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GIKS>();

                if (_instance == null)
                {
                    GameObject singleton = new GameObject("AudioManager");
                    _instance = singleton.AddComponent<GIKS>();
                    DontDestroyOnLoad(singleton);
                }
            }
            return _instance;
        }
    }
    

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }
            //a
        }
        else
        {
            if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        //a
    }
    public string debugGetGameIntKeysArray()
    {
        string text = "";
        text = "";
        for (int i = 0; i < GIKClasses.Count; i++)
        {
            if (GIKClasses[i].value == 0)
            {
                text += "⬠";
            }
            else
            {
                text += GIKClasses[i].name.ToString() + "=";
                text += GIKClasses[i].value.ToString() + "; ";
            }

            if (i % 42 == 0)
            {
                text += "\n";
            }
        }

        return text;
    }

    public void SetGIKValue(int index, byte value) 
    {
        GIKClasses[index].value = value;
    }
    public byte GetGIKValue(int index)
    {
        return GIKClasses[index].value;
    }
    public bool GetGIKEquals(int index, byte reqValue)
    {
        return GIKClasses[index].value == reqValue;
    }

    public GIK GetGIKByName(string name)
    {
        for (int i = 0; i < GIKClasses.Count; i++)
        {
            if (GIKClasses[i].name.ToUpper() == name.ToUpper())
            {
                return GIKClasses[i];
            }
        }
        print("Wdym by GetGIKByName(" + name + ")??" + "-- No GIK with name" + name + " was found??");
        return new GIK();
    }

    public bool GetGIKEqualsByName(string name, byte value)
    {
        for (int i = 0; i < GIKClasses.Count; i++)
        {
            if (GIKClasses[i].name.ToUpper() == name.ToUpper())
            {
                return GIKClasses[i].value == value;
            }
        }

        print("No GIK with name  " + name + "  was found!");
        return false;
    }

    public void SetGIKByName(string name, byte value)
    {
        for (int i = 0; i < GIKClasses.Count; i++)
        {
            if (GIKClasses[i].name.ToUpper() == name.ToUpper())
            {
                GIKClasses[i].value = value;
            }
        }

        print("No GIK with name  " + name + "  was found!");
    }

    public void AddGIKOfName(string name, byte value, string description)
    {
        GIK createdItem = new GIK();
        createdItem.name = name;
        createdItem.value = value;
        createdItem.description = description;
        GIKClasses.Add(createdItem);
        print("Game Int Key" + name + " created at index: " + GIKClasses.Count );
    }
}