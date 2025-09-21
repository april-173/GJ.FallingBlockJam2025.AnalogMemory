using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Config")]
    [SerializeField] private int width = 12;
    [SerializeField] private int visibleHeight = 24;
    [SerializeField] private int hiddenHeight = 5;

    [Header("References")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private TileRenderer tileRenderer;
    [SerializeField] private ScoringSystem scoringSystem;
    [SerializeField] private SettingsManager settingsManager;

    [Header("Key bindings")]
    [SerializeField] private Key resetGame = Key.R;
    [SerializeField] private Key destroyCurrentPiece = Key.F;
    [SerializeField] private Key toggleUI = Key.V;

    [Header("Jurisdiction")]
    public bool canGhost = true;
    public bool canPreview = true;
    public bool canDestory = true;

    [Header("Time")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private string timePrefix = "Time: ";
    private float currentTime;

    [Header("Gameobject")]
    [SerializeField] private TextMeshProUGUI spaceGo;
    [SerializeField] private TextMeshProUGUI gameOverGo;
    [SerializeField] private GameObject uiGo;

    private RotationSystem rotationSystem;
    private Grid grid;
    private BagGenerator bagGenerator;
    private PieceController activePiece;
    private List<PolyominoAsset> pieces;

    private Keyboard keyboard;

    private bool isPaused = true;
    private bool isGameOver = false;

    private void Awake()
    {
        if(instance ==  null)
            instance = this;

        keyboard = Keyboard.current;

        pieces = GetAllPentominoes();

        rotationSystem = new RotationSystem();
        grid = new Grid(width, visibleHeight, hiddenHeight);
        bagGenerator = new BagGenerator(pieces);

        tileRenderer.SetDependencies(grid);
        tileRenderer.Initialize();
        tileRenderer.UpdateLockedTilemap();
        tileRenderer.UpdatePreviewTilemap(null, false);

        grid.OnGridChanged += tileRenderer.UpdateLockedTilemap;
        grid.OnLinesCleared += HandleLinesCleared;

        tileRenderer.PauseTilemapController(true);
        spaceGo.gameObject.SetActive(true);
        gameOverGo.gameObject.SetActive(false);
    }

    private void SpawnNewPiece()
    {
        PolyominoAsset asset = bagGenerator.GetNext();
        PolyominoAsset next = bagGenerator.PeekNext(1)[0];

        GameObject pieceGO = new GameObject("PieceController");
        activePiece = pieceGO.AddComponent<PieceController>();
        activePiece.SetDependencies(grid, inputManager, rotationSystem, tileRenderer, scoringSystem);

        Vector2Int spawnPos = new Vector2Int(width / 2, visibleHeight + hiddenHeight - 3);
        activePiece.Initialize(asset, spawnPos);

        activePiece.OnLocked += OnPieceLocked;

        tileRenderer.UpdatePreviewTilemap(next.baseCells, canPreview);
    }

    private void OnPieceLocked(PieceController piece)
    {
        if (piece == null || piece != activePiece) return;

        // 检查游戏结束条件
        if (grid.HasAnyBlockAboveVisible())
        {
            Debug.Log("Game Over!");
            isPaused = true;
            SoundManager.instance.PlaySFX(8);

            inputManager.enableInput = false;
            if (activePiece != null)
            {
                activePiece.SetInputEnabled(false);
                Destroy(activePiece);
            }

            tileRenderer.PauseTilemapController(true, 0.01f);
            spaceGo.gameObject.SetActive(false);
            gameOverGo.gameObject.SetActive(true);

            tileRenderer.UpdatePreviewTilemap(null, false);

            isGameOver = true;
            return;
        }
    }

    private void Update()
    {
        if (keyboard[resetGame].wasPressedThisFrame && !isPaused)
        {
            StartCoroutine(StartRestGame());
        }

        if (keyboard[destroyCurrentPiece].wasPressedThisFrame && canDestory && !isPaused) 
        {
            DestroyCurrentPiece();
        }

        if (keyboard[Key.Space].wasPressedThisFrame)
        {
            if (settingsManager != null) settingsManager.Back2();

            if(!isGameOver)
            {
                isPaused = !isPaused;

                inputManager.enableInput = !isPaused;
                if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

                tileRenderer.PauseTilemapController(isPaused, 0.01f);
                spaceGo.gameObject.SetActive(isPaused);
            }
            else
            {
                ResetGame();
                isPaused = false;

                inputManager.enableInput = !isPaused;
                if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

                tileRenderer.PauseTilemapController(isPaused, 0.01f);
                spaceGo.gameObject.SetActive(isPaused);
                gameOverGo.gameObject.SetActive(false);
            }
        }

        if (!isPaused && activePiece == null)
        {
            SpawnNewPiece();
        }

        if(!isPaused)
        {
            currentTime += Time.deltaTime;
            UpdateTime();
        }

        if(keyboard[toggleUI].wasPressedThisFrame)
        {
            uiGo.SetActive(!uiGo.activeSelf);
            SoundManager.instance.PlaySFX(1);
        }

        if (keyboard[Key.Escape].wasPressedThisFrame)
        {
            StartCoroutine(settingsManager.StartQuit());
        }
    }

    private void ResetGame()
    {
        isGameOver = false;

        grid.Reset();                       // 清空 Grid
        tileRenderer.ClearActivePiece();    // 清空上一次的活动方块
        tileRenderer.ClearGhostPiece();     // 清空幽灵方块

        // 销毁当前活动方块
        if (activePiece != null)
        {
            activePiece.OnLocked -= OnPieceLocked;
            Destroy(activePiece.gameObject);
            activePiece = null;
        }

        bagGenerator.Reset();   // 重置 BagGenerator
        scoringSystem.Reset();  // 重置分数系统
        currentTime = 0;

        // 生成新方块
        SpawnNewPiece();
    }

    private IEnumerator StartRestGame()
    {
        isPaused = true;

        inputManager.enableInput = !isPaused;
        if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

        tileRenderer.PauseTilemapController(isPaused, 0.01f);

        yield return new WaitForSeconds(0.5f);

        isPaused = false;

        inputManager.enableInput = !isPaused;
        if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

        tileRenderer.PauseTilemapController(isPaused, 0.01f);

        ResetGame();
    }

    private void DestroyCurrentPiece()
    {
        SoundManager.instance.PlaySFX(6);

        tileRenderer.ClearActivePiece();    // 清空上一次的活动方块
        tileRenderer.ClearGhostPiece();     // 清空幽灵方块

        // 销毁当前活动方块
        if (activePiece != null)
        {
            activePiece.OnLocked -= OnPieceLocked;
            Destroy(activePiece.gameObject);
            activePiece = null;
        }

        // 生成新方块
        SpawnNewPiece();
    }

    private void HandleLinesCleared(List<int> cleared)
    {
        scoringSystem.AddLines(cleared.Count);
    }

    public static List<PolyominoAsset> GetAllPentominoes()
    {
        var assets = Resources.LoadAll<PolyominoAsset>("Polyomino");
        if (assets == null || assets.Length == 0)
            Debug.LogError("<color=#92BFD1><b>[PolyominoAsset]</b></color> <color=#F17D7C>在 Resources/Polyomino 中未找到相关资产</color>");
        return new List<PolyominoAsset>(assets);
    }

    private void UpdateTime()
    {
        int totalMilliseconds = (int)(currentTime * 1000f);
        int minute = totalMilliseconds / 60000;
        int second = (totalMilliseconds / 1000) % 60;
        int centisecond = (totalMilliseconds / 10) % 100;

        if (timeText != null)
            timeText.text = timePrefix
                + minute.ToString("D2") + ":"
                + second.ToString("D2") + ":"
                + centisecond.ToString("D2");
    }

    public void SetCanPreview(bool canPreview)
    {
        this.canPreview = canPreview;
        tileRenderer.UpdatePreviewTilemap(null, false);
    }

    public void SetCanGhost(bool canGhost)
    {
        this.canGhost = canGhost;
        tileRenderer.UpdateGhostPiece(null, Vector2Int.zero, false);
    }

    public void SetCanDestory(bool canDestory)
    {
        this.canDestory = canDestory;
    }

    public void Paused(bool paused)
    {
        if(isPaused ==  paused) return;

        isPaused = paused;

        inputManager.enableInput = !isPaused;
        if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

        tileRenderer.PauseTilemapController(isPaused, 0.01f);
        spaceGo.gameObject.SetActive(isPaused);
    }

    public void SettinPausedg(bool paused)
    {
        if (isPaused == paused) return;

        isPaused = paused;

        inputManager.enableInput = !isPaused;
        if (activePiece != null) activePiece.SetInputEnabled(!isPaused);

        tileRenderer.PauseTilemapController(isPaused);
        spaceGo.gameObject.SetActive(isPaused);
    }
}

