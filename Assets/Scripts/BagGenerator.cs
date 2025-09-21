using System;
using System.Collections.Generic;

[Serializable]
public class BagGenerator
{
    private List<PolyominoAsset> bag;
    private Queue<PolyominoAsset> queue;
    private System.Random rng;

    public BagGenerator(List<PolyominoAsset> pieces,int? seed = null)
    {
        rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
        bag = pieces;
        if(bag == null || bag.Count == 0)
            throw new Exception("<color=#92BFD1><b>[BagGenerator]</b></color> <color=#F17D7C>未分配任何 PentominoAssets 资产</color>");
        queue = new Queue<PolyominoAsset>();
        RefillQueue(2);
    }

    private void RefillQueue(int times = 1)
    {
        for (int t = 0; t < times; t++)
        {
            var temp = new List<PolyominoAsset>(bag);
            for (int i = temp.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (temp[i], temp[j]) = (temp[j], temp[i]);
            }
            foreach (var p in temp)
                queue.Enqueue(p);
        }
    }

    public PolyominoAsset GetNext()
    {
        RefillCheck(bag.Count);
        return queue.Dequeue();
    }

    public List<PolyominoAsset> PeekNext(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "<color=#92BFD1><b>[BagGenerator]</b></color> <color=#F17D7C>count 必须大于0</color>");
        
        RefillCheck(count);
        var arr = queue.ToArray();
        var result = new List<PolyominoAsset>(count);
        for (int i = 0; i < count; i++)
            result.Add(arr[i]);
        return result;
    }

    public void RefillCheck(int count)
    {
        if (queue.Count < count)
        {
            int need = (int)Math.Ceiling((count - queue.Count) / (float)bag.Count);
            RefillQueue(need);
        }
    }

    public void Reset()
    {
        queue.Clear();
        RefillQueue(2);
    }

    public int QueueCount => queue.Count;
}
