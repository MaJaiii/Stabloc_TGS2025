using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using DG.Tweening;
using Unity.VisualScripting;

public class GameTitle : MonoBehaviour
{
    [Header("UI Element")]
    [SerializeField] RectTransform logo_Base_L;
    [SerializeField] RectTransform logo_Base_R;
    [SerializeField] Image drop;
    [SerializeField] Image input_L_Move;
    [SerializeField] Image input_L_Camera;
    [SerializeField] Image input_R_Rotate;
    [SerializeField] Image input_R_Camera;
    [SerializeField] GameObject logo;

    [Header("Sprite")]
    [SerializeField] Sprite[] drop_Sprite;
    [SerializeField] Sprite[] input_L_Move_Sprite;
    [SerializeField] Sprite[] input_L_Camera_Sprite;
    [SerializeField] Sprite[] input_R_Rotate_Sprite;
    [SerializeField] Sprite[] input_R_Camera_Sprite;


    bool isReadyToPlay;
    bool isDropping;

    float timer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isReadyToPlay = false;  
        isDropping = false;
        drop.gameObject.SetActive(false);
        timer = 0;
        input_L_Move.transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(-700, -650, 0);
        input_R_Rotate.transform.parent.GetComponent<RectTransform>().localPosition = new Vector3(700, -650, 0);
    }

    // Update is called once per frame
    void Update()
    {
        int isGamepad = Gamepad.current != null ? 1: 0;
        drop.sprite = drop_Sprite[isGamepad];
        input_L_Move.sprite = input_L_Move_Sprite[isGamepad];
        input_L_Move.GetComponent<RectTransform>().localScale = Vector3.one * (.2f + .1f * isGamepad);
        input_L_Camera.sprite = input_L_Camera_Sprite[isGamepad];
        input_L_Camera.GetComponent<RectTransform>().localScale = Vector3.one * (.2f + .1f * isGamepad);
        input_R_Rotate.sprite = input_R_Rotate_Sprite[isGamepad];
        input_R_Rotate.GetComponent<RectTransform>().localScale = Vector3.one * (.2f + .1f * isGamepad);
        input_R_Camera.sprite = input_R_Camera_Sprite[isGamepad];
        input_R_Camera.GetComponent<RectTransform>().localScale = Vector3.one * (.2f + .1f * isGamepad);

        drop.SetNativeSize();
        input_L_Move.SetNativeSize();
        input_R_Rotate.SetNativeSize();

        if (!isReadyToPlay && GameStatus.gameState == GAME_STATE.GAME_READYTOPLAY)
        {
            isReadyToPlay = true;
            drop.gameObject.SetActive(true);
            input_L_Move.transform.parent.GetComponent<RectTransform>().DOLocalMoveX(-300, .7f);
            input_R_Rotate.transform.parent.GetComponent<RectTransform>().DOLocalMoveX(300, .7f);
        }

        if (!isDropping && GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            isDropping = true;
            drop.GetComponent<Animator>().SetBool("isFadeOut", true);
            input_L_Move.transform.parent.AddComponent<Rigidbody2D>().gravityScale = 10000;
            input_R_Rotate.transform.parent.AddComponent<Rigidbody2D>().gravityScale = 10000;
            logo_Base_L.DORotate(new Vector3(0, 0, -80), .2f);
            logo_Base_R.DORotate(new Vector3(0, 0, 80), .2f);
            for (int i = 0; i < logo.transform.childCount; i++)
            {
                logo.transform.GetChild(i).GetComponent<Rigidbody2D>().gravityScale = 10000;
            }
        }

        if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
        {
            timer += Time.deltaTime;
            if (timer > 3)
            {
                Destroy(logo_Base_R.transform.parent.gameObject);
            }
        }
    }
}
