using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BlindController : MonoBehaviour
{
    Image blind;
    private void Start()
    {
        blind = GetComponent<Image>();
        StartCoroutine(BlindFade());
    }

    IEnumerator BlindFade()
    {
        yield return new WaitForSeconds(.5f);

        blind.DOFade(0, 1.5f);

        yield return new WaitForSeconds(4.5f);

        blind.DOFade(1, 1.5f).OnComplete(() =>
        {
            SceneManager.LoadScene("SampleScene_Ma");
        });

    }
}
