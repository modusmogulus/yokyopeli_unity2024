using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using EasyTransition;
using Q3Movement;
using UnityEngine.UI;

public class MainGameObject : MonoBehaviour
{

    public bool hasJob = false;
    public GameObject interactText;
    public TMPro.TMP_Text interactTextComponent;
    public GameObject storyTextBox;
    public TMPro.TMP_Text controlTips;
    public TMPro.TMP_Text scoreText;
    public TMPro.TMP_Text bottlesText;
    public TMPro.TMP_Text moneyText;
    public TMPro.TMP_Text worthText;
    public GameObject player;
    public Q3Movement.Q3PlayerController playerController;
    public float score;
    public int bottles = 0;
    public float money = 0;
    public float worth = 0;
    
    //Accessibility settings (s_ prefix stands for settings)
    public bool s_reduceNausea = false;
    public bool s_disableHeadTilt = false;
    public bool s_alwaysHardStrafeInAir = false;
    public bool s_disableLoudNoises = true;
    //-----------------------------------------------------
    public TransitionSettings transition;
    public float loadDelay;
    byte[] gameIntKeys = new byte[2048];
    public Texture displayedCharacterTexture;
    public RawImage characterDisplayerObject;
    public GameObject mainCanvas;
    public string GameIntKeyDebugText;
        public static MainGameObject Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<GIKS>();
        }
    }

    public void setJob(bool value)
    {
        hasJob = value;
    }

    public void setCharacterTexture(string name)
    {
        //var charTex = Resources.Load<Texture>("Textures/ba_anoppi") as Texture;
        var rend = characterDisplayerObject.GetComponentInParent<CanvasRenderer>();
        characterDisplayerObject.GetComponentInParent<CharacterDisplay>().SetCharacter(name);
    }
    public void setShowCharacter(bool value)
    {
        characterDisplayerObject.enabled = value;
    }

    public void setGameIntKey(int keyInQuestion, byte valueInQuestion)
    {
       GameIntKeyDebugText = "";
        gameIntKeys[keyInQuestion] = valueInQuestion;
        if (keyInQuestion == 294)
        {
            s_alwaysHardStrafeInAir = valueInQuestion > 0;
        }

        GameIntKeyDebugText += "\n######### GAME INT KEY WAS CHANGED ########\n";

        GameIntKeyDebugText += "I     Key:  " + keyInQuestion + "        I\n";
        GameIntKeyDebugText += "I     Value was:  " + gameIntKeys[keyInQuestion] + "        I\n";
        GameIntKeyDebugText += "I     Value set to:  " + valueInQuestion + "        I\n";
        GameIntKeyDebugText += "--------------------------------------I\n";
        print(GameIntKeyDebugText);
    }

    public int getGameIntKey(int keyInQuestion)
    {
        GameIntKeyDebugText = "";
        GameIntKeyDebugText += "\n----- GAME INT KEY GET REQUEST ---------\n";

        GameIntKeyDebugText += "I     Key:  " + keyInQuestion + "              I\n";
        GameIntKeyDebugText += "I     Value:  " + gameIntKeys[keyInQuestion] + "           I\n";

        GameIntKeyDebugText += "--------------------------------------I\n";
        print(GameIntKeyDebugText);
        return gameIntKeys[keyInQuestion];
    }

    public bool getGameIntKeyEquals(int keyInQuestion, byte valueInQuestion)
    {
        if (gameIntKeys[keyInQuestion] == valueInQuestion) {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void setInteractText(string text)
    {
        if (interactText.GetComponent<TMP_Text>())
        {
            interactTextComponent = interactText.GetComponent<TMP_Text>();
        }
        interactTextComponent.text = text;
    }

    public void setBottlesText()
    {
        bottlesText.text = bottles.ToString();
        worthText.text = (bottles * 0.2).ToString();
    }

    public void changeScene(string scene)
    {
        //SceneManager.LoadScene(scene);
        TransitionManager.Instance().Transition(scene, transition, loadDelay);
    }

    public string debugGetGameIntKeysArray()
    {
        string text = "";
        text = "";
        for (int i = 0; i < gameIntKeys.Length; i++)
        {
            if (gameIntKeys[i] == 0) {
                text += ". ";
            }
            else
            {
                text += gameIntKeys[i].ToString();
            }

            if (i % 42 == 0)
            {
                text += "\n";
            }
        }
        
        return text;
    }

    private void Start()
    {
        if (interactText.GetComponent<TMP_Text>()) {
            interactTextComponent = interactText.GetComponent<TMP_Text>();
        }

    }

    private void Update()
    {

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

}
