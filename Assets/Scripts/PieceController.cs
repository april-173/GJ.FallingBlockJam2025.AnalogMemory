using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceState
{
    Falling,
    Locking,
    Locked
}

public class PieceController : MonoBehaviour
{
    private PolyominoAsset polyomino;           // ��������
    private Vector2Int position;                // �����������е�λ�ã����½�Ϊԭ�㣩
    private int rotationIndex;                  // ��ǰ��ת״̬������0-3��
    private bool isMirrored;                    // �Ƿ�ת
    private bool inputEnabled;                  // �Ƿ���������
    private PieceState state;                   // ����״̬

    private List<Vector2Int> currentCells;      // ��ǰ����ĵ�Ԫ��ƫ���б�
    private Vector2Int ghostPosition;           // ���鷽��λ��

    private Grid grid;                          // ��������
    private InputManager inputManager;          // �������������
    private RotationSystem rotationSystem;      // ��תϵͳ����
    private TileRenderer tileRenderer;          // ��Ƭ��Ⱦ������
    private ScoringSystem scoringSystem;        // �Ʒ�ϵͳ����

    [SerializeField] private float lockDelay = 0.5f;    // �����ӳ�ʱ��
    [SerializeField] private float dropInterval = 0.8f; // ������ʱ��
    [SerializeField] private int maxLockResets = 15;    // ����������ô���
    [SerializeField] private float hardDropLockDelay = 0.15f;

    private float lockTimer;
    private float dropTimer;
    private int lockResetsCount;
    private int caution;

    public event Action<PieceController> OnLocked;

    public void SetDependencies(Grid grid, InputManager inputManager, RotationSystem rotationSystem,TileRenderer tileRenderer,ScoringSystem scoringSystem)
    {
        this.grid = grid;
        this.inputManager = inputManager;
        this.rotationSystem = rotationSystem;
        this.tileRenderer = tileRenderer;
        this.scoringSystem = scoringSystem;
    }

    public void Initialize(PolyominoAsset polyomino, Vector2Int startPosition)
    {
        this.polyomino = polyomino;
        this.position = startPosition;
        this.rotationIndex = 0;
        this.isMirrored = false;
        this.inputEnabled = true;
        this.state = PieceState.Falling;

        lockTimer = 0f;
        dropTimer = 0f;
        lockResetsCount = 0;

        this.polyomino.allRotatedCells = this.polyomino.GetAllRotatedCells(this.polyomino.baseCells);
        this.polyomino.allFlippedCells = this.polyomino.GetAllFlippedCells(this.polyomino.allRotatedCells);

        currentCells = null;
        ghostPosition = Vector2Int.zero;

        SubscribeInput();
        UpdateCells();
    }

