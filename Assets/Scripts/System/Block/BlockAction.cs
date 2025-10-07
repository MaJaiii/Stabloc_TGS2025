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
    [SerializeField] public GameOver gameOver;
    [SerializeField] GameObject gameOverTrigger;
    [SerializeField] Vector2 rotateInput;
    [SerializeField] GameObject gameTitleCanvas;
    [SerializeField] bool isMuteBGM = false;

    [Header("Block Settings")]
    [SerializeField] float dropSpeed;
    [SerializeField] public Transform pivotObj;
    [SerializeField] GameObject cubePrefab;
    [SerializeField] GameObject weightedCubePrefab;
    [SerializeField] GameObject groundCubePrefab;
    [SerializeField] GameObject showCornerPrefab;
    [SerializeField] Material lineMaterial;
    [SerializeField] Transform ground;
    [SerializeField] CameraController cameraController;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField, Range(1, 18)] public int blockIndex;
    [SerializeField] NextBlockPreview nextBlockPreview;
    [SerializeField]  BeaconLineAnimation beaconLineAnimation;

    [Header("Audio")]
    [SerializeField] AudioClip titleBGM;
    [SerializeField] float titleBGM_BPM;
	[SerializeField] AudioClip tutorialBGM;
	[SerializeField] float tutorialBGM_BPM;
	[SerializeField] AudioClip ingameBGM;
    [SerializeField] float ingameBGM_BPM;
    [SerializeField] AudioClip startSE;
    [SerializeField] AudioClip generateSE;
    [SerializeField] AudioClip moveSE;
    [SerializeField] AudioClip rotationSE;
    [SerializeField] AudioClip dropSE;
    [SerializeField] AudioClip connectSE;
    [SerializeField] public AudioClip chainSE;
    [SerializeField] public AudioClip groundSE;


    [Header("TGS")]
    [SerializeField] ActionTimer actionTimer;

    [Header("Tutorial")]
    [SerializeField] public bool tutorial = false;
    [SerializeField] TutorialController tutorialController;
    [SerializeField] GameObject tutorialObj;
    [SerializeField, Tooltip("12 data for tutorial block array")] int[] t_blockArray;
    [SerializeField, Tooltip("12 data for tutorial color array")] int[] t_colorArray;
    [SerializeField] int count;
    int[] dir_mark = new int[8];
    #endregion



    #region Private Fields
    Rigidbody rb;
    InputController ic;
    public GhostBlockPreview ghostSystem;
    List<GameObject> rootObj = new();
    List<Vector3> rootRotation = new();
    List<float> nowAngle = new();
    bool cameraPositive;
    GameSettings settings;

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

	[SerializeField] private LineAnimation m_lineAnimation;

	Vector3 weightedBlockOffset;

    public FlagsStatus flagStatus;

    public int lastGroundLevel = -4;
    bool isCreating = false;

    Vector3 moveInput;
    float nowHighestPoint;
    int colorCount;

    [SerializeField]
    int blockCount;
    float timer;
    public int[] blockHistory = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
    public int[] colorHistory = new int[12];
    float moveInputCooldown;
    float rotateInputCooldown;
    public Vector3 droppingPos;

    #endregion

    #region Public Fields
    public int placedBlockCount;
    public int height;
    public Vector3 origin;
    public Vector3[] fillVertex;
    public bool isAnim = false;
    #endregion

    #region Unity Methods
    private void Start()
    {
        flagStatus = FlagsStatus.FirstDrop;

        fillVertex = new Vector3[2];

        settings = CsvSettingsLoader.Load();

        Time.timeScale = 1;
        Cursor.visible = !settings.isCursorHidden;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = false;

        ghostSystem = GetComponent<GhostBlockPreview>();

        AudioController.Instance.PlayBGM(titleBGM, isMuteBGM ? 0 : settings.masterVolume, 1, titleBGM_BPM);
        AudioController.Instance.OnBar += Initialize;

        blockCount = 0;
        blockHistory = t_blockArray;
        colorHistory = t_colorArray;

        lastGroundLevel = -4;

        timer = 0;
    }

    void Initialize(int bar)
    {
        if (bar < 1) return;
		if (GameStatus.gameState == GAME_STATE.GAME_READYTOPLAY && ((flagStatus & FlagsStatus.ReadyToPlay) != FlagsStatus.ReadyToPlay))
		{
			flagStatus |= FlagsStatus.ReadyToPlay;
			gameManager.ChangeGameState(GAME_STATE.GAME_READYTOPLAY);

			AudioController.Instance.OnBeat += ExecutionOnBeat;
			AudioController.Instance.OnBar += ExecutionOnBar;

			ic = new();
			ic.Block.Move.performed += OnMove;
			ic.Block.Rotate.performed += OnRotate;
			ic.Block.Drop.started += OnDrop;
			ic.Block.Spin.performed += OnSpin;
			ic.Camera.Switch.started += OnCameraSwitch;
			ic.Tutorial.SkipTutorial.performed += OnTutorialSkip;
			ic.Enable();
           
            AudioController.Instance.OnBar -= Initialize;
			timer = 1;
		}
	}

    private void Update()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER)
        {
			return;
		}

        if (tutorial)
        {
            if (tutorialController != null && !tutorialController.isReady)
            {
                dir_mark = new int[6];
                count = 0;
            }
            if (tutorialController.step == tutorialController.entries.Count)
            {
                tutorialController.step = 999;
                actionTimer.RecoveryTimer();
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (moveInputCooldown > 0) moveInputCooldown -= Time.deltaTime;
        if (rotateInputCooldown > 0) rotateInputCooldown -= Time.deltaTime;

        if ((flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton)
        {
            if (Vector3.Distance(transform.position, droppingPos) > .5f)
            {
                transform.position += (Vector3.down * dropSpeed * Time.deltaTime);
            }
            else
            {
                transform.position += (Vector3.down * 2 * Time.deltaTime);
            }
        }
        

        Vector3 pos = rb.position;
        pos.x = Mathf.RoundToInt(pos.x); pos.z = Mathf.RoundToInt(pos.z);

        if (pivotObj == null)
        {
            var genPos = GetHighestPoint();
            genPos.y += 5;
            genPos.x = origin.x; genPos.z = origin.z;
            this.transform.position = genPos;
        }
    }

    private void FixedUpdate()
    {
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if (pivotObj)
            CheckBlockConnectability();
        else
            do { HandleNewBlockSpawn(); } while (blockHistory[2] == -1);


    }

    void ExecutionOnBar(int bar)
    {
		if (isCreating || isAnim) return;
		if (bar % 4 == 2 && pivotObj != null)
        {
            int maxY = Mathf.CeilToInt(GetHighestPoint().y);
            List<CheckCore> corePos = new List<CheckCore>();
            bool[] isCore = new bool[maxY - lastGroundLevel];
            bool[] isBlock = new bool[isCore.Length];
            for (int i = 0; i < isBlock.Length; i++)
            {
                isCore[i] = false;
                isBlock[i] = false;
            }
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Placed"))
            {
                BlockFade bf = obj.GetComponent<BlockFade>();
                if (bf == null || bf?.worldPos.y - lastGroundLevel < 0) continue;
                if (Mathf.Abs(bf.worldPos.x - origin.x) == 2 && Mathf.Abs(bf.worldPos.z - origin.z) == 2)
                {
                    if (obj.GetComponent<CheckCore>() == null)
                    {
                        isBlock[bf.worldPos.y - lastGroundLevel] = true;
                        isCore[bf.worldPos.y - lastGroundLevel] = false;
                        continue;
                    }
                    else if (!isBlock[bf.worldPos.y - lastGroundLevel])
                    {
                        isCore[bf.worldPos.y - lastGroundLevel] = true;
                        corePos.Add(obj.GetComponent<CheckCore>());
                    }
                }
            }
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Ground"))
            {
                BlockFade bf = obj.GetComponent<BlockFade>();
                if (bf == null || bf?.worldPos.y - lastGroundLevel < 0) continue;
                if (Mathf.Abs(bf.worldPos.x - origin.x) == 2 && Mathf.Abs(bf.worldPos.z - origin.z) == 2)
                {
                    isBlock[bf.worldPos.y - lastGroundLevel] = true;
                    isCore[bf.worldPos.y - lastGroundLevel] = false;
                }
            }

            for (int i = 0; i < isCore.Length; i++)
            {
                if (isBlock[i] || !isCore[i]) continue;
                Transform showCornerParent = new GameObject($"showCornerParent_y: {i + lastGroundLevel}").transform;
                showCornerParent.tag = "ShowCorner";
                showCornerParent.position = new Vector3(origin.x, i + lastGroundLevel, origin.z);

				// 親オブジェクトにBlockColorコンポーネントを追加
				BlockColor parentBlockColor = showCornerParent.gameObject.AddComponent<BlockColor>();
				parentBlockColor.blockAction = this;
				parentBlockColor.blockColor = PentacubeColors[colorHistory[colorHistory.Length - 1]];

				Vector3[] offsets = new Vector3[] { new Vector3(-2, 0, -2), new Vector3(2, 0, -2), new Vector3(2, 0, 2), new Vector3(-2, 0, 2) };
                foreach (var core in corePos)
                {
                    if (core.pos.y != i + lastGroundLevel) continue;
                    for (int j = 0; j < offsets.Length; j++)
                    {
                        if (core.pos == showCornerParent.position + offsets[j]) offsets[j] = Vector3.zero;
                    }
                }

                foreach (var offset in offsets)
                {
                    if (offset == Vector3.zero) continue;
                    Instantiate(showCornerPrefab, showCornerParent.position + offset, Quaternion.identity, showCornerParent);
                }
            }

        }
        else if (bar % 4 == 0)
        {
            GameObject[] showCornerParent = GameObject.FindGameObjectsWithTag("ShowCorner");
            foreach (var obj in showCornerParent) Destroy(obj);
        }
    }

    void ExecutionOnBeat(int beat)
    {
        if (isCreating || isAnim) return;
		if (tutorial && (tutorialController.flag & T_Flags.Done) == T_Flags.Done)
		{
			tutorial = false;
            if (GameStatus.gameState == GAME_STATE.GAME_INGAME)
            {
				AudioController.Instance.PlaySFX(startSE, settings.masterVolume * 5);
				AudioController.Instance.PlayBGM(ingameBGM, isMuteBGM ? 0 : settings.masterVolume, 1, ingameBGM_BPM);
			}
		}
		if ((flagStatus & FlagsStatus.CameraRotate) == FlagsStatus.CameraRotate)
        {
            flagStatus &= ~FlagsStatus.CameraRotate;
        }
        if ((flagStatus & FlagsStatus.Drop) == FlagsStatus.Drop)
        {
            flagStatus &= ~FlagsStatus.Drop;
            GameObject[] gos = GameObject.FindGameObjectsWithTag("ShowCorner");
            foreach (var obj in gos)
            {
                Destroy(obj);
            }
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
                    rb.transform.position = droppingPos + Vector3.up * .3f;

                }



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
            if ((flagStatus & FlagsStatus.GenerateBlock) == FlagsStatus.GenerateBlock && (flagStatus & FlagsStatus.Drop) != FlagsStatus.Drop)
            {
                if (tutorial)
                {
                    if (tutorialController.flag == T_Flags.None && tutorialController.isReady) GenerateBlock(blockHistory[blockHistory.Length - 1]);
				}
                else if (blockHistory[blockHistory.Length - 1] != -1)
                {
                    GenerateBlock(blockHistory[blockHistory.Length - 1]);
                }
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

    public int lastPlacedCorePos_y;

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

        AudioController.Instance.PlaySFX(dropSE, settings.masterVolume * 1f);

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
            //child.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            for (int j = 0; j < child.GetChild(0).childCount; j++)
            {
                ParticleSystem particle = child.GetChild(0).GetChild(j).GetComponent<ParticleSystem>();
                if (particle != null)
                {
                    particle.transform.rotation = Quaternion.Euler(90, 0, 0);
                    particle.transform.position = child.GetChild(0).transform.position - new Vector3(0, .5f, 0);

                    if ((Physics.Raycast(child.position + Vector3.down * .3f, Vector3.down, out RaycastHit hit, 1f) &&
                        (hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Placed")) ||
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

                        // 再生終了後に自動削除するためのコンポーネントを付与
                        if (particle.gameObject.GetComponent<ParticleManager>() == null)
                        {
                            particle.gameObject.AddComponent<ParticleManager>();
                        }


                        particle.transform.parent = null;
                    }
                    
                }
            }
            child.tag = "Placed";
        }
        Rigidbody toRb = pivotObj.gameObject.AddComponent<Rigidbody>();

        //0 => Max; 1 => Min
        if (fillVertex[0] != fillVertex[1] && (fillVertex[0].x != fillVertex[1].x || fillVertex[0].z != fillVertex[1].z))
        {
            Vector3 dir = fillVertex[1] - fillVertex[0];
            StartCoroutine(VibrateGamepad(.8f, .8f, 0.5f));
            if (true)
            {

                GameObject[] placedObjs = GameObject.FindGameObjectsWithTag("Placed");
                foreach (var obj in placedObjs)
                {
                    if (obj.GetComponent<Rigidbody>() != null) obj.GetComponent<Rigidbody>().isKinematic = true;
                    if ((obj.name.Contains("Cube") && obj != null && !obj.transform.parent.name.Contains("ReleasePt")))
                    {
                        if (obj.transform.position.y < fillVertex[0].y && obj.transform.position.y >= fillVertex[1].y) Destroy(obj.gameObject);
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
                int y = -9999;
				// ★
				Vector3 lastPlacedCorePos = Vector3.zero;
				for (int i = 0; i < pivotObj.childCount; i++)
                {
                    CheckCore core = pivotObj.GetChild(i).GetComponent<CheckCore>();
                    if (core == null) continue;
                    y = Mathf.RoundToInt(core.pos.y);
					// ★
					lastPlacedCorePos = core.pos;
					break;
                }
				// ★
				if (m_lineAnimation != null)
				{
					m_lineAnimation.lineanimation_pattern = GetLineAnimationPattern(lastPlacedCorePos, fillVertex);
				}
				lastPlacedCorePos_y = y;
                if (y != -9999)
                {
					GameObject newGroundParentObj = new GameObject("Ground");
					newGroundParentObj.tag = "Ground";
					StartCoroutine(CreatePlatform(y, newGroundParentObj.transform));
					newGroundParentObj.AddComponent<Rigidbody>().isKinematic = true;
				}
            }
        }
        else
        {
            StartCoroutine(VibrateGamepad(0, .5f, 0.3f));
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
            AudioController.Instance.PlaySFX(connectSE, settings.masterVolume * 3);
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

        if (tutorial)
        {
            tutorialController.index++;
        }
        else
        {
            actionTimer.blockCount++;
            if (actionTimer.AddPoint(height + 3.5f)) AudioController.Instance.PlaySFX(chainSE, settings.masterVolume * 5);
        }
        rb.constraints = RigidbodyConstraints.None;
        flagStatus &= ~FlagsStatus.PressDownButton;
    }

   

    IEnumerator CreatePlatform(int height, Transform parent)
    {
		isCreating = true;
		int lastheight = lastGroundLevel;
		lastGroundLevel = Mathf.RoundToInt(fillVertex[1].y);

		yield return new WaitForSeconds(2f);

        int diff = Mathf.RoundToInt(Mathf.Max(Mathf.Abs(height - lastheight), Mathf.Abs(height - fillVertex[1].y) + 6));
		for (int i = 0; i <= diff; i++)
        {
            int y = height + i;
            if (y <= fillVertex[1].y)
            {
				GameObject[] objs = GameObject.FindGameObjectsWithTag("Placed");
				foreach (var obj in objs)
					if (Mathf.Abs(obj.transform.position.y - y) < .5f && obj.name.Contains("Cube") && obj != null)
						Destroy(obj);
				objs = GameObject.FindGameObjectsWithTag("Ground");
				foreach (var obj in objs)
					if (Mathf.Abs(obj.transform.position.y - y) < .5f && obj.name.Contains("Cube") && obj != null)
						Destroy(obj);
				for (float x = fillVertex[0].x; x <= fillVertex[1].x; x++)
				{
					for (float z = fillVertex[0].z; z <= fillVertex[1].z; z++)
					{
						GameObject floor = Instantiate(groundCubePrefab, new Vector3(x, y, z), Quaternion.identity, parent);
						floor.GetComponent<MeshRenderer>().material.color = Color.white;
						floor.tag = "Ground";
					}
				}
			}
            else
            {
                GameObject[] objs = GameObject.FindGameObjectsWithTag("Ground");
                if (objs.Length == 0) continue;
                foreach (var obj in objs)
                {
                    BlockFade blockFade = obj.GetComponent<BlockFade>();
                    if (blockFade == null || !obj.name.Contains("Cube")) continue;
                    if (blockFade.worldPos.y == y)
                    {
                        if (blockFade.worldPos.x >= fillVertex[0].x && blockFade.worldPos.z >= fillVertex[0].z &&
							blockFade.worldPos.x <= fillVertex[1].x && blockFade.worldPos.z <= fillVertex[1].z)
                        {
                            GameObject floor = Instantiate(groundCubePrefab, blockFade.worldPos, Quaternion.identity, parent);
                            floor.GetComponent<MeshRenderer>().material.color = Color.white;
                            floor.tag = "Ground";
                        }
                        Destroy(obj);
                    }
                }
            }
			y = height - i;
            if (y >= lastheight && i != 0)
            {
				GameObject[] objs = GameObject.FindGameObjectsWithTag("Placed");
				foreach (var obj in objs)
					if (Mathf.Abs(obj.transform.position.y - y) < .5f && obj.name.Contains("Cube") && obj != null)
						Destroy(obj);
				objs = GameObject.FindGameObjectsWithTag("Ground");
				foreach (var obj in objs)
					if (Mathf.Abs(obj.transform.position.y - y) < .5f && obj.name.Contains("Cube") && obj != null)
						Destroy(obj);
				for (float x = fillVertex[0].x; x <= fillVertex[1].x; x++)
				{
					for (float z = fillVertex[0].z; z <= fillVertex[1].z; z++)
					{
						GameObject floor = Instantiate(groundCubePrefab, new Vector3(x, y, z), Quaternion.identity, parent);
						floor.GetComponent<MeshRenderer>().material.color = Color.white;
						floor.tag = "Ground";
					}
				}
			}
            yield return new WaitForSeconds(0.1f);
        }
		
		//for (int i = height + 6; i > lastheight; i--)
		//      {

		//          if (i <= height)
		//          {
		//              GameObject[] objs = GameObject.FindGameObjectsWithTag("Placed");
		//              foreach (var obj in objs)
		//                  if (Mathf.Abs(obj.transform.position.y - i) < .5f && obj.name.Contains("Cube") && obj != null)
		//                      Destroy(obj);
		//              objs = GameObject.FindGameObjectsWithTag("Ground");
		//              foreach (var obj in objs)
		//                  if (Mathf.Abs(obj.transform.position.y - i) < .5f && obj.name.Contains("Cube") && obj != null)
		//                      Destroy(obj);
		//              for (float x = fillVertex[0].x; x <= fillVertex[1].x; x++)
		//              {
		//                  for (float z = fillVertex[0].z; z <= fillVertex[1].z; z++)
		//                  {
		//                      GameObject floor = Instantiate(groundCubePrefab, new Vector3(x, i, z), Quaternion.identity, parent);
		//                      floor.GetComponent<MeshRenderer>().material.color = Color.white;
		//                      floor.tag = "Ground";
		//                  }
		//              }
		//              yield return new WaitForSeconds(.05f);
		//          }
		//          else
		//          {
		//              GameObject[] objs = GameObject.FindGameObjectsWithTag("Ground");
		//              if (objs.Length == 0) continue;
		//              foreach (var obj in objs)
		//              {
		//                  BlockFade blockFade = obj.GetComponent<BlockFade>();
		//                  if (blockFade == null || !obj.name.Contains("Cube")) continue;
		//                  if (Mathf.Abs(obj.transform.position.y - i) < .5f)
		//                  {
		//                      if (true)
		//                      {
		//                          GameObject floor = Instantiate(groundCubePrefab, blockFade.worldPos, Quaternion.identity, parent);
		//                          floor.GetComponent<MeshRenderer>().material.color = Color.white;
		//                          floor.tag = "Ground";
		//                      }
		//                      Destroy(obj);
		//                  }
		//              }
		//          }

		//      }
		isCreating = false;
        fillVertex = new Vector3[2];
        yield return null;
    }
	// ★
	private int GetLineAnimationPattern(Vector3 corePosition, Vector3[] fillVertex)
	{
		// fillVertexの4つの頂点から、グリッドの中心を計算
		Vector3 center = Vector3.zero;
		foreach (var vertex in fillVertex)
		{
			center += vertex;
		}
		center /= fillVertex.Length;

		// Coreの位置をグリッドの中心からの相対的な位置として判断
		float relativeX = corePosition.x - center.x;
		float relativeZ = corePosition.z - center.z;

		if (relativeX < 0 && relativeZ < 0)
		{
			return 1; // -x, -z (左下)
		}
		else if (relativeX > 0 && relativeZ < 0)
		{
			return 2; // +x, -z (右下)
		}
		else if (relativeX > 0 && relativeZ > 0)
		{
			return 3; // +x, +z (右上)
		}
		else if (relativeX < 0 && relativeZ > 0)
		{
			return 4; // -x, +z (左上)
		}
		else
		{
			// 中心線上など、予期せぬ位置の場合
			return 0;
		}
	}
	#endregion


	#region Input Callbacks
	void OnCameraSwitch(InputAction.CallbackContext context)
    {
        if ((flagStatus & FlagsStatus.Collapse) != FlagsStatus.Collapse && (flagStatus & FlagsStatus.CameraRotate) != FlagsStatus.CameraRotate)
        {
            if (tutorial)
            {
				if (!tutorialController.isActive || !tutorialController.isReady) return;
				int sign = Mathf.RoundToInt(Mathf.Sign(context.ReadValue<Vector2>().x));
                if (tutorialController.step < 7) return;
                if (tutorialController.step == 8)
                {
                    if (sign <= 0) return;
                    count++;
                    tutorialController.index = Mathf.Abs(count);
                }
                else if (tutorialController.step == 7)
                {
                    if (sign >= 0) return;
                    count++;
                    tutorialController.index = Mathf.Abs(count);
                }
            }
            cameraPositive = context.ReadValue<Vector2>().x > 0;
            cameraController.SwitchCamera(cameraPositive);
            flagStatus |= FlagsStatus.CameraRotate;
        }

    }

    void OnTutorialSkip(InputAction.CallbackContext context)
    {
        if (!tutorial) return;
		tutorialController.flag = T_Flags.Done;
		tutorialController.HideNowStep(-1);
        tutorialController.flag = T_Flags.Done;
		if (GameStatus.gameState == GAME_STATE.GAME_INGAME) actionTimer.RecoveryTimer();
        Destroy(tutorialObj);
        ic.Tutorial.SkipTutorial.performed -= OnTutorialSkip;
	}


    void OnMove(InputAction.CallbackContext context)
    {
        if (pivotObj == null) return;
        if (tutorial)
        {
            if (!tutorialController.isActive || !tutorialController.isReady) return;
		}
        if (GameStatus.gameState == GAME_STATE.GAME_TITLE || GameStatus.gameState == GAME_STATE.GAME_OVER) return;
        if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || (flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || moveInputCooldown > 0 || context.ReadValue<Vector2>().magnitude < .5f) return;

        Vector2 input = context.ReadValue<Vector2>();
        Vector3 moveDir = Vector3.zero;
        if (input.magnitude > .2f)
        {
			if (Mathf.Abs(input.x) > .2f && Mathf.Abs(input.y) <= .2f)
			{
				moveDir = new Vector3(Mathf.Sign(input.x), 0, -Mathf.Sign(input.x));
				if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[0] = 1;
			}
			else if (Mathf.Abs(input.y) > .2f && Mathf.Abs(input.x) <= .2f)
			{
				moveDir = new Vector3(Mathf.Sign(input.y), 0, Mathf.Sign(input.y));
				if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[1] = 1;
			}
			else
			{
				if (input.x < 0)
				{
					if (input.y < 0) { moveDir.x = -1; if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[2] = 1; }
					else { moveDir.z = 1; if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[3] = 1; }
				}
				else
				{
					if (input.y < 0) { moveDir.z = -1; if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[4] = 1; }
					else { moveDir.x = 1; if (tutorialController.step == 0 && tutorialController.isReady) dir_mark[5] = 1; }
				}
			}
			if (tutorial && tutorialController.step == 0 && tutorialController.isReady)
			{
				count = 0;
				for (int i = 0; i < dir_mark.Length; i++)
				{
					count += dir_mark[i];
				}
				tutorialController.index = count;

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
			AudioController.Instance.PlaySFX(moveSE, settings.masterVolume * .6f);
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
    }

    void OnDrop(InputAction.CallbackContext context)
    {
        if (tutorial)
        {
            if (!tutorialController.isActive)
            {
                gameTitleCanvas?.SetActive(false);
                tutorialController.isActive = true;
                tutorialController.flag |= T_Flags.Show;
                AudioController.Instance.PlayBGM(tutorialBGM,settings.masterVolume, 1, tutorialBGM_BPM);
                return;
            }
			if (!tutorialController.isActive || !tutorialController.isReady) return;
			if (tutorialController.step < 9) return;
            int tempCount = 0;
            List<Vector3> targetPos = tutorialController.entries[tutorialController.step].targetPos;

            for (int i = 0; i < ghostSystem.ghostBlock.childCount; i++)
            {
                var child = ghostSystem.ghostBlock.GetChild(i).GetComponent<BlockFade>();
                if (child == null) continue;
                for (int j = 0; j < targetPos.Count; j++)
                {
                    if (child.worldPos == targetPos[j])
                    {
                        tempCount++;
                        break;
                    }
                }
            }
            if (tempCount < 5) return;
        }
        if ((flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || (flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || pivotObj == null) return;
        ic.Block.Disable();
        droppingPos = ghostSystem.ghostBlock.position;
        if (GameStatus.gameState != GAME_STATE.GAME_INGAME && (tutorialController.step >= tutorialController.entries.Count || !tutorial))
        {
            AudioController.Instance.PlaySFX(startSE, settings.masterVolume * 5);
            AudioController.Instance.PlayBGM(ingameBGM, isMuteBGM ? 0 : settings.masterVolume, 1, ingameBGM_BPM);
        }
        flagStatus |= FlagsStatus.PressDownButton;
        flagStatus |= FlagsStatus.Drop;

    }

    void OnSpin(InputAction.CallbackContext context)
    {
        if (tutorial)
        {
            if (!tutorialController.isActive || !tutorialController.isReady || tutorialController.step < 1) return;
        }
        if (GameStatus.gamepadMode == 0) return;
        if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || (flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || rotateInputCooldown > 0 || context.ReadValue<Vector2>().magnitude < .5f) return;
        Vector2 readValue = context.ReadValue<Vector2>();
        if (readValue.magnitude < .2f) return;
        rotateInputCooldown = .2f;
        Vector3 axis = Vector3.zero;
        if (readValue.x > 0)
        {
            axis.y = 1;
            dir_mark[1] += 1;
        }
        else if (readValue.x < 0)
        {
            axis.y = -1;
            dir_mark[0] += 1;
        }
        if (tutorial && tutorialController.step <= 2)
        {
            tutorialController.index = dir_mark[tutorialController.step - 1];
        }
        AudioController.Instance.PlaySFX(rotationSE, settings.masterVolume * .8f);
        BlockDORotateAround(axis, .2f, 1);
    }

    void OnRotate(InputAction.CallbackContext context)
    {
		if (tutorial)
        {
			if (!tutorialController.isActive || !tutorialController.isReady || tutorialController.step < 1) return;
		}
		if ((flagStatus & FlagsStatus.Collapse) == FlagsStatus.Collapse || (flagStatus & FlagsStatus.PressDownButton) == FlagsStatus.PressDownButton || rotateInputCooldown > 0 || context.ReadValue<Vector2>().magnitude < .5f) return;

        Vector2 readValue = context.ReadValue<Vector2>();
        if (readValue.magnitude <= .2f) return;
        rotateInputCooldown = .2f;
        float angle = Mathf.Atan2(readValue.y, readValue.x) * Mathf.Rad2Deg;
        angle += angle < 0 ? 360 : 0;
        Vector3 axis = Vector3.zero;
        if (GameStatus.gamepadMode == 0)
        {
            if (angle >= 330 || angle < 30)  // right
            {
                axis.y = 1;
                dir_mark[1] += 1;
            }
            else if (angle >= 150 && angle < 210) // left
            {
                axis.y = -1;
                dir_mark[0] += 1;
            }
            else if (angle >= 30 && angle < 90) // forward
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.z = -1;
                        break;
                    case 1:
                        axis.x = 1;
                        break;
                    case 2:
                        axis.z = 1;
                        break;
                    case 3:
                        axis.x = -1;
                        break;
                }
                dir_mark[4] += 1;
            }
            else if (angle >= 90 && angle < 150) // up
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.x = 1;
                        break;
                    case 1:
                        axis.z = 1;
                        break;
                    case 2:
                        axis.x = -1;
                        break;
                    case 3:
                        axis.z = -1;
                        break;
                }
                dir_mark[2] += 1;
            }
            else if (angle >= 210 && angle < 270) // back
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.z = 1;
                        break;
                    case 1:
                        axis.x = -1;
                        break;
                    case 2:
                        axis.z = -1;
                        break;
                    case 3:
                        axis.x = 1;
                        break;
                }
                dir_mark[5] += 1;
            }
            else if (angle >= 270 && angle < 330) // down
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.x = -1;
                        break;
                    case 1:
                        axis.z = -1;
                        break;
                    case 2:
                        axis.x = 1;
                        break;
                    case 3:
                        axis.z = 1;
                        break;
                }
                dir_mark[3] += 1;
            }
        }
        else
        {
            if (angle >= 0 && angle < 90)
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.z = -1;
                        break;
                    case 1:
                        axis.x = 1;
                        break;
                    case 2:
                        axis.z = 1;
                        break;
                    case 3:
                        axis.x = -1;
                        break;
                }
                dir_mark[4] += 1;
            }
            else if (angle >= 90 && angle < 180)
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.x = 1;
                        break;
                    case 1:
                        axis.z = 1;
                        break;
                    case 2:
                        axis.x = -1;
                        break;
                    case 3:
                        axis.z = -1;
                        break;
                }
                dir_mark[2] += 1;
            }
            else if (angle >= 180 && angle < 270)
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.z = 1;
                        break;
                    case 1:
                        axis.x = -1;
                        break;
                    case 2:
                        axis.z = -1;
                        break;
                    case 3:
                        axis.x = 1;
                        break;
                }
                dir_mark[5] += 1;
            }
            else if (angle >= 270 && angle < 360)
            {
                switch (cameraController.cameraIndex)
                {
                    case 0:
                        axis.x = -1;
                        break;
                    case 1:
                        axis.z = -1;
                        break;
                    case 2:
                        axis.x = 1;
                        break;
                    case 3:
                        axis.z = 1;
                        break;
                }
                dir_mark[3] += 1;
            }
        }



        if (tutorial && tutorialController.step <= 6)
        {
            tutorialController.index = dir_mark[tutorialController.step - 1];
        }
        AudioController.Instance.PlaySFX(rotationSE, settings.masterVolume * .8f);
        BlockDORotateAround(axis, .2f, 1);

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
            if (!settings.isColorRandom) return tempOrder;
        }
        else
        {
            if (!settings.isColorRandom)
            {
                if (array[11] == 5) tempOrder = new int[12] { 4, 5, 2, 3, 4, 5, 2, 3, 4, 5, 2, 3 };
                else tempOrder = new int[12] { 2, 3, 4, 5, 2, 3, 4, 5, 2, 3, 4, 5 };
                return tempOrder;
            }
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
        if (colorHistory[colorHistory.Length - 3] == 0) colorHistory = GenerateColor(colorHistory);
    }




    public void GenerateBlock(int index)
    {
        if (pivotObj) Destroy(pivotObj.gameObject);
        if (GameStatus.gameState == GAME_STATE.GAME_INGAME && !tutorial) actionTimer.RecoveryTimer();
        for (int i = 0; i < fillVertex.Length; i++)
        {
            fillVertex[i] = Vector3.zero;
        }


        for (int i = 0; i < 3; i++)
        {
            nextBlockPreview.GeneratePreviewBlock(i, blockHistory[i + 9]);
        }
        Block3DType blockType = (Block3DType)index;
        Vector3 genPos = GetHighestPoint(true);
        if (genPos.y < lastGroundLevel + 8)
        {
            genPos = cameraController.transform.position;
            genPos.y = lastGroundLevel + 8;
            var zonePos = gameOverTrigger.transform.position;
            zonePos.y = genPos.y - 12;
            gameOverTrigger.transform.position = zonePos;
        }
        else
        {
            genPos.x = origin.x; genPos.z = origin.z; genPos.y += 2;
        }
        cameraController.MoveCamera(genPos, genPos.y - (lastGroundLevel + 8));
        genPos.y += 8;


        transform.position = RoundOffVec3(genPos);
        transform.rotation = Quaternion.identity;

        AudioController.Instance.PlaySFX(generateSE, settings.masterVolume);

        pivotObj = new GameObject($"Block {blockCount}").transform;
        pivotObj.parent = transform;
        pivotObj.localPosition = Vector3.zero;
        BlockColor pivotColor = pivotObj.AddComponent<BlockColor>();
        pivotColor.blockAction = this;
        pivotColor.blockColor = PentacubeColors[colorHistory[colorHistory.Length - 1]];
        int cubeCount = 0;
        weightedBlockOffset = Vector3.zero;
		foreach (Vector3 offset in PentacubeShapes.Shapes[blockType]) weightedBlockOffset += offset;
        weightedBlockOffset /= 5;
		foreach (Vector3 offset in PentacubeShapes.Shapes[blockType])
        {
            GameObject obj;
            if (offset == Vector3.zero && (blockCount % settings.CoreFrom < settings.CoreGet || blockCount <= 8))
            {
                obj = Instantiate(weightedCubePrefab, transform.position + offset, Quaternion.identity, pivotObj);
                weightedBlockOffset = offset;
                // ビーコンアニメーションの再生
                if (blockCount > 1)
                {
                    beaconLineAnimation.PlayAnimation();
                }   
            }
            else
            {
                obj = Instantiate(cubePrefab, transform.position + offset, Quaternion.identity, pivotObj);
            }

            // ★
            // BlockGlowControllerをobjに追加
            //BlockGlowController glowController = obj.AddComponent<BlockGlowController>();

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
            //glowController.SetBlinkOffset(colorOffset);

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

    // 追加：ビーコン=========================================================================
    public Color GetCurrentBlockColor()
    {
        // 現在のブロックの色を取得
        if (colorHistory.Length > 0)
        {
            return PentacubeColors[colorHistory[colorHistory.Length - 1]];
        }
        return Color.white; // デフォルトの色
    }
    // 追加===================================================================================
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