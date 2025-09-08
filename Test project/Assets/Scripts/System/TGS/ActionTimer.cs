using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ActionTimer : MonoBehaviour
{
    [SerializeField] Image[] timerEdges;
    [SerializeField] TextMeshProUGUI blockCountText;
    [SerializeField] public float timer;
    [SerializeField] BlockAction blockAction;
    [SerializeField] int maxTime;
    [SerializeField] float fillAmount;

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
            timer += Time.deltaTime * maxTime * 1.5f;
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
            if (Gamepad.current != null && timer < 3)
            {
                var gp = Gamepad.current;
                float motor = Mathf.Max(0, 3 - timer) / 3;
                gp.SetMotorSpeeds(motor, motor);

                foreach (var image in timerEdges)
                {
                    if (timer % .5f > .25f) image.color = Color.red;
                    else image.color = Color.white;
                }
            }
        }
        else Gamepad.current?.SetMotorSpeeds(0, 0);

        fillAmount = (timer/maxTime) * 5920;

        int[] lengths = new int[] { 510, 1910, 1050, 1910, 540 };



        // Define segment sizes in the same order as your conditions
        int[] segmentSizes = { 510, 1910, 1050, 1910, 540 };

        // Reset all fills
        for (int i = 0; i < timerEdges.Length; i++)
            timerEdges[i].fillAmount = 0f;

        float remaining = fillAmount;

        // Loop through segments from last to first (because your original code started at index 4)
        for (int i = segmentSizes.Length - 1; i >= 0; i--)
        {
            if (remaining <= 0) break;

            if (remaining < segmentSizes[i])
            {
                timerEdges[i].fillAmount = remaining / (float)segmentSizes[i];
                break;
            }
            else
            {
                timerEdges[i].fillAmount = 1f;
                remaining -= segmentSizes[i];
            }
        }
        blockCountText.text = $"{blockCount} blocks placed";
    }

    public void RecoveryTimer()
    {
        isRecovery = true;
        foreach (var image in timerEdges)
        {
            image.color = Color.white;
        }
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
