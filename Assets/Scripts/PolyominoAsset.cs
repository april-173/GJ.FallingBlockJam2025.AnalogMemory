using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PolyominoAsset", menuName = "Pentris/PolyominoAsset")]
public class PolyominoAsset : ScriptableObject
{
    public string id;
    public List<Vector2Int> baseCells = new List<Vector2Int>();                                 // 相对于原点的方块偏移列表
    public List<List<Vector2Int>> allRotatedCells = new List<List<Vector2Int>>();               // 各旋转状态的方块偏移列表
    public List<List<Vector2Int>> allFlippedCells = new List<List<Vector2Int>>();               // 各翻转状态的方块偏移列表
    
    public List<Vector2Int> rotatedCells1, rotatedCells2, rotatedCells3, rotatedCells4;         // 旋转状态的方块偏移列表
    public List<Vector2Int> flippedCells1, flippedCells2, flippedCells3, flippedCells4;         // 翻转后的方块偏移列表

    public List<List<Vector2Int>> GetAllRotatedCells(List<Vector2Int> cells)
    {
        List<List<Vector2Int>> allRotated = new List<List<Vector2Int>>();
        allRotated.Add(cells);

        List<Vector2Int> current = new List<Vector2Int>(cells);
        for (int i = 1; i < 4; i++)
        {
            List<Vector2Int> newCells = new List<Vector2Int>(current.Count);
            for (int j = 0; j < current.Count; j++)
                newCells.Add(new Vector2Int(current[j].y, -current[j].x));
            allRotated.Add(newCells);
            current = newCells;
        }

        return allRotated;
    }

    public List<List<Vector2Int>> GetAllFlippedCells(List<List<Vector2Int>> allCells)
    {
        List<List<Vector2Int>> allFlipped = new List<List<Vector2Int>>();

        for(int i = 0;i < allCells.Count; i++)
        {
            List<Vector2Int> newCells = new List<Vector2Int>(allCells[i].Count);
            for (int j = 0; j < allCells[i].Count; j++)
                newCells.Add(new Vector2Int(-allCells[i][j].x, allCells[i][j].y));
            allFlipped.Add(newCells);
        }

        return allFlipped;
    }

    public void GetRotatedAndFlippedCell()
    {
        rotatedCells1 = new List<Vector2Int>(allRotatedCells[0]);
        rotatedCells2 = new List<Vector2Int>(allRotatedCells[1]);
        rotatedCells3 = new List<Vector2Int>(allRotatedCells[2]);
        rotatedCells4 = new List<Vector2Int>(allRotatedCells[3]);
        flippedCells1 = new List<Vector2Int>(allFlippedCells[0]);
        flippedCells2 = new List<Vector2Int>(allFlippedCells[1]);
        flippedCells3 = new List<Vector2Int>(allFlippedCells[2]);
        flippedCells4 = new List<Vector2Int>(allFlippedCells[3]);
    }

    public void Initialize(int order)
    {
        switch(order)
        {
            case 0:
                id = "F";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, -1), 
                    new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-1, 0)
                };
                break;
            case 1:
                id = "I";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(-1, 0), 
                    new Vector2Int(-2, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
                };
                break;

            case 2:
                id = "L";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, -1), 
                    new Vector2Int(1, -1), new Vector2Int(0, 1), new Vector2Int(0, 2)
                };
                break;
            case 3:
                id = "N";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(1, 0), 
                    new Vector2Int(2, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1)
                };
                break;
            case 4:
                id = "P";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(1, 0), 
                    new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, -1)
                };
                break;
            case 5:
                id = "T";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, -1), 
                    new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(-1, 1)
                };
                break;
            case 6:
                id = "U";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(-1, 0), 
                    new Vector2Int(-1, 1), new Vector2Int(1, 0), new Vector2Int(1, 1)
                };
                break;
            case 7:
                id = "V";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, 1), 
                    new Vector2Int(0, 2), new Vector2Int(1, 0), new Vector2Int(2, 0)
                };
                break;
            case 8:
                id = "W";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, -1), 
                    new Vector2Int(-1, -1), new Vector2Int(1, 0), new Vector2Int(1, 1)
                };
                break;
            case 9:
                id = "X";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(1, 0), 
                    new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1)
                };
                break;
            case 10:
                id = "Y";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, -1), 
                    new Vector2Int(0, -2), new Vector2Int(0, 1), new Vector2Int(1, 0)
                };
                break;
            case 11:
                id = "Z";
                baseCells = new List<Vector2Int> { 
                    new Vector2Int(0, 0), new Vector2Int(0, 1), 
                    new Vector2Int(1, 1), new Vector2Int(0, -1), new Vector2Int(-1, -1) };
                break;
            default:
                id = "Unknown";
                baseCells = new List<Vector2Int> { new Vector2Int(0, 0) };
                allRotatedCells = new List<List<Vector2Int>> { baseCells };
                break;
        }

        allRotatedCells = GetAllRotatedCells(baseCells);
        allFlippedCells = GetAllFlippedCells(allRotatedCells);
        GetRotatedAndFlippedCell();
    }
}
