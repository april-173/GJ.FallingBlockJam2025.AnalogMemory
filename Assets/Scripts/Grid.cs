using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 表示将对象放置在特定位置的尝试结果。
/// </summary>
/// <remarks>Success 成功/Collision 冲突/OutOfBounds 越界</remarks>
public enum PlaceResult
{
    Success,
    Collision,
    OutOfBounds
}

public class Grid
{
    public int Width { get; private set; }
    public int VisibleHeight { get; private set; }
    public int HiddenHeight { get; private set; }
    public int TotalHeight => VisibleHeight + HiddenHeight;

    private Cell[,] cells;          // 网格单元格数组
    private int[] rowCounts;        // 每行已占用的单元格数量
    private bool useBitmask;        // 是否使用位掩码优化
    private ulong[] rowBitmasks;    // 每行的位掩码
    private int[] columnHeights;    // 每列的高度

    public event Action<List<int>> OnLinesCleared;  // 当行被清除时触发的事件
    public event Action OnGridChanged;              // 当网格发生变化时触发的事件

    public Grid(int width, int visibleHeight, int hiddenHeight)
    {
        if (width <= 0) throw new ArgumentException("Width 宽度必须大于0");
        if (visibleHeight <= 0) throw new ArgumentException("visibleHeight 可见高度必须大于0");
        if (hiddenHeight < 0) throw new ArgumentException("hiddenHeight 必须大于等于0");

        Width = width;
        VisibleHeight = visibleHeight;
        HiddenHeight = hiddenHeight;

        cells = new Cell[Width, TotalHeight];
        rowCounts = new int[TotalHeight];
        useBitmask = Width <= 64; // 仅当宽度不超过64时使用位掩码优化
        if (useBitmask) rowBitmasks = new ulong[TotalHeight];
        columnHeights = new int[Width];

        Reset();
    }
    
    public void Reset()
    {
        for (int x = 0; x < Width; x++) 
            for(int y = 0; y < TotalHeight; y++)
                cells[x, y] = new Cell();

        for (int y = 0; y < TotalHeight; y++)
        {
            rowCounts[y] = 0;
            if (useBitmask) rowBitmasks[y] = 0UL;
        }

        for(int x = 0; x < Width; x++)
            columnHeights[x] = 0;

        OnGridChanged?.Invoke();
    }

    public bool IsInside(Vector2Int p)
    {
        return p.x >= 0 && p.x < Width && p.y >= 0 && p.y < TotalHeight;
    }

    public bool IsEmpty(Vector2Int p)
    {
        return IsInside(p) && !cells[p.x, p.y].occupied;
    }

    public bool CanPlace(IEnumerable<Vector2Int> blockOffsets,Vector2Int origin)
    {
        if(blockOffsets == null) throw new ArgumentNullException(nameof(blockOffsets));

        var offsets = blockOffsets as List<Vector2Int> ?? new List<Vector2Int>(blockOffsets);
        foreach (var offset in offsets)
        {
            Vector2Int p = origin + offset;
            if (!IsInside(p)) return false;             // 越界
            if (cells[p.x, p.y].occupied) return false; // 冲突
        }

        return true;
    }

    public PlaceResult PlacePiece(IEnumerable<Vector2Int> blockOffsets,Vector2Int origin,string pieceId)
    {
        if (blockOffsets == null) throw new ArgumentNullException(nameof(blockOffsets));
        
        var offsets = blockOffsets as List<Vector2Int> ?? new List<Vector2Int>(blockOffsets);
        foreach (var offset in offsets)
        {
            Vector2Int p = origin + offset;
            if (!IsInside(p)) return PlaceResult.OutOfBounds;               // 越界
            if (cells[p.x, p.y].occupied) return PlaceResult.Collision;     // 冲突
        }

        foreach(var offset in offsets)
        {
            Vector2Int p = origin + offset;
            cells[p.x, p.y].occupied = true;
            cells[p.x, p.y].pieceId = pieceId;

            rowCounts[p.y]++;
            if (useBitmask) rowBitmasks[p.y] |= (1UL << p.x);
            columnHeights[p.x] = Mathf.Max(columnHeights[p.x], p.y + 1);
        }

        OnGridChanged?.Invoke();
        return PlaceResult.Success;
    }

    public List<int> ClearFullLines()
    {
        List<int> cleared = new List<int>();
        for (int y = 0; y < TotalHeight; y++)
            if (rowCounts[y] == Width) 
                cleared.Add(y);
        if (cleared.Count == 0) 
            return cleared;

        Cell[,] newCells = new Cell[Width, TotalHeight];
        int[] newRowCounts = new int[TotalHeight];
        ulong[] newRowBitmasks = useBitmask ? new ulong[TotalHeight] : null;
        int[] newColumnHeights = new int[Width];

        int newY = 0;
        for (int y = 0; y < TotalHeight; y++)
        {
            if (rowCounts[y] == Width) continue;
            for (int x = 0; x < Width; x++)
            {
                newCells[x, newY] = cells[x, y];
                if (cells[x, y].occupied) newColumnHeights[x] = Mathf.Max(newColumnHeights[x], newY + 1);
            }
            newRowCounts[newY] = rowCounts[y];
            if (useBitmask) newRowBitmasks[newY] = rowBitmasks[y];
            newY++;
        }

        cells = newCells;
        rowCounts = newRowCounts;
        if(useBitmask) rowBitmasks = newRowBitmasks;
        columnHeights = newColumnHeights;

        OnGridChanged?.Invoke();
        OnLinesCleared?.Invoke(cleared);

        SoundManager.instance.PlaySFX(3);
        return cleared;
    }

    public bool HasAnyBlockAboveVisible()
    {
        for (int y = VisibleHeight; y < TotalHeight; y++)
            if (rowCounts[y] > 0) return true;
        return false;
    }

    public Vector2Int PredictPlacementOrigin(IEnumerable<Vector2Int> blockOffsets,Vector2Int origin)
    {
        if (blockOffsets == null) throw new ArgumentNullException(nameof(blockOffsets));

        var offsets = blockOffsets as List<Vector2Int> ?? new List<Vector2Int>(blockOffsets);
        Vector2Int predictedOrigin = origin;

        while(CanPlace(offsets,predictedOrigin + Vector2Int.down))
            predictedOrigin += Vector2Int.down;

        return predictedOrigin;
    }

    public List<Vector2Int> PredictPlacementCells(IEnumerable<Vector2Int> blockOffsets, Vector2Int origin)
    {
        if (blockOffsets == null) throw new ArgumentNullException(nameof(blockOffsets));

        Vector2Int predictedOrigin = PredictPlacementOrigin(blockOffsets, origin);
        return blockOffsets.Select(offset => predictedOrigin + offset).ToList();
    }

    public Cell[,] Cells => cells;
}
