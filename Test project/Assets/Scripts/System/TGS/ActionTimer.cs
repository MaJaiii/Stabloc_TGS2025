using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionTimer : MonoBehaviour
{
    [SerializeField] Image[] overallTimer;
    [SerializeField] Image[] bonusTimer;
    [SerializeField] TextMeshProUGUI blockCountText;
    [SerializeField] public float timer;
    [SerializeField] BlockAction blockAction;
    [SerializeField] int maxTime;

    ScoreSystem scoreSystem;
    public int blockCount = 0;
    bool isRecovery = false;
    public bool isGameOver = false;
    int chain;

    private void Start()
    {
        scoreSystem = GetComponent<ScoreSystem>();
        Gamepad.current?.SetMotorSpeeds(0, 0);
    }

    private void Update()
    {
        if (GameStatus.gameState == GAME_STATE.TRANSITIONING)
        {
            isGameOver = true;
        }
        if (isGameOver) return;
        if (isRecovery)
        {
            timer += Time.deltaTime * 15;
            if (timer > maxTime)
            {
                timer = maxTime;
                isRecovery = false;
            }
        }
        else if (timer > 0 && (blockAction.flagStatus & FlagsStatus.PressDownButton) != FlagsStatus.PressDownButton)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                timer = 0;
                blockAction.flagStatus |= FlagsStatus.Drop;

            }
            if (Gamepad.current != null)
            {
                var gp = Gamepad.current;
                float motor = Mathf.Max(0, 5 - timer) / 5;
                gp.SetMotorSpeeds(motor, motor);
            }
        }
        else Gamepad.current?.SetMotorSpeeds(0, 0);
        for (int i = 0; i < 2; i++)
        {
            overallTimer[i].fillAmount = Mathf.Max(0, Mathf.Min(6, timer)) / 6;
            bonusTimer[i].fillAmount = Mathf.Max(0, Mathf.Min(maxTime, timer)) / maxTime;
            bonusTimer[i].color = blockAction.colorHistory[blockAction.colorHistory.Length - 1];
        }
        blockCountText.text = $"{blockCount} blocks placed";
    }

    public void RecoveryTimer()
    {
        isRecovery = true;
    }

    public void AddPoint(float height, int score = 0)
    {
        if (scoreSystem == null)
        {
            Debug.LogError("scoreSystem is not assigned!");
            return;
        }
        if (timer >= maxTime * .6f) chain++;
        else chain = 0;
        int gainScore = 0;
        if (score <= 0)
        {
            int timeScore = (int)Mathf.Ceil((timer) * 5);
            int heightScore = (int)Mathf.Ceil((height) * 2);
            if (timeScore == 0) heightScore = 0;
            int chainScore = (int)Mathf.Ceil((timeScore + heightScore) * 0.25f * (chain + 1));

            gainScore = timeScore + heightScore + chainScore;
        }
        else gainScore += score;

        scoreSystem.ModifyScore(gainScore);

    }
}
