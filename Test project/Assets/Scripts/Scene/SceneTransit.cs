using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneTransit : MonoBehaviour
{
    [Header("Target Scene")]
    [SerializeField] string sceneName;

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.anyKeyDown) SceneManager.LoadScene(sceneName);
    }
}