    private void Update()
    {
        if (!inputEnabled) return;
        if (state == PieceState.Locked || polyomino == null) return;
        if (state == PieceState.Locking)
        {
            lockTimer += Time.deltaTime;
            if (lockTimer >= lockDelay || lockResetsCount >= maxLockResets)
            {
                LockPiece();
                return;
            }
        }

        dropTimer += Time.deltaTime;
        if (dropTimer >= dropInterval)
        {
            if (!AttemptMoveDown())
            {
                state = PieceState.Locking; 
                return;
            }
            state = PieceState.Falling;
            dropTimer = 0f;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeInput();
    }

    #region < Input �¼� >
    private void SubscribeInput()
    {
        if (inputManager == null) return;

        inputManager.OnMoveLeft += HandleMoveLeft;
        inputManager.OnMoveRight += HandleMoveRight;
        inputManager.OnRotate += HandleRotate;
        inputManager.OnMirror += HandleMirror;
        inputManager.OnSoftDrop += HandleSoftDrop;
        inputManager.OnHardDrop += HandleHardDrop;
    }

    private void UnsubscribeInput()
    {
        if (inputManager == null) return;

        inputManager.OnMoveLeft -= HandleMoveLeft;
        inputManager.OnMoveRight -= HandleMoveRight;
        inputManager.OnRotate -= HandleRotate;
        inputManager.OnMirror -= HandleMirror;
        inputManager.OnSoftDrop -= HandleSoftDrop;
        inputManager.OnHardDrop -= HandleHardDrop;
    }


    private void HandleMoveLeft() => Move(Vector2Int.left);
    private void HandleMoveRight() => Move(Vector2Int.right);
    private void HandleRotate() => Rotate();
    private void HandleMirror() => Mirror();
    private void HandleSoftDrop() { if (AttemptMoveDown()) { dropTimer = 0f; scoringSystem.HandleSoftDrop(); } SoundManager.instance.PlaySFX(2); }
    private void HandleHardDrop() { scoringSystem.HandleHardDrop(position.y - ghostPosition.y); HardDrop(); }

    #endregion


    #region < Piece ���� >
    public bool Move(Vector2Int dir)
    {
        if (!inputEnabled || polyomino == null || grid == null) return false;

        Vector2Int newPosition = position + dir;
        if(CanPlaceAt(newPosition,rotationIndex,isMirrored))
        {
            position = newPosition;
            UpdateCells();
            LockResets();
            SoundManager.instance.PlaySFX(2);
            caution = 0;
            return true;
        }
        caution++;
        if (caution == 3) SoundManager.instance.PlaySFX(0);
        return false;
    }

    public bool Rotate()
    {
        if (!inputEnabled || polyomino == null || grid == null) return false;

        if (rotationSystem.TryRotate(
            polyomino, position, rotationIndex, isMirrored, grid, 
            out var newPos, out var newRot))
        {
            position = newPos;
            rotationIndex = newRot;
            UpdateCells();
            LockResets();
            SoundManager.instance.PlaySFX(2);
            return true;
        }
        SoundManager.instance.PlaySFX(0);
        return false;
    }

    public bool Mirror()
    {
        if (!inputEnabled || polyomino == null || grid == null) return false;

        if (rotationSystem.TryMirror(
            polyomino, position, rotationIndex, isMirrored, grid,
            out var newPos, out var newMirror))
        {
            position = newPos;
            isMirrored = newMirror;
            UpdateCells();
            LockResets();
            SoundManager.instance.PlaySFX(2);
            return true;
        }
        SoundManager.instance.PlaySFX(0);
        return false;
    }

    public bool AttemptMoveDown()
    {
        if (polyomino == null || grid == null) return false;

        if (CanPlaceAt(position + Vector2Int.down, rotationIndex, isMirrored))
        {
            position += Vector2Int.down;
            UpdateCells();
            return true;
        }
        return false;
    }

    public bool HardDrop()
    {
        if (!inputEnabled || polyomino == null || grid == null || state == PieceState.Locked) return false;

        VisualEffect.instance.TriggerShake();

        position = ghostPosition;
        StartCoroutine(HardDropLockPiece());

        SoundManager.instance.PlaySFX(4);

        return true;
    }

    private IEnumerator HardDropLockPiece()
    {
        UpdateCells();
        yield return new WaitForSeconds(hardDropLockDelay);
        LockPiece();
    }

    private void LockPiece()
    {
        if (polyomino == null || grid == null || state == PieceState.Locked) return;

        state = PieceState.Locked;
        inputEnabled = false;

        var offsets = GetOffsetsForCurrentPiece(rotationIndex, isMirrored);
        grid.PlacePiece(offsets, position, polyomino.id);
        grid.ClearFullLines();

        OnLocked?.Invoke(this);

        tileRenderer.UpdateActivePiece(new List<Vector2Int>(), Vector2Int.zero);
        tileRenderer.ClearGhostPiece();

        Destroy(this.gameObject);

    }

    private void LockResets()
    {
        if (state == PieceState.Locking) { lockTimer = 0; lockResetsCount++; }
        if (state == PieceState.Falling) lockResetsCount = 0;
    }

    public bool CanPlaceAt(Vector2Int targetPos, int targetRot, bool targetMirror)
    {
        if (polyomino == null || grid == null) return false;
        var offsets = GetOffsetsForCurrentPiece(targetRot, targetMirror);
        return grid.CanPlace(offsets,targetPos);
    }

    private List<Vector2Int> GetOffsetsForCurrentPiece(int targetRot,bool targetMirror)
    {
        if (polyomino == null) return new List<Vector2Int>();

        var list = targetMirror ? polyomino.allFlippedCells : polyomino.allRotatedCells;

        if (list == null || list.Count == 0)
            return new List<Vector2Int>();

        int index = targetRot % list.Count;
        return list[index] ?? new List<Vector2Int>();
    }
    #endregion

    private void UpdateCells()
    {
        if (polyomino == null) return;

        var rotList = isMirrored ? polyomino.allFlippedCells : polyomino.allRotatedCells;

        // ��ֹԽ��
        if (rotList == null || rotList.Count == 0)
        {
            currentCells = new List<Vector2Int>();
            ghostPosition = position;
            return;
        }

        int index = rotationIndex % rotList.Count;
        currentCells = rotList[index];

        ghostPosition = grid.PredictPlacementOrigin(currentCells, position);

        tileRenderer.UpdateActivePiece(currentCells, position);
        tileRenderer.UpdateGhostPiece(currentCells, ghostPosition, GameManager.instance.canGhost);
    }

    public void SetInputEnabled(bool canInput)
    {
        this.inputEnabled = canInput;
    }
}
