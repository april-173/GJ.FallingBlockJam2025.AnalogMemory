using UnityEngine;

public struct Cell
{
    public bool occupied;           // �Ƿ�ռ��
    public string pieceId;             // ռ�øõ�Ԫ��ķ���ID

    public void Clear()
    {
        occupied = false;
        pieceId = "";
    }
}
