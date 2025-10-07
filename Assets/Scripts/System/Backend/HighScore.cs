using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighScore : MonoBehaviour
{
    [Header("HighScore")]
    [SerializeField] float history;
    [SerializeField] float current;
    void Start()
    {
        history = PlayerPrefs.GetFloat("highScore");
        transform.position = new Vector3(0, history - 3.5f, 0);
    }

    // Update is called once per frame
    void Update()
    {
        current = Mathf.Max(history, PlayerPrefs.GetFloat("highScore"));
    }
}
