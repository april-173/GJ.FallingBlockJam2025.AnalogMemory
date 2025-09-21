using UnityEngine;

public struct Cell
{
    public bool occupied;           // 是否被占用
    public string pieceId;             // 占用该单元格的方块ID

    public void Clear()
    {
        occupied = false;
        pieceId = "";
    }
}
