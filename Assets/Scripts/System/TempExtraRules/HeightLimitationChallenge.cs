using UnityEngine;
using DG.Tweening;

public class HeightLimitationChallenge : MonoBehaviour
{
    int nowChallengeIndex;

    int blockCount;
    float nowHeight;

    [SerializeField]
    int targetBlockCount;
    [SerializeField]
    float targetHeightLimitation;


    [SerializeField]
    GameObject targetHeightVisualize;
    void Start()
    {
        blockCount = 0;
        nowChallengeIndex = 0;
        nowHeight = transform.position.y;
        GenerateNextChallenge();
    }

    // Update is called once per frame
    void Update()
    {
        targetHeightVisualize.transform.position = Vector3.zero + Vector3.up * targetHeightLimitation;
        if (nowHeight > targetHeightLimitation)
        {
            GenerateNextChallenge(false);
        }
        else if (blockCount >= targetBlockCount)
        {
            GenerateNextChallenge();
        }
        else
        {
            Color color = Color.yellow;
            color.a = .1f;
            targetHeightVisualize.GetComponent<MeshRenderer>().material.color = color;

        }
    }

    void GenerateNextChallenge(bool isCompleted = true)
    {
        if (!isCompleted) nowChallengeIndex = 0;
        targetHeightLimitation = nowHeight + 2 + nowChallengeIndex;
        targetBlockCount = 5 + nowChallengeIndex * 2;
        blockCount = 0;
        nowChallengeIndex++;
    }

    public void CountBlock (float height, int value = 1)
    {
        blockCount += value;
        nowHeight = height;
    }
}
