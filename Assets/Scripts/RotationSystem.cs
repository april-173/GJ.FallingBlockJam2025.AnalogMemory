using UnityEngine;

public class RotationSystem
{
    private static readonly Vector2Int[] defaultRotateKicks = new Vector2Int[]
    {
        new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(2,0), new Vector2Int(-2,0),new Vector2Int(0,1)
    };
    private static readonly Vector2Int[] defaultMirrorKicks = new Vector2Int[]
    {
        new Vector2Int(0,0),new Vector2Int(1,0),
        new Vector2Int(-1,0),new Vector2Int(0,1)
    };

    public bool TryRotate(
        PolyominoAsset piece, Vector2Int currentPos, int currentRot, bool isMirrored, Grid grid,
        out Vector2Int newPos, out int newRot)
    {
        newPos = currentPos;
        newRot = currentRot;
        if (piece == null || grid == null) return false;

        for(int i = 1; i <= 3; i++)
        {
            int targetRot = (currentRot + i + 4) % 4;

            foreach(var kick in defaultRotateKicks)
            {
                Vector2Int testPos = currentPos + kick;
                var offsets = isMirrored ? piece.allFlippedCells[targetRot] : piece.allRotatedCells[targetRot];
                if(grid.CanPlace(offsets, testPos))
                {
                    newPos = testPos;
                    newRot = targetRot;
                    return true;
                }
            }
        }
        return false;
    }

    public bool TryMirror(
        PolyominoAsset piece, Vector2Int currentPos, int currentRot, bool isMirrored, Grid grid,
        out Vector2Int newPos, out bool newMirror)
    {
        newPos = currentPos;
        newMirror = isMirrored;
        if (piece == null || grid == null) return false;

        bool targetMirror = !isMirrored;

        foreach (var kick in defaultMirrorKicks)
        {
            Vector2Int testPos = currentPos + kick;
            var offsets = targetMirror ? piece.allFlippedCells[currentRot] : piece.allRotatedCells[currentRot];
            if (grid.CanPlace(offsets, testPos))
            {
                newPos = testPos;
                newMirror = targetMirror;
                return true;
            }
        }
        return false;
    }
}
