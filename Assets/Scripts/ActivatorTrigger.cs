using UnityEngine;
using Q3Movement;
using Yarn.Unity;
using NaughtyAttributes;
using UnityEditor;
public class ActivatorTrigger : MonoBehaviour
{
    private Q3PlayerController playerController;

    [ShowAssetPreview(128, 128)]
    public GameObject gameObject;  // btw this is very dangerous, should be renamed to something else prob

    public bool changeStateOnEnter = true;
    public bool changeStateOnExit = false;

    [ShowIf("changeStateOnEnter")]
    public bool activeOnEnter = true;
    [ShowIf("changeStateOnExit")]
    public bool activeOnExit = true;



    public bool destroyOnEnter = false;
    public bool interact = false;
    private bool isInRange = false;
    [ShowIf("interact")]
    public string interactTextString = "'E'";
    public string soundToPlay = "";
    public float delay = 0f; // New variable for delay
    private bool delayInProgress = false;
    private float delayTimer = 0f;
    [DisableIf("setGameIntKey")]
    public bool requireGameIntKey = false;
    [DisableIf("requireGameIntKey")]
    public bool setGameIntKey;

    [HorizontalLine(color: EColor.Green)]
    [InfoBox("Game Int Keys -- My idea of a keychain manager lol...")]
    public string gameIntKeyName;
    public byte gameIntValue = 0;
    [ResizableTextArea]
    public string eventDescription;
    [HorizontalLine(color: EColor.Green)]
    //[EnableIf("startDialogue")]
    public bool targetIsGUI = false;
    public bool startDialogue;

    public bool costsMoney = false;
    public int cost = 0;
    
    [ShowIf("startDialogue")]
    public string yarnDialogueNode = "";
    [ShowIf("costsMoney")]
    public string yarnDialogueNodeTooPoor = "";


    [ShowIf("startDialogue")]
    public Yarn.Unity.DialogueRunner dialogueRunner;

    [ShowIf("startDialogue")]
    private ADGS_Dialogue diag;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && changeStateOnEnter)
        {
            playerController = other.GetComponent<Q3PlayerController>();
            if (gameObject != null) {
                if (targetIsGUI == true && gameObject.GetComponent<ADGS_Dialogue>())
                {
                    diag = gameObject.GetComponent<ADGS_Dialogue>();
                }
            }

            isInRange = true;

            if (!interact)
            {
                if (delay <= 0f)
                    ActivateObject();
                else
                {
                    delayInProgress = true;
                    delayTimer = 0f;
                }
            }
            else
            {
                MainGameObject.Instance.setInteractText(interactTextString);
                MainGameObject.Instance.interactText.SetActive(true);
            }
        }
    }
    public GameObject GIKManagerPrefab;
    [Button("Add Game Int Key Event")]
    public void AddGIK() {
        GIKS GIKSComp = GIKManagerPrefab.GetComponent<GIKS>();
        GIKSComp.AddGIKOfName(gameIntKeyName, 0, eventDescription);
        GIKSComp.GetGIKByName(gameIntKeyName).whoCalled = this.gameObject;
        PrefabUtility.SaveAsPrefabAsset(GIKManagerPrefab, AssetDatabase.GetAssetPath(GIKManagerPrefab));
    }

    [Button("View Game Int Key Events")]
    public void ViewGIKs() {
        GIKS GIKSComp = GIKManagerPrefab.GetComponent<GIKS>();
        AssetDatabase.OpenAsset(GIKManagerPrefab);
    }

    public TextAsset editorYarnFile;
    [Button("Open Yarn Project (selector only for editor - doesnt change anything ingame)")]
    public void EditorOpenYarnThing()
    {
        AssetDatabase.OpenAsset(editorYarnFile);
    }


    [Button("MakeSelfInvisible")]
    public void MakeSelfInvisible() 
    {
        if (!GetComponent<HideOnPlay>()) new HideOnPlay();   
    }
    
    private void OnTriggerExit(Collider other)
    {
        MainGameObject.Instance.interactText.SetActive(false);
        if (other.CompareTag("Player"))
        {
            isInRange = false;
            if (gameObject != null) { gameObject.SetActive(activeOnExit); }
            if (targetIsGUI == true)
            {
                HideUI();
            }
            if (changeStateOnExit) { 
                MainGameObject.Instance.interactText.SetActive(false);
            }

        }
    }

    private void Update()
    {
        if (delayInProgress)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= delay)
            {
                delayInProgress = false;
                ActivateObject();
                if (delay > 0) { isInRange = false;  }
            }
        }

        if (isInRange && Input.GetKeyDown(KeyCode.E) && interact)
        {
            

            if (delay <= 0f)
                ActivateObject();
            else
            {
                delayInProgress = true;
                delayTimer = 0f;
            }
        }
    }
    private void ShowUI()
    {
        if (playerController != null)
        {
            print("MOUSE LOOK CHANGED");
            playerController.m_MouseLook.SetCursorLock(false);
        }
    }

    private void HideUI()
    {

        if (playerController != null)
        {

            print("MOUSE LOOK CHANGED");
            playerController.m_MouseLook.SetCursorLock(true);
        }
    }

    private void ActivateObject()
    {
        if (isInRange == true)
        {
            delayInProgress = false;
            if (cost <= MainGameObject.Instance.money || cost == 0 || !costsMoney)
            {
                if (setGameIntKey == true)
                {
                    print("Trying to set" + gameIntKeyName + "to " + gameIntValue);
                    GIKS.Instance.SetGIKByName(gameIntKeyName, gameIntValue);
                }

                if (soundToPlay != "") { AudioManager.Instance.PlayAudio(soundToPlay); }
                if (gameObject != null) { gameObject.SetActive(activeOnEnter); }
                if (destroyOnEnter) { MainGameObject.Instance.interactText.SetActive(false); Destroy(this); }
                if (targetIsGUI == true)
                {
                    ShowUI();
                }
                if (startDialogue == true && dialogueRunner) { dialogueRunner.StartDialogue(yarnDialogueNode); }
            }
            if (requireGameIntKey == true)
            {
                print("GIK Required: " + gameIntKeyName + " value: " + gameIntValue.ToString());
                print("Which is currently: " + GIKS.Instance.GetGIKByName(gameIntKeyName).value.ToString());
                if (GIKS.Instance.GetGIKEqualsByName(gameIntKeyName, gameIntValue) == true)
                {
                    if (soundToPlay != "") { AudioManager.Instance.PlayAudio(soundToPlay); }
                    if (gameObject != null) { gameObject.SetActive(activeOnEnter); }
                    if (destroyOnEnter) { MainGameObject.Instance.interactText.SetActive(false); Destroy(this); }
                    if (targetIsGUI == true)
                    {
                        ShowUI();
                    }
                    if (startDialogue == true) { dialogueRunner.StartDialogue(yarnDialogueNode); }
                }
            }   
        }
        else
        {
            dialogueRunner.StartDialogue(yarnDialogueNodeTooPoor);
        }
    }
}
