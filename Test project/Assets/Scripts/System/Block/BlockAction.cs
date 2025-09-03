// Simplified and organized version of BlockAction.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BlockAction : MonoBehaviour
{
    #region Serialized Fields
    [Header("System")]
    [SerializeField] GameManager gameManager;
    [SerializeField] GameOver gameOver;
    [SerializeField] Vector2 rotateInput;

    [Header("Block Settings")]
    [SerializeField] float dropSpeed;
    [SerializeField] public Transform pivotObj;
    [SerializeField] GameObject cubePrefab;
    [SerializeField] GameObject weightedCubePrefab;
    [SerializeField] GameObject groundCubePrefab;
    [SerializeField] Material lineMaterial;
    [SerializeField] Transform ground;
    [SerializeField] CameraController cameraController;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField, Range(1, 18)] public int blockIndex;
    [SerializeField] NextBlockPreview nextBlockPreview;



    [Header("Audio")]
    [SerializeField]
    AudioClip[] audioClips;
    [SerializeField]
    AudioSource bgmSource;
    [SerializeField]
    int deltaFreq;


    [Header("TGS")]
    [SerializeField] ActionTimer actionTimer;
    #endregion



    #region Private Fields
    Rigidbody rb;
    InputController ic;
    GhostBlockPreview ghostSystem;
    AudioSource audioSource;
    List<GameObject> rootObj = new();
    List<Vector3> rootRotation = new();
    List<float> nowAngle = new();
    public List<Color> PentacubeColors = new() {
        Color.white,
        Color.red,
        Color.green,
        Color.yellow,
        Color.magenta,
        new Color(0f, 0.824f, 1),

    };

    // ★
    List<Color> PatternGlowColors = new() {
    new Color(0.5f, 1.0f, 0.3f), // Color.white に対応
    new Color(0.5f, 1.0f, 0.3f), // Color.redに対応
    new Color(0.5f, 1.0f, 0.3f),   // Color.green に対応
    new Color(1f, 0.890f, 0.451f),  // Color.yellow に対応
    new Color(1f, 0.486f, 1f),  // Color.magenta に対応
    new Color(0.52f, 0.976f, 1f), // new Color(0f, 0.824f, 1) に対応
    };

    Vector3 weightedBlockOffset;

    public FlagsStatus flagStatus;

    int lastGroundLevel = -4;
    bool isCreating = false;

    Vector3 moveInput;
    float nowHighestPoint;
    int colorCount;

    [SerializeField]
    int blockCount;
    float timer;
    public int[] blockHistory = new int[3] { -1, -1, -1 };
    public int[] colorHistory = new int[12];
    float moveInputCooldown;
    float rotateInputCooldown;


    #endregion

    #region BGM Related

    int totalPCMFrequency;
    int prevFreq;
    int nowFreq;

    #endregion

    #region Public Fields
    public int placedBlockCount;
    public int height;
    public Vector3 origin;
    public Vector3[] fillVertex;
    #endregion

    #region Unity Methods
    private void Start()
    {
        flagStatus = FlagsStatus.FirstDrop;

        fillVertex = new Vector3[2];

        Time.timeScale = 1;
        Cursor.visible = InGameSetting.isCursorVisible;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;

        ghostSystem = GetComponent<GhostBlockPreview>();
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = .001f * InGameSetting.masterVolume;

        blockCount = 0;
        totalPCMFrequency = bgmSource.clip.frequency * (int)bgmSource.clip.length;

        lastGroundLevel = -4;

        timer = 0;
    }

    private void Update()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if (GameStatus.gameState == GAME_STATE.GAME_READYTOPLAY && ((flagStatus & FlagsStatus.ReadyToPlay) != FlagsStatus.ReadyToPlay) )
        {
            flagStatus |= FlagsStatus.ReadyToPlay;
            gameManager.ChangeGameState(GAME_STATE.GAME_READYTOPLAY);

            ic = new();
            ic.Block.Move.performed += OnMove;
            ic.Block.Rotate.performed += OnRotate;
            ic.Block.Drop.started += OnDrop;
            ic.Camera.Switch.started += OnCameraSwitch;
            ic.Enable();

            timer = 1;
        }
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (moveInputCooldown > 0) moveInputCooldown-= Time.deltaTime;
        if (rotateInputCooldown > 0) rotateInputCooldown -= Time.deltaTime;

        if ((flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton)
            rb.MovePosition(rb.position + Vector3.down * dropSpeed * Time.deltaTime);

        Vector3 pos = rb.position;
        pos.x = Mathf.RoundToInt(pos.x); pos.z = Mathf.RoundToInt(pos.z);
        rb.position = pos;

        if (pivotObj == null)
        {
            var genPos = GetHighestPoint();
            genPos.y += 5;
            genPos.x = origin.x; genPos.z = origin.z;
            this.transform.position = genPos ;
        }
    }

    private void FixedUpdate()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if (audioSource && audioSource.clip)
        {
            if (audioSource.time / audioSource.clip.length >= .9)
            {
                audioSource.time = 0;
                audioSource.clip = null;
            }
        }
        if (pivotObj)
            CheckBlockConnectability();
        else
            do { HandleNewBlockSpawn(); } while (blockHistory[2] == -1);
        if (nowFreq != prevFreq)
        {
            prevFreq = nowFreq;
            StartCoroutine(Execution());
        }
        nowFreq = bgmSource.timeSamples - bgmSource.timeSamples % deltaFreq;

    }

    IEnumerator Execution()
    {
        if ((flagStatus & FlagsStatus.Drop) == FlagsStatus.Drop)
        {
            flagStatus &= ~FlagsStatus.Drop;
            if (ghostSystem.ghostBlock != null)
            {
                for (int i = 0; i < pivotObj.childCount; i++)
                {
                    if (pivotObj.GetChild(i).GetComponent<CheckCore>() != null)
                    {
                        pivotObj.GetChild(i).gameObject.layer = LayerMask.NameToLayer("Core");
                        break;
                    }
                }
                rb.constraints = RigidbodyConstraints.FreezePositionX & RigidbodyConstraints.FreezePositionZ & RigidbodyConstraints.FreezeRotation;

                if ((flagStatus & FlagsStatus.FirstDrop) == FlagsStatus.FirstDrop)
                {
                    flagStatus &= ~FlagsStatus.FirstDrop;
                    rb.DOMoveY((ghostSystem.ghostBlock.position + Vector3.up * .5f).y, 1f).SetEase(Ease.InSine);
                    Vector3 maxValue = pivotObj.position + new Vector3(2, 0, 2);
                    Vector3 minValue = pivotObj.position - new Vector3(2, 0, 2);
                    GameObject floor = new GameObject("Ground");
                    maxValue += Vector3.one;
                    for (int x = (int)minValue.x; x < maxValue.x; x++)
                    {
                        for (int z = (int)minValue.z; z < maxValue.z; z++)
                        {
                            GameObject go = Instantiate(groundCubePrefab, new Vector3(x, -4, z), Quaternion.identity);
                            go.transform.parent = floor.transform;
                            go.tag = "Ground";
                        }
                    }
                    origin = pivotObj.position;
                    GameStatus.fieldOrigin.x = Mathf.RoundToInt(origin.x); GameStatus.fieldOrigin.z = Mathf.RoundToInt(origin.z); GameStatus.fieldOrigin.y = Mathf.RoundToInt(origin.y);
                    cameraController.transform.DOMove(new Vector3(pivotObj.position.x, 3, pivotObj.position.z), .2f);
                    cameraController.startPos = pivotObj.position;
                    gameManager.ChangeGameState(GAME_STATE.GAME_INGAME);
                    floor.tag = "Ground";
                    floor.AddComponent<Rigidbody>().isKinematic = true;
                }
                else
                {
                    rb.transform.DOMoveY(rb.position.y - 3, .05f).SetEase(Ease.InSine);
                    Vector3 ghostPos = ghostSystem.ghostBlock.position;
                    yield return new WaitForSeconds(.05f);
                    rb.transform.position = ghostPos + Vector3.up * .5f;
                }

                flagStatus |= FlagsStatus.PressDownButton;
                flagStatus &= ~FlagsStatus.Move;
                flagStatus &= ~FlagsStatus.Rotate;
                flagStatus &= ~FlagsStatus.GenerateBlock;
                ghostSystem.isActive = false;
            }
        }
        else
        {
            if ((flagStatus & FlagsStatus.Move) == FlagsStatus.Move)
            {
                flagStatus &= ~FlagsStatus.Move;
            }
            if ((flagStatus & FlagsStatus.Rotate) == FlagsStatus.Rotate)
            {
                flagStatus &= ~FlagsStatus.Rotate;
            }
            if ((flagStatus & FlagsStatus.GenerateBlock) == FlagsStatus.GenerateBlock)
            {
                if (blockHistory[blockHistory.Length - 1] != -1 && !isCreating)
                {
                    GenerateBlock(blockHistory[blockHistory.Length - 1]);
                }
                flagStatus &= ~FlagsStatus.GenerateBlock;
            }

        }

    }

    private void LateUpdate()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if (rootObj.Count <= 0) return;

    }

    private void OnDestroy() 
    {
        ic.Block.Disable();
        ic.Camera.Disable();
        ic.Dispose(); 
    }

    private void OnCollisionStay(Collision collision)
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if (pivotObj == null) return;

        bool isReturn = true;

        foreach (ContactPoint contact in collision.contacts)
        {
            Vector3 normal = contact.normal;

            if (Vector3.Dot(normal, Vector3.up) > .9f) 
            {
                isReturn = false;
                break;
            }
        }
        if (isReturn) return;

        if (collision.gameObject.CompareTag("GameOver"))
        {
            gameOver.isGameOver = true;
            return;
        }
        if ((flagStatus & FlagsStatus.Connectable) != FlagsStatus.Connectable && !collision.gameObject.CompareTag("Ground")) return;
        if (pivotObj == null || collision == null || (!collision.gameObject.CompareTag("Ground") && !collision.gameObject.CompareTag("Placed") && (flagStatus & FlagsStatus.Connectable) != FlagsStatus.Connectable)) return;

        actionTimer.blockCount++;


        pivotObj.tag = "Placed";
        pivotObj.parent = null;
        pivotObj.GetComponent<BlockColor>().heightWhenSet = pivotObj.transform.position.y;
        pivotObj.GetComponent<BlockColor>().startNor = pivotObj.transform.up;
        pivotObj.GetComponent<BlockColor>().isGhost = false;
        pivotObj.GetComponent<BlockColor>().originPos = origin;
        for (int i = 0; i < pivotObj.childCount; i++)
        {
            Transform child = pivotObj.GetChild(i);
            child.GetComponent<BoxCollider>().enabled = true;
            child.GetComponent<MeshRenderer>().enabled = true;
            child.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            for (int j = 0; j < child.GetChild(1).childCount; j++)
            {
                ParticleSystem particle = child.GetChild(1).GetChild(j).GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.transform.rotation = Quaternion.Euler(90, 0, 0);
                    particle.transform.position = child.GetChild(1).transform.position - new Vector3(0, .5f, 0);

                    if ((Physics.Raycast(child.position + Vector3.down * .3f, Vector3.down, out RaycastHit hit, 1f) &&
                        (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Placed"))||
                        particle.gameObject.name.Contains("Box")))
                    {
                        Gradient gradient = new Gradient();
                        gradient.SetKeys(new GradientColorKey[]
                        {
                        new GradientColorKey(PentacubeColors[colorHistory[colorHistory.Length - 1]], 0f),
                        new GradientColorKey(PentacubeColors[colorHistory[colorHistory.Length - 1]], 1f)
                        },
                        new GradientAlphaKey[]
                        {
                        new GradientAlphaKey(1, 0),
                        new GradientAlphaKey(0, 1)
                        });
                        var col = particle.colorOverLifetime;
                        col.enabled = true;

                        col.color = new ParticleSystem.MinMaxGradient(gradient);
                      
                        particle.Play();
                    }
                }
            }
            child.tag = "Placed";
        }

        audioSource.clip = audioClips[0];
        audioSource.time = .5f;
        audioSource.Play();
        Rigidbody toRb = pivotObj.gameObject.AddComponent<Rigidbody>();
        cameraController.DropMotionCamera();
        if (fillVertex[0] != fillVertex[1])
        {
            Vector3 dir = fillVertex[1] - fillVertex[0];
            StartCoroutine(VibrateGamepad(.8f, .8f, 0.25f));
            if (Mathf.Abs(dir.y) < .2f)
            {
                GameObject[] placedObjs = GameObject.FindGameObjectsWithTag("Placed");
                foreach (var obj in placedObjs)
                {
                    if (obj.GetComponent<Rigidbody>() != null) obj.GetComponent<Rigidbody>().isKinematic = true;
                    if (obj.name.Contains("Cube") && obj != null && !obj.transform.parent.name.Contains("ReleasePt"))
                    {
                        if (Mathf.Abs(obj.transform.position.y - fillVertex[1].y) < .2f) Destroy(obj.gameObject);
                        else
                        {
                            RaycastHit[] hits = Physics.RaycastAll(obj.transform.position + Vector3.up, Vector3.down, 2);
                            foreach (var hit in hits)
                            {
                                if (hit.transform != obj.transform) continue;
                                if (Vector3.Dot(Vector3.up, hit.normal) > .05f)
                                {
                                    Destroy(obj.transform.parent.gameObject);
                                    break;
                                }
                            }
                            if (obj.transform.parent == null) continue;
                            obj.transform.parent.GetComponent<Rigidbody>().isKinematic = true;
                            obj.GetComponent<MeshRenderer>().material.color = Color.white;
                            obj.tag = "Ground";
                            if (obj.GetComponent<CheckCore>() != null)
                            {
                                Destroy(obj.GetComponent<CheckCore>());
                                Destroy(obj.GetComponent<BlockWeight>());
                                Destroy(obj.transform.GetChild(2).gameObject);  
                            }
                        }
                    }
                }
                pivotObj.GetComponent<Rigidbody>().isKinematic = true;
                GameObject newGroundParentObj = new GameObject("Ground");
                newGroundParentObj.tag = "Ground";
                StartCoroutine(CreatePlatform(Mathf.RoundToInt(fillVertex[1].y), newGroundParentObj.transform));
                newGroundParentObj.AddComponent<Rigidbody>().isKinematic = true;
            }
        }
        else
        {
            StartCoroutine(VibrateGamepad(0, .5f, 0.1f));
        }

        if (collision.gameObject.GetComponent<BlockColor>() != null && pivotObj.GetComponent<BlockColor>().blockColor == collision.gameObject.GetComponent<BlockColor>().blockColor)
        {
            Rigidbody fromRb = collision.transform.parent ? collision.transform.parent.GetComponent<Rigidbody>() : collision.gameObject.GetComponent<Rigidbody>();
            toRb.interpolation = RigidbodyInterpolation.Interpolate;
            HingeJoint hinge = toRb.gameObject.AddComponent<HingeJoint>();
            hinge.connectedBody = fromRb;
            hinge.anchor = Vector3.zero;
            hinge.axis = Vector3.up;
            hinge.useLimits = true;
            hinge.limits = new JointLimits { min = 0, max = 0 };
            hinge.enablePreprocessing = false;
            hinge.enableCollision = false;
            pivotObj = null;
        }
        else
        {
            pivotObj = null;
            if (collision.gameObject.CompareTag("Ground"))
            {
                rootRotation.Add(new Vector3(toRb.transform.rotation.x, toRb.transform.rotation.y, toRb.transform.rotation.z));
                nowAngle.Add(0);
                rootObj.Add(toRb.gameObject);
            }
            else
            {
                toRb.isKinematic = false;
                toRb.mass = .2f;
                toRb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                toRb.angularDamping = 5;
                toRb.automaticCenterOfMass = false;
                toRb.centerOfMass = weightedBlockOffset;
            }
        }

        actionTimer.RecoveryTimer();
        actionTimer.AddPoint(height + 3.5f);
        rb.constraints = RigidbodyConstraints.None;
    }

    IEnumerator CreatePlatform(int height, Transform parent)
    {
        isCreating = true;
        for (int i = height + 6; i > lastGroundLevel; i--)
        {

            if (i <= height)
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("Placed");
                foreach (var obj in objs)
                    if (Mathf.Abs(obj.transform.position.y - i) < .5f && obj.name.Contains("Cube") && obj != null)
                        Destroy(obj);
                objs = GameObject.FindGameObjectsWithTag("Ground");
                foreach (var obj in objs)
                    if (Mathf.Abs(obj.transform.position.y - i) < .5f && obj.name.Contains("Cube") && obj != null)
                        Destroy(obj);
                for (float x = fillVertex[0].x; x <= fillVertex[1].x; x++)
                {
                    for (float z = fillVertex[0].z; z <= fillVertex[1].z; z++)
                    {
                        GameObject floor = Instantiate(groundCubePrefab, new Vector3(x, i, z), Quaternion.identity, parent);
                        floor.GetComponent<MeshRenderer>().material.color = Color.white;
                        floor.tag = "Ground";
                    }
                }
                yield return new WaitForSeconds(.05f);
            }
            else
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("Ground");
                if (objs.Length == 0) continue;
                foreach (var obj in objs)
                {
                    BlockFade blockFade = obj.GetComponent<BlockFade>();
                    if (blockFade == null || !obj.name.Contains("Cube")) continue;
                    if (Mathf.Abs(obj.transform.position.y - i) < .5f)
                    {
                        if (true)
                        {
                            GameObject floor = Instantiate(groundCubePrefab, blockFade.worldPos, Quaternion.identity, parent);
                            floor.GetComponent<MeshRenderer>().material.color = Color.white;
                            floor.tag = "Ground";
                        }
                        Destroy(obj);
                    }
                }
            }

        }
        lastGroundLevel = height;
        isCreating = false;
        yield return null;
    }
    #endregion


    #region Input Callbacks
    void OnCameraSwitch(InputAction.CallbackContext context)
    {
        if ((flagStatus & FlagsStatus.Collapse) != FlagsStatus.Collapse && (flagStatus & FlagsStatus.CameraRotate) != FlagsStatus.CameraRotate)
        {
            cameraController.SwitchCamera(context.ReadValue<Vector2>().x > 0);
            flagStatus |= FlagsStatus.CameraRotate;
        }

    }

    void OnMove(InputAction.CallbackContext context)
    {
        if (pivotObj == null) return;
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || (flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || moveInputCooldown > 0 || context.ReadValue<Vector2>().magnitude < .5f) return;

        Vector2 input = context.ReadValue<Vector2>();
        Vector3 moveDir = Vector3.zero;

        if (Mathf.Abs(input.x) > .2f && Mathf.Abs(input.y) <= .2f)
            moveDir = new Vector3(Mathf.Sign(input.x), 0, -Mathf.Sign(input.x));
        else if (Mathf.Abs(input.y) > .2f && Mathf.Abs(input.x) <= .2f)
            moveDir = new Vector3(Mathf.Sign(input.y), 0, Mathf.Sign(input.y));
        else
        {
            if (input.x < 0)
            {
                if (input.y < 0) moveDir.x = -1;
                else moveDir.z = 1;
            }
            else
            {
                if (input.y < 0) moveDir.z = -1;
                else moveDir.x = 1;
            }
        }

        float tempX = moveDir.x;
        float tempY = moveDir.z;

        switch (cameraController.cameraIndex)
        {
            case 0: moveDir = new Vector3(tempX, 0, tempY); break;
            case 1: moveDir = new Vector3(-tempY, 0, tempX); break;
            case 2: moveDir = new Vector3(-tempX, 0, -tempY); break;
            case 3: moveDir = new Vector3(tempY, 0, -tempX); break;
        }

        if (GameStatus.gameState == GAME_STATE.GAME_READYTOPLAY)
        {
            if ((moveDir.x > 0 && transform.position.x >= 3) || (moveDir.x < 0 && transform.position.x <= -3)) moveDir.x = 0;
            if ((moveDir.z > 0 && transform.position.z >= 3) || (moveDir.z < 0 && transform.position.z <= -3)) moveDir.z = 0;
        }

        // Preview where it will move
        moveInput = rb.position + moveDir;

        flagStatus |= FlagsStatus.Move;
        moveInputCooldown = .1f;
        moveInput = new Vector3Int(Mathf.RoundToInt(moveInput.x), Mathf.RoundToInt(moveInput.y), Mathf.RoundToInt(moveInput.z));
        transform.position = moveInput;
        // Manually preview ghost at the new position
        if (ghostSystem != null && ghostSystem.pivotObj != null)
        {
            ghostSystem.pivotObj.position = moveInput;
            ghostSystem.UpdateGhostPosition();
        }

    }

    void OnDrop(InputAction.CallbackContext context)
    {
        if ((flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || (flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || pivotObj == null) return;
        ic.Block.Disable();
        flagStatus |= FlagsStatus.Drop;
        
    }

    void OnRotate(InputAction.CallbackContext context)
    {
        if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || (flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || rotateInputCooldown > 0 || context.ReadValue<Vector2>().magnitude < .5f) return;

        Vector2 readValue = context.ReadValue<Vector2>();
        if (readValue.magnitude <= .5f) return;
        Vector3 axis = Vector3.zero;
        if (Mathf.Abs(readValue.y) <= .25f) //horizontal
        {
            axis.y = 1 ;
        }
        else 
        {
            switch (cameraController.cameraIndex)
            {
                case 0:
                    if (Mathf.Sign(readValue.x) == Mathf.Sign(readValue.y)) axis.z = -1; else axis.x = -1;
                    break;
                case 1:
                    if (Mathf.Sign(readValue.x) == Mathf.Sign(readValue.y)) axis.x = 1; else axis.z = -1;
                    break;
                case 2:
                    if (Mathf.Sign(readValue.x) == Mathf.Sign(readValue.y)) axis.z = 1; else axis.x = 1;
                    break;
                case 3:
                    if (Mathf.Sign(readValue.x) == Mathf.Sign(readValue.y)) axis.x = -1; else axis.z = 1;
                    break;
            }
        }
        rotateInputCooldown = .2f;
        BlockDORotateAround(axis, .2f, Mathf.Sign(readValue.x));

    }
    #endregion

    #region Block Management

    float prevValue;
    void RotateAroundPrc(float value, Vector3 axis)
    {
        float delta = value - prevValue;
        transform.RotateAround(transform.position, axis, delta);
        prevValue = value;
    }

    Tween BlockDORotateAround(Vector3 axis, float duration, float sign)
    {
        Tween ret;
        float endValue = 0;
        if (Mathf.Abs(axis.x) >= 1)
        {
            prevValue = transform.eulerAngles.x;
            endValue = transform.eulerAngles.x + 90 * sign;
        }
        else if (Mathf.Abs(axis.y) >= 1)
        {
            prevValue = transform.eulerAngles.y;
            endValue = transform.eulerAngles.y + 90 * sign;
        }
        else if (Mathf.Abs(axis.z) >= 1)
        {
            prevValue = transform.eulerAngles.z;
            endValue = transform.eulerAngles.z + 90 * sign;
        }
        else return null;
        ret = DOTween.To(x => RotateAroundPrc(x, axis), prevValue, endValue, .1f).OnComplete(() => ghostSystem?.UpdateGhostPosition()).OnUpdate(ghostSystem.UpdateGhostPosition);

        return ret;
    }

    public void TowerCollapse()
    {
        if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || blockCount <= 0) return;
        flagStatus |= FlagsStatus.Collapse;
        if (ghostSystem != null) ghostSystem.isActive = false;

        foreach (var b in GameObject.FindGameObjectsWithTag("Placed"))
        {
            var hinge = b.GetComponent<HingeJoint>();
            if (hinge != null)
            {
                hinge.enableCollision = true;
                Destroy(hinge);
            }
        }

        Destroy(pivotObj?.gameObject);
        audioSource.PlayOneShot(audioClips[1]);
        cameraController.SwitchCamera(true);
        gameOver.isGameOver = true;
    }

    void CheckBlockConnectability()
    {
        if ((flagStatus & FlagsStatus.Connectable) == FlagsStatus.Connectable || pivotObj.position.y < -4)
        {
            foreach (Transform child in pivotObj)
                child.GetComponent<Collider>().enabled = true;
            flagStatus |= FlagsStatus.Connectable;
        }
        else
        {

            Vector3[] offsets = new Vector3[]
            {
                Vector3.zero,
                new Vector3(.3f, 0, .3f),
                new Vector3(-.3f, 0, .3f),
                new Vector3(.3f, 0, -.3f),
                new Vector3(-.3f, 0, -.3f),
            };
            foreach (Transform child in pivotObj)
            {
                foreach (var offset in offsets)
                {
                    if (Physics.Raycast(child.position + offset, Vector3.down, out RaycastHit hit, 2f) &&
                        (hit.collider.CompareTag("Placed") || hit.collider.CompareTag("Ground")))
                    {
                        flagStatus |= FlagsStatus.Connectable;
                        break;
                    }
                }
            }
        }
    }

    int[] GenerateColor(int[] array)
    {
        int[] count = new int[4] { 3, 3, 3, 3 };
        int[] tempOrder;
        int fishStart = 0;
        if (array[0] == 0)
        {
            tempOrder = new int[12] { 2, 3, 4, 5, 2, 3, 4, 5, 2, 3, 4, 5 };
        }
        else
        {
            tempOrder = new int[12];
            for (int i = tempOrder.Length - 1; i > 0; i--)
            {
                if (array[i] != 0)
                {
                    tempOrder[i] = array[i];
                    count[array[i - 2]]--;
                    fishStart++;
                    continue;
                }
                for (int j = 0; j < count.Length; j++)
                {
                    if (count[j] <= 0) continue;
                    tempOrder[i] = j + 2;
                    count[j]--;
                }
            }
        }

        for (int i = tempOrder.Length - fishStart - 1; i > 0; i--)
        {
            int temp = tempOrder[i];
            int r = UnityEngine.Random.Range(0, i + 1);
            tempOrder[i] = tempOrder[r];
            tempOrder[r] = temp;
        }
        return tempOrder;
    }

    void HandleNewBlockSpawn()
    {
        timer += Time.deltaTime;
        if (timer > 0.75f && (flagStatus & FlagsStatus.Collapse) != FlagsStatus.Collapse && (flagStatus & FlagsStatus.GenerateBlock) != FlagsStatus.GenerateBlock)
        {
            int tempIndex = 0;
            placedBlockCount = blockCount;
            blockCount++;
            colorCount = (colorCount + 1) % PentacubeColors.Count;
            if (colorCount <= 1) colorCount = 2;

            bool isHistoryIncluded = true;

            while (isHistoryIncluded)
            {
                isHistoryIncluded = false;
                if (blockCount <= 1)
                    tempIndex = UnityEngine.Random.Range(5, 10);
                else if (blockCount <= 3)
                    tempIndex = UnityEngine.Random.Range(2, 11);
                else if (blockCount == 4)
                    tempIndex = UnityEngine.Random.Range(11, 19);
                else
                    tempIndex = UnityEngine.Random.Range(1, 19);

                foreach (var h in blockHistory)
                {
                    if (h == tempIndex)
                    {
                        isHistoryIncluded = true;
                        break;
                    }
                }
            }

            for (int i = colorHistory.Length - 1; i > 0; i--)
            {
                colorHistory[i] = colorHistory[i - 1];
                if (i < blockHistory.Length) blockHistory[i] = blockHistory[i - 1];
            }


            blockHistory[0] = tempIndex; colorHistory[0] = 0;
            timer = 0;
            if (blockHistory[blockHistory.Length - 1] != -1) flagStatus |= FlagsStatus.GenerateBlock;
        }
        if (colorHistory[colorHistory.Length - blockHistory.Length] == 0) colorHistory = GenerateColor(colorHistory);
    }




    public void GenerateBlock(int index)
    {
        if (pivotObj) Destroy(pivotObj.gameObject);

        for (int i = 0; i < fillVertex.Length; i++)
        {
            fillVertex[i] = Vector3.zero;
        }

        for (int i = 0;  i < blockHistory.Length; i++)
        {
            nextBlockPreview.GeneratePreviewBlock(i, blockHistory[i]);
        }
        Block3DType blockType = (Block3DType)index;
        Vector3 genPos = cameraController.transform.position;
        if (genPos.y < lastGroundLevel + 8) genPos.y = lastGroundLevel + 8;
        else genPos.y = GetHighestPoint(true).y;
        cameraController.MoveCamera(genPos);        
        genPos.y += 5;


        transform.position = RoundOffVec3(genPos);
        transform.rotation = Quaternion.identity;


        pivotObj = new GameObject($"Block {blockCount}").transform;
        pivotObj.parent = transform;
        pivotObj.localPosition = Vector3.zero;
        BlockColor pivotColor = pivotObj.AddComponent<BlockColor>();
        pivotColor.blockAction = this;
        pivotColor.blockColor = PentacubeColors[colorHistory[colorHistory.Length - 1]];
        int cubeCount = 0;
        foreach (Vector3 offset in PentacubeShapes.Shapes[blockType])
        {
            GameObject obj;
            if (offset == Vector3.zero && blockCount % 3 != 0)
            {
                obj = Instantiate(weightedCubePrefab, transform.position + offset, Quaternion.identity, pivotObj);
                weightedBlockOffset = offset;
            }
            else
            {
                obj = Instantiate(cubePrefab, transform.position + offset, Quaternion.identity, pivotObj);
            }

            // ★
            // BlockGlowControllerをobjに追加
            BlockGlowController glowController = obj.AddComponent<BlockGlowController>();

            // オフセットを色に基づいて計算
            float colorOffset = GetBlinkOffsetForColor(PentacubeColors[colorHistory[colorHistory.Length - 1]]);

            // マテリアルのプロパティを個別に設定
            obj.GetComponent<MeshRenderer>().material.SetColor("_BaseColor", PentacubeColors[colorHistory[colorHistory.Length - 1]]);
            int colorIndex = PentacubeColors.IndexOf(PentacubeColors[colorHistory[colorHistory.Length - 1]]);
            if (colorIndex >= 0 && colorIndex < PatternGlowColors.Count)
            {
                Color patternColor = PatternGlowColors[colorIndex];
                obj.GetComponent<MeshRenderer>().material.SetColor("_GlowColor", patternColor);
            }

            // glowControllerのメソッドを呼び出し、オフセット値を渡す
            glowController.SetBlinkOffset(colorOffset);

            obj.GetComponent<BoxCollider>().enabled = false;
            // --------------------------------
            obj.GetComponent<MeshRenderer>().enabled = true;
            cubeCount++;
        }


        if (ghostSystem != null && index != 0)
        {
            ghostSystem.pivotObj = pivotObj;
            ghostSystem.isActive = true;
            ghostSystem.CreateGhost(pivotObj.gameObject, pivotColor.blockColor);
            ghostSystem.UpdateGhostPosition();
        }

        flagStatus &= ~FlagsStatus.PressDownButton;
        flagStatus &= ~FlagsStatus.Connectable;
        flagStatus &= ~FlagsStatus.GenerateBlock;
        if (ic != null) ic.Block.Enable();
    }

    // ★
    // 色ごとに点滅のタイミングを変更
    private float GetBlinkOffsetForColor(Color color)
    {
        if (color == Color.green) return 0f;
        if (color == Color.yellow) return 15f;
        if (color == Color.magenta) return 27f;
        if (color == new Color(0.0f, 0.947f, 1.0f)) return 35f;
        return 6.0f; // デフォルト値
    }

    Vector3 RoundOffVec3(Vector3 value)
    {
        Vector3 result;
        result.x = Mathf.Round(value.x);
        result.y = Mathf.Round(value.y);
        result.z = Mathf.Round(value.z);
        return result;
    }

    public Vector3 GetHighestPoint(bool update = false)
    {
        Vector3 minPos = Vector3.positiveInfinity;
        Vector3 maxPos = Vector3.negativeInfinity;
        float highestY = -3.5f;
        Transform topObj = null;

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Placed"))
        {
            if (!obj.activeInHierarchy) continue;
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (!mr) continue;
            float topY = mr.bounds.max.y;
            if (topY > highestY)
            {
                highestY = topY;
                topObj = obj.transform.parent;
            }
        }
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground"))
        {
            if (!obj.activeInHierarchy) continue;
            MeshRenderer mr = obj.GetComponent<MeshRenderer>();
            if (!mr) continue;
            float topY = mr.bounds.max.y;
            if (topY > highestY)
            {
                highestY = topY;
                topObj = obj.transform.parent;
            }
        }

        if (highestY > -3.5f && highestY + 3.5f > PlayerPrefs.GetFloat("highScore"))
            PlayerPrefs.SetFloat("highScore", highestY + 3.5f);

        if (highestY > nowHighestPoint && update)
        {
            nowHighestPoint = highestY;
        }

        if (topObj == null) return Vector3.down * 3.5f;

        for (int i = 0; i < topObj.childCount; i++)
        {
            MeshRenderer mr = topObj.GetChild(i).GetComponent<MeshRenderer>();
            if (!mr) continue;
            if (mr.bounds.max.x > maxPos.x) maxPos.x = mr.bounds.max.x;
            if (mr.bounds.min.x < minPos.x) minPos.x = mr.bounds.min.x;
            if (mr.bounds.max.z > maxPos.z) maxPos.z = mr.bounds.max.z;
            if (mr.bounds.min.z < minPos.z) minPos.z = mr.bounds.min.z;
        }


        if (height < highestY) height = (int)highestY;

        return new Vector3((maxPos.x + minPos.x) / 2, Mathf.Max(0, highestY), (maxPos.z + minPos.z) / 2);
    }
    #endregion

    IEnumerator VibrateGamepad(float rate_L, float rate_R, float duration)
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(rate_L, rate_R);
            if (duration > 0) yield return new WaitForSeconds(duration);
            else yield return new WaitForEndOfFrame();
            Gamepad.current.SetMotorSpeeds(0, 0);
        }
    }

}

[Flags]
public enum FlagsStatus
{
    None = 0,
    Connectable = 1 << 0,
    PressDownButton = 1 << 1,
    Collapse = 1 << 2,
    FirstDrop = 1 << 3,
    ReadyToPlay = 1 << 4,

    Drop = 1 << 5,
    Rotate = 1 << 6,
    GenerateBlock = 1 << 7,
    Move = 1 << 8,
    CameraRotate = 1 << 9,
}