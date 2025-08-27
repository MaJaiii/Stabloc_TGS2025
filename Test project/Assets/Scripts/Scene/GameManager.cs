using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GAME_STATE nowGameState = GAME_STATE.GAME_TITLE;

    [Header("Title")]
    [SerializeField, Tooltip("Ground parent object")] GameObject groundObj;
    [SerializeField, Tooltip("The block to instantiate")] GameObject cubePrefab;
    [SerializeField, Tooltip("Platform size (size x size)")] int size = 10;    
    [SerializeField, Tooltip("Distance between blocks")] float spacing = 1f; 
    [SerializeField, Tooltip(" Y coordinate (height)")] float yLevel = 0f;
    [SerializeField, Tooltip("Delay between each spawn")] float delay = 0.05f;
    [SerializeField] GameObject previewUI;
    List<GameObject> groundCubeGenerated;

    int cmdCount;
    float timer;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }

    private void OnApplicationQuit()
    {
        foreach (var pad in Gamepad.all)
        {
            pad.SetMotorSpeeds(0, 0);
            pad.ResetHaptics();   // Stops vibration
        }
            
    }

    private void OnDisable()
    {
        // Extra safety (in case scripts reload, playmode stops, etc.)
        foreach (var pad in Gamepad.all)
        {
            pad.SetMotorSpeeds(0, 0);
            pad.ResetHaptics();   // Stops vibration
        }
    }
    private void Start()
    {
        GameStatus.gameState = GAME_STATE.GAME_TITLE;
        ExecuteStateAction();
        cmdCount = 0;
        previewUI.SetActive(false);
    }

    void Update()
    {
        if (nowGameState != GameStatus.gameState) ExecuteStateAction();
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                cmdCount = 0;
            }
        }
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            cmdCount++;
            timer = 1;
            if (cmdCount == 3)
            {
                SceneManager.LoadScene("BackendScene");
            }
        }
    }

    void ExecuteStateAction()
    {
        GameStatus.gameState = nowGameState;
        switch (GameStatus.gameState)
        {
            case GAME_STATE.GAME_TITLE:
                StartCoroutine(FieldLightUpAction());
                break;
            case GAME_STATE.GAME_OVER:
                break;
            case GAME_STATE.GAME_READYTOPLAY:
                break;
            case GAME_STATE.GAME_INGAME:
                StartCoroutine(FieldShutDownAction());
                break;
        }
    }

    public void ChangeGameState(GAME_STATE gameState)
    {
        nowGameState = gameState;
    }

    IEnumerator FieldLightUpAction()
    {
        Vector2 gridCenter = new Vector2(size / 2f - 0.5f, size / 2f - 0.5f);
        Vector3 worldCenter = Vector3.zero;

        List<Vector2Int> gridPositions = new List<Vector2Int>();
        for (int x = 0; x < size; x++)
            for (int z = 0; z < size; z++)
                gridPositions.Add(new Vector2Int(x, z));

        gridPositions.Sort((a, b) =>
            Vector2.Distance(a, gridCenter).CompareTo(Vector2.Distance(b, gridCenter)));

        foreach (var pos in gridPositions)
        {
            float worldX = pos.x * spacing - gridCenter.x * spacing + worldCenter.x;
            float worldZ = pos.y * spacing - gridCenter.y * spacing + worldCenter.z;
            Vector3 worldPos = new Vector3(worldX, yLevel, worldZ);

            GameObject obj = Instantiate(cubePrefab, worldPos, Quaternion.identity);
            obj.tag = "Ground";
            obj.transform.parent = groundObj.transform;
            obj.GetComponent<MeshRenderer>().material.color = Color.white;
            yield return new WaitForSeconds(delay);
        }

        nowGameState = GAME_STATE.GAME_READYTOPLAY;
    }

    IEnumerator FieldShutDownAction()
    {
        if (groundObj.transform.childCount == 0)
            yield break;

        // Cache GameObjects instead of Transforms
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in groundObj.transform)
            children.Add(child.gameObject);

        // Remove colliders
        foreach (GameObject child in children)
        {
            DestroyImmediate(child.GetComponent<BoxCollider>());
        }


        // Destroy each GameObject with delay
        for (int i = children.Count - 1; i >= 0; i--)
        {
            Destroy(children[i]);
            yield return new WaitForSeconds(.001f);
        }
        previewUI.SetActive(true);
    }

}

public enum GAME_STATE
{
    GAME_OVER,
    GAME_INGAME,
    GAME_TITLE,
    GAME_READYTOPLAY,
    TRANSITIONING
}

public static class GameStatus
{
    public static GAME_STATE gameState;
}
