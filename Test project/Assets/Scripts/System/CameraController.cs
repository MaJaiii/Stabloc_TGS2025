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
    [SerializeField] Transform camera;
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
        prevValue = camera.eulerAngles.y;

        Tween ret = DOTween.To(x => RotateAroundPrc(x), camera.eulerAngles.y, endValue, duration).OnComplete(() => blockAction.flagStatus &= ~FlagsStatus.CameraRotate);

        return ret;
    }


    void RotateAroundPrc(float value)
    {
        float delta = value - prevValue;

        camera.RotateAround(new Vector3(startPos.x, camera.position.y, startPos.z), Vector3.up, delta);

        prevValue = value;
    }
    private void Start()
    {
        camera.position = points[0].position;
        camera.rotation = points[0].rotation;
    }


    public void SwitchCamera(bool isPos, bool wideView = false)
    {

        if (wideView)
        {
            camera.DOMove(new Vector3(0, 0, -15), .3f).SetEase(Ease.OutSine);
            camera.DORotate(Vector3.zero, .3f);
            return;
        }
        Vector3 angle = camera.rotation.eulerAngles;
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

        DoRotateAround(angle.y, .5f);

    }

    public void MoveCamera(Vector3 pos)
    {
        if (pos.y > 3) transform.DOMoveY((pos.y - 3), .3f).SetEase(Ease.OutSine);
        return;
    }

}
