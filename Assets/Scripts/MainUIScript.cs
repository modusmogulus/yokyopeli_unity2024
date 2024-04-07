using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Q3Movement;
using UnityEngine.UI;
using static DamageTypes;
public class MainUIScript : MonoBehaviour
{
    public DamageTypes[] dmgTypes;
    public Sprite[] snowBootIndicatorSprites;
    public Image snowBootIndicator;
    public Image[] hpIndicators;
    public Sprite[] hpNormalSprites;
    public Sprite[] hpDamagedSprites;
    public Sprite[] hpDamagedBorderSprites;
    
    private Q3Movement.Q3PlayerController pl;

    void Start()
    {
        MainGameObject.Instance.mainCanvas = gameObject;
        pl = MainGameObject.Instance.playerController;
    }

    void SetSnowBootIndicator(int index)
    {
        snowBootIndicator.sprite = snowBootIndicatorSprites[index];
    }

    private void Update()
    {
        for (int i = 0; i < hpIndicators.Length; i++) { 
            int hpQuantized = (int)Mathf.Round((pl.health / pl.maxhp) * hpIndicators.Length);
            if (hpQuantized > i) 
            { 
                hpIndicators[i].sprite = hpNormalSprites[0];
            }
            else 
            {
                hpIndicators[i].sprite = hpDamagedSprites[0];
            }
        }
    }

}
