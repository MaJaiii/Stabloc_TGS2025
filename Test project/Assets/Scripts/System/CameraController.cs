using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraObj;
    [SerializeField] Transform lookAtObj;
    [SerializeField] Transform ground;
    [SerializeField] Transform directionalLight;
    [SerializeField] Transform[] points;
    [SerializeField] BlockAction blockAction;
    public int cameraIndex = 0;


    public Vector3 startPos = Vector3.zero;

    float prevValue;

    Tween DoRotateAround(float endValue, float duration)
    {
        prevValue = cameraObj.eulerAngles.y;

        Tween ret = DOTween.To(x => RotateAroundPrc(x), cameraObj.eulerAngles.y, endValue, duration).OnComplete(() => blockAction.flagStatus &= ~FlagsStatus.CameraRotate);

        return ret;
    }


    void RotateAroundPrc(float value)
    {
        float delta = value - prevValue;

        cameraObj.RotateAround(new Vector3(startPos.x, cameraObj.position.y, startPos.z), Vector3.up, delta);

        prevValue = value;
    }
    private void Start()
    {
        cameraObj.position = points[0].position;
        cameraObj.rotation = points[0].rotation;
    }


    public void SwitchCamera(bool isPos, bool wideView = false)
    {

        if (wideView)
        {
            cameraObj.DOMove(new Vector3(0, 0, -15), .1f).SetEase(Ease.OutSine);
            cameraObj.DORotate(Vector3.zero, .1f);
            return;
        }
        Vector3 angle = cameraObj.rotation.eulerAngles;
        Vector3 startAngle = angle;
        if (isPos)
        {
            cameraIndex++;
            cameraIndex %= points.Length;
            angle.y -= 90;
        }
        else
        {
            if (cameraIndex == 0) cameraIndex = points.Length;
            cameraIndex--;
            cameraIndex %= points.Length;
            angle.y += 90;
        }

        DoRotateAround(angle.y, .2f);

    }

    public void MoveCamera(Vector3 pos, float yDiff = 0)
    {
        if (yDiff < 0) yDiff = 0;
        if (pos.y > 1 && pos.y > transform.position.y)
        {
            transform.DOMoveY((pos.y - 1), .3f).SetEase(Ease.OutSine);
            //float tempR = cameraObj.localRotation.eulerAngles.y;
            //Debug.Log(yDiff);

            //// Clamp to prevent domain errors in Asin
            //float safeYDiff = Mathf.Clamp(yDiff, -1f, 1f);

            //// Convert Asin from radians Å® degrees
            //float angleX = 10f + Mathf.Asin(safeYDiff) * Mathf.Rad2Deg;

            //// Apply only if result is valid
            //if (!float.IsNaN(angleX))
            //{
            //    cameraObj.localRotation = Quaternion.Euler(angleX, tempR, 0f);
            //}
        }
        return;
    }

}
