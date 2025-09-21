using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileRenderer : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap lockedTilemap;   // 已锁定的格子
    [SerializeField] private Tilemap activeTilemap;   // 活动方块
    [SerializeField] private Tilemap ghostTilemap;    // 影子
    [SerializeField] private Tilemap previewTilemap;  // 预览
    [SerializeField] private Tilemap pauseTilemap;    // 暂停

    [Header("Assets")]
    [SerializeField] private Tile pieceTile;
    [SerializeField] private Tile nullTile;
    [SerializeField] private Tile pauseTile;

    private Grid grid;

    private bool isEntryPause = true;
    private bool isExitPause = false;

    private float soundInterval = 0.8f;
    private float soundTimer;

    public void SetDependencies(Grid grid)
    {
        this.grid = grid;
    }

    public void Initialize()
    {
        grid.OnGridChanged += UpdateLockedTilemap;
        grid.OnLinesCleared += UpdateLinesCleared;
    }

    private void Update()
    {
        if (soundTimer > 0) soundTimer -= Time.deltaTime;
        if (soundTimer <= 0) soundTimer = 0;
    }

    public void UpdateLockedTilemap()
    {
        lockedTilemap.ClearAllTiles();
        var cell = grid.Cells;
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.VisibleHeight; y++)
            {
                if (cell[x, y].occupied)
                    lockedTilemap.SetTile(new Vector3Int(x, y), pieceTile);
                else
                    lockedTilemap.SetTile(new Vector3Int(x, y), nullTile);
            }
        }
    }

    public void UpdateActivePiece(List<Vector2Int> offsets, Vector2Int origin)
    {
        activeTilemap.ClearAllTiles();

        foreach (var offset in offsets)
        {
            int x = origin.x + offset.x;
            int y = origin.y + offset.y;

            if (x < 0 || x >= grid.Width) continue;
            if (y < 0 || y >= grid.VisibleHeight) continue;

            Vector3Int pos = new Vector3Int(x, y, 0);
            activeTilemap.SetTile(pos, pieceTile);
        }
    }

    public void UpdateGhostPiece(List<Vector2Int> offsets, Vector2Int origin, bool canGhost)
    {
        ghostTilemap.ClearAllTiles();

        if (!canGhost) return;

        foreach (var offset in offsets)
        {
            int x = origin.x + offset.x;
            int y = origin.y + offset.y;

            if (x < 0 || x >= grid.Width) continue;
            if (y < 0 || y >= grid.VisibleHeight) continue;

            Vector3Int pos = new Vector3Int(x, y, 0);
            ghostTilemap.SetTile(pos, pieceTile);
        }
    }

    public void UpdatePreviewTilemap(List<Vector2Int> offsets, bool canPreview)
    {
        previewTilemap.ClearAllTiles();

        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                previewTilemap.SetTile(pos, nullTile);
            }
        }

        if (!canPreview) return;

        foreach (var offset in offsets)
        {
            Vector3Int pos = new Vector3Int(offset.x, offset.y, 0);
            previewTilemap.SetTile(pos, pieceTile);
        }
    }

    public void PauseTilemapController(bool condition,float rate1 = 0, float rate2 = 0)
    {
        if (condition)
            StartCoroutine(EntryPause(rate1, rate2));
        else
            StartCoroutine(ExitPause(rate1, rate2));
    }

    private IEnumerator EntryPause(float rate1 = 0, float rate2 = 0)
    {
        if (!isEntryPause && !isExitPause && soundTimer <= 0) SoundManager.instance.PlaySFX(7);
        if (soundTimer <= 0) soundTimer = soundInterval;

        isEntryPause = true;

        for (int y = grid.VisibleHeight - 1; y >= 0; y--) 
        {
            if (y % 2 == 0)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    pauseTilemap.SetTile(new Vector3Int(x, y, 0), pauseTile);
                    if (rate2 != 0) yield return new WaitForSeconds(rate2);
                }
            }
            else
            {
                for (int x = grid.Width - 1; x >= 0; x--)
                {
                    pauseTilemap.SetTile(new Vector3Int(x, y, 0), pauseTile);
                    if (rate2 != 0) yield return new WaitForSeconds(rate2);
                }
            }
            if (rate1 != 0) yield return new WaitForSeconds(rate1);
        }

        isEntryPause = false;
    }

    private IEnumerator ExitPause(float rate1 = 0, float rate2 = 0)
    {
        if (!isExitPause && !isEntryPause && soundTimer <= 0) SoundManager.instance.PlaySFX(7);
        if (soundTimer <= 0) soundTimer = soundInterval;

        isExitPause = true;

        for (int y = grid.VisibleHeight - 1; y >= 0; y--)
        {
            if (y % 2 == 0)
            {
                for (int x = 0; x < grid.Width; x++)
                {
                    pauseTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    if (rate2 != 0) yield return new WaitForSeconds(rate2);
                }
            }
            else
            {
                for (int x = grid.Width - 1; x >= 0; x--)
                {
                    pauseTilemap.SetTile(new Vector3Int(x, y, 0), null);
                    if (rate2 != 0) yield return new WaitForSeconds(rate2);
                }
            }
            if (rate1 != 0) yield return new WaitForSeconds(rate1);
        }

        isExitPause = false;
    }

    public void UpdateLinesCleared(List<int> cleared)
    {
        StartCoroutine(LinesCleared(cleared));
    }

    private IEnumerator LinesCleared(List<int> cleared)
    {
        foreach (int i in cleared)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                pauseTilemap.SetTile(new Vector3Int(x, i, 0), pauseTile);
            }
        }

        yield return new WaitForSeconds(0.1f);

        pauseTilemap.ClearAllTiles();
    }

    public void ClearGhostPiece() => ghostTilemap.ClearAllTiles();
    public void ClearActivePiece() => activeTilemap.ClearAllTiles();
    public void ClearPreviewTilemap() => previewTilemap.ClearAllTiles();
}
