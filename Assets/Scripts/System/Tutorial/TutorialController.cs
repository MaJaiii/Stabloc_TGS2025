using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class TutorialController : MonoBehaviour
{
    [Header("General Setting")]
    [SerializeField] public int step = -1;
    [SerializeField] GameObject tutorial_targetPosDisplayPrefab;
    GameObject tutorial_targetPosDisplayParent;
    [SerializeField] GameObject displayParentObj;
    [SerializeField] Image displayImageObj;
    [SerializeField] Image displayImageBaseObj;
    [SerializeField] TMP_Text displayTextObj_JP;
    [SerializeField] TMP_Text displayTextObj_EN;

    [Header("Audio")]
    [SerializeField] AudioClip taskClearSE;
    [SerializeField] AudioClip tutorialCompleteSE;

    [Header("Entries")]
    public int index;
    public List<TutorialEntry> entries;

    [HideInInspector] public bool isReady;
	[HideInInspector] public bool isActive;

	[HideInInspector] public T_Flags flag;

    GameSettings settings;
    float currentSPB;
    int lastIndex;

	private void Start()
	{
        settings = CsvSettingsLoader.Load();
        lastIndex = 0;
        flag = T_Flags.None;
        AudioController.Instance.OnBeat += ShowNextStep;
        AudioController.Instance.OnBeat += HideNowStep;
	}

	private void Update()
    {
        if (step < 0 || step >= entries.Count || (flag & T_Flags.Done) == T_Flags.Done) return;
        currentSPB = 60 / AudioController.Instance.bpm;
        float fillAmount = (float)index / entries[step].targetIndex;
        if (displayImageObj.fillAmount != fillAmount && !isFilling) FillUpImage(fillAmount);

        if (index >= entries[step].targetIndex && isReady && (flag & T_Flags.Hide) != T_Flags.Hide)
        {
            isReady = false;
            AudioController.Instance.PlaySFX(taskClearSE, settings.masterVolume);
			if (tutorial_targetPosDisplayParent != null) Destroy(tutorial_targetPosDisplayParent);
            transform.DOPunchScale(Vector3.one, currentSPB, 1, 1).OnComplete(() => flag |= T_Flags.Hide);
        }

        else if (isReady && !isOnShow)
        {
            if (displayParentObj.GetComponent<RectTransform>().localPosition.x != -320)
            {
                var nowPos = displayParentObj.GetComponent<RectTransform>().localPosition;
                nowPos.x = -320;
                displayParentObj.GetComponent<RectTransform>().localPosition = nowPos;
            }
            if (step == 11)
            {
                if (index > lastIndex)
                {
                    lastIndex = index;
                    if (tutorial_targetPosDisplayParent != null) Destroy(tutorial_targetPosDisplayParent);
                    if (entries[step].targetPos.Count > 0)
                    {
                        tutorial_targetPosDisplayParent = new GameObject("targetPosDisplayParent");
                        for (int i = index * 5; i < (index + 1) * 5; i++)
                        {
                            var pos = entries[step].targetPos[i];
                            Instantiate(tutorial_targetPosDisplayPrefab, pos, Quaternion.identity, tutorial_targetPosDisplayParent.transform);
                        }
                    }
                }
            }
        }
    }

    bool isFilling = false;

    void FillUpImage(float targetAmount)
    {
        if (isFilling) return;
        isFilling = true;
        displayImageObj.DOFillAmount(targetAmount, .1f).SetEase(Ease.Linear).OnComplete(() => isFilling = false);
    }

    public void HideNowStep(int beat)
    {
        if ((flag & T_Flags.Hide) != T_Flags.Hide) return;
        displayParentObj.GetComponent<RectTransform>().DOLocalMoveX(-900, currentSPB).SetEase(Ease.InSine).OnComplete(() => {
            if (beat != -1)  
            {
                flag |= T_Flags.Show; flag &= ~T_Flags.Hide; 
            } 
            else
            
            {
                return;
            }
        });
        isFilling = false;
    }

    bool isOnShow = false;

    public void ShowNextStep(int beat)
    {
		if ((flag & T_Flags.Show) != T_Flags.Show || isOnShow) return;
        step++;
        if (step >= entries.Count)
        {
            flag = T_Flags.Done;
			AudioController.Instance.OnBeat -= ShowNextStep;
			AudioController.Instance.OnBeat -= HideNowStep;
			AudioController.Instance.PlaySFX(tutorialCompleteSE, settings.masterVolume);
			return;
        }
		isOnShow = true;
        InputMethod inputMethod;

        if (Gamepad.current == null) inputMethod = InputMethod.Keyboard;
        else inputMethod = GameStatus.gamepadMode == 0 ? InputMethod.Gamepad : InputMethod.Gamepad_ver2;

		displayImageBaseObj.sprite = entries[step].GetSprite(inputMethod);
        displayImageObj.sprite = displayImageBaseObj.sprite;
        displayImageObj.fillMethod = entries[step].fillMethod;
        switch (step)
        {
            case 2:
                displayImageObj.fillClockwise = false; break;
            case 3:
                displayImageObj.fillClockwise = true; break;
            default:
                break;
        }
        index = 0;
		displayTextObj_JP.text = "";
        displayTextObj_JP.font = LanguageManager.Instance.GetFont(Language.Japanese);
        displayTextObj_EN.text = "";
        displayTextObj_EN.font = LanguageManager.Instance.GetFont(Language.English);
        displayImageBaseObj.fillAmount = 0;
        displayImageObj.fillAmount = 0;
        displayParentObj.GetComponent<RectTransform>().DOLocalMoveX(-320, 0).OnComplete(() =>
        {
            StartCoroutine(ShowNextStepCoroutine());
        });
    }
    IEnumerator ShowNextStepCoroutine()
    {
        var str_JP = entries[step].GetText(Language.Japanese) ?? "";
        var str_EN = entries[step].GetText(Language.English) ?? "";


		for (int i = 0; i < str_EN.Length; i++)
		{
			displayTextObj_EN.text += str_EN[i];
			yield return new WaitForSeconds(currentSPB / str_EN.Length);
		}

		for (int i = 0 ; i < str_JP.Length; i++)
        {
            displayTextObj_JP.text += str_JP[i];
            yield return new WaitForSeconds(currentSPB / str_JP.Length);
		}


		displayImageBaseObj.fillAmount = 1;
		if (entries[step].targetPos.Count > 0)
		{
			tutorial_targetPosDisplayParent = new GameObject("targetPosDisplayParent");
            for (int i = index * 5; i < (index + 1) * 5; i++)
            {
                var pos = entries[step].targetPos[i];
                Instantiate(tutorial_targetPosDisplayPrefab, pos, Quaternion.identity, tutorial_targetPosDisplayParent.transform);
            }
        }
        index = 0;
        isReady = true;
        isOnShow = false;
        flag = T_Flags.None;
	}

}


[System.Serializable]
public class TutorialEntry
{
    public Image.FillMethod fillMethod;
    public List<SpriteDisplay> sprites = new List<SpriteDisplay>();

    public List<LocalizedText> texts = new List<LocalizedText>();

    public int targetIndex;
    public List<Vector3> targetPos = new List<Vector3>();

    public Sprite GetSprite(InputMethod inputMethod)
    {
        var display = sprites.Find(i => i.method == inputMethod);
        return display != null ? display.sprite : null;
    }

    public string GetText(Language lang)
    {
        var caption = texts.Find(l => l.language == lang);
        return caption != null ? caption.text : null;
    }
}

public enum InputMethod
{
    Keyboard, Gamepad, Gamepad_ver2
}

[System.Serializable]
public class SpriteDisplay
{
    public InputMethod method;
    public Sprite sprite;
}

[Flags]
public enum T_Flags
{
    None = 0,
    Hide = 1 << 0,
    Show = 1 << 1,
    Done = 1 << 2,
}