using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Q3Movement;
using UnityEngine.SceneManagement;
public class PlayerData
{
    public string levelname;
    public float health;
    public int score;
    public Vector3 position;
    public List<GIK> GameIntKeys;
    public AudioManager am;
}

public class SaveManager : MonoBehaviour
{
    PlayerData playerData;
    string saveFilePath;
    private string saveName;
    private Q3Movement.Q3PlayerController playerController;
    public bool loadPending;
    public static SaveManager Instance { get; private set; }

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
            playerData = new PlayerData();
            saveFilePath = Application.persistentDataPath + "/" + saveName + ".json";
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SaveGame();
        print("SAVINGSAVINGSAVING");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
            SaveGame();

        if (Input.GetKeyDown(KeyCode.Alpha6))
            LoadGame();
    }

    public void SetPlayerdata(PlayerData data)
    {
        playerData = data;
    }
    public PlayerData GetPlayerdata()
    {
        return playerData;
    }
    public void SaveGame()
    {

        playerData.am = AudioManager.Instance;
        playerData.levelname = SceneManager.GetActiveScene().name;
        print(playerData.levelname);
        Debug.Log("Save file created at: " + saveFilePath);


        playerData.GameIntKeys = GIKS.Instance.GetAllGIKS();

        string savePlayerData = JsonUtility.ToJson(playerData);
        File.WriteAllText(saveFilePath, savePlayerData);
    }

    public void SetSaveName(string savename)
    {
        saveName = savename;
        saveFilePath = Application.persistentDataPath + "/" + savename + ".json";
    }
    public void SetSaveNameAndCreate(string savename)
    {
        saveName = savename;
        saveFilePath = Application.persistentDataPath + "/" + savename + ".json";
        SaveGame();
    }

    public void SetSaveNameAndLoad(string savename)
    {
        saveName = savename;
        saveFilePath = Application.persistentDataPath + "/" + savename + ".json";
        LoadGame();
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            loadPending = true;
            string loadPlayerData = File.ReadAllText(saveFilePath);
            playerData = JsonUtility.FromJson<PlayerData>(loadPlayerData);
            SceneManager.LoadScene(playerData.levelname);
            for (int i = 0; i < playerData.GameIntKeys.Count; i++) { 
                GIKS.Instance.SetGIK(i, playerData.GameIntKeys[i]);
            }
            loadPending = false;
        }
        
        else { 
            Debug.Log("There is no save files to load!");
            SceneManager.LoadScene(1);
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);

            Debug.Log("Save file deleted!");
        }
        else
            Debug.Log("There is nothing to delete!");
    }

    public void NewGame()
    {
        playerData.health = 100;
        playerData.score = 0;
    }
}