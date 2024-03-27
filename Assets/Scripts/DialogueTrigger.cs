using UnityEngine;
using Q3Movement;
using Yarn.Unity;
public class DialogueTrigger : MonoBehaviour
{
    private Q3PlayerController playerController;
    public GameObject gameObject;  // btw this is very dangerous, should be renamed to something else prob
    public bool activeOnEnter = true;
    public bool activeOnExit = true;
    public bool changeStateOnEnter = true;
    public bool changeStateOnExit = false;
    public bool destroyOnEnter = false;
    public bool interact = false;
    private bool isInRange = false;
    public string interactTextString = "'E'";
    public string soundToPlay = "";
    public float delay = 0f; // New variable for delay
    private bool delayInProgress = false;
    private float delayTimer = 0f;
    public bool requireGameIntKey = false;
    public int gameIntKey = 0;
    public byte gameIntValue = 0;
    public bool targetIsGUI = false;
    private ADGS_Dialogue diag;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && changeStateOnEnter)
        {
            playerController = other.GetComponent<Q3PlayerController>();
            if (targetIsGUI == true && gameObject.GetComponent<ADGS_Dialogue>()) 
            {
                diag = gameObject.GetComponent<ADGS_Dialogue>();
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

    private void OnTriggerExit(Collider other)
    {
        MainGameObject.Instance.interactText.SetActive(false);
        if (other.CompareTag("Player") && changeStateOnExit)
        {
            isInRange = false;
            gameObject.SetActive(activeOnExit);
            if (targetIsGUI == true)
            {
                HideUI();
            }
            MainGameObject.Instance.interactText.SetActive(false);
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
            playerController.m_MouseLook.SetCursorLock(false);
        }
    }

    private void HideUI()
    {

        if (playerController != null)
        {
            playerController.m_MouseLook.SetCursorLock(true);
        }
    }

    private void ActivateObject()
    {
        
        if (requireGameIntKey == true)
        {
            if (MainGameObject.Instance.getGameIntKeyEquals(gameIntKey, gameIntValue) == true) {
                if (soundToPlay != "") { AudioManager.Instance.PlayAudio(soundToPlay); }
                gameObject.SetActive(activeOnEnter);
                if (destroyOnEnter) { Destroy(this); }
                if (targetIsGUI == true)
                {
                    ShowUI();
                }
            }
        }

        else { 
            if (soundToPlay != "") { AudioManager.Instance.PlayAudio(soundToPlay); }
            gameObject.SetActive(activeOnEnter);
            if (targetIsGUI == true)
            {
                ShowUI();
            }
            if (destroyOnEnter) { Destroy(this); }
        }
    }
}
