using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Q3Movement;
using UnityEngine.UI;
using static DamageTypes;
using TMPro;

public class MainUIScript : MonoBehaviour
{
    public DamageTypes[] dmgTypes;
    public Sprite[] snowBootIndicatorSprites;
    public Image snowBootIndicator;
    public Image[] hpIndicators;
    public Sprite[] hpNormalSprites;
    public Sprite[] hpDamagedSprites;
    public Sprite[] hpDamagedBorderSprites;
    private int lastHp = 0;
    public TMPro.TMP_Text kmhMeter;
    public TMPro.TMP_Text debugText;
    public TMPro.TMP_Text debugText2;

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

        pl = MainGameObject.Instance.playerController;
        debugText.text = MainGameObject.Instance.GameIntKeyDebugText.ToString();
        if (debugText2) { debugText2.text = MainGameObject.Instance.debugGetGameIntKeysArray(); }
        if (kmhMeter != null && pl != null) { kmhMeter.text = (Mathf.Round(pl.Speed * 320) / 100).ToString(); }
        for (int d = 0; d < dmgTypes.Length; d++) { 
            for (int i = 0; i < hpIndicators.Length; i++) {
                if (pl.GetLastDmgType() == dmgTypes[d]) { 
                    int hpQuantized = (int)Mathf.Round((pl.health / pl.maxhp) * hpIndicators.Length);
                    if (hpQuantized > i) 
                    { 
                        hpIndicators[i].sprite = hpNormalSprites[d];
                    }
                    else 
                    {
                        hpIndicators[i].sprite = hpDamagedSprites[d];
                    }
                    if (hpQuantized-1 == i && pl.health != pl.maxhp)
                    {
                        hpIndicators[i].sprite = hpDamagedBorderSprites[d];
                    }
                    if (lastHp > hpQuantized) { AudioManager.Instance.PlayAudio("UI_DamageTing"); }
                    if (lastHp < hpQuantized) { AudioManager.Instance.PlayAudio("UI_RegenTing"); }
                    lastHp = hpQuantized;
                }
            }
        }
    }

}
