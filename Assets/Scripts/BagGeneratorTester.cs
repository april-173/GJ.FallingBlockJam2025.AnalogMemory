using System.Collections.Generic;
using UnityEngine;

public class BagGeneratorTester : MonoBehaviour
{
    private BagGenerator bagGen;

    void Start()
    {
        var assets = Resources.LoadAll<PolyominoAsset>("Polyomino");

        // 固定随机种子，方便观察结果（改成 null 可以每次都随机）
        bagGen = new BagGenerator(new List<PolyominoAsset>(assets), seed: null);

        Debug.Log("==== 初始化 BagGenerator 完成 ====");

        // 打印初始化时队列里的内容
        PrintQueue("初始队列");

        // 连续取 15 次（比一袋大），观察是否会自动补袋
        Debug.Log("==== 开始连续取 Piece ====");
        for (int i = 0; i < 15; i++)
        {
            var piece = bagGen.GetNext();
            Debug.Log($"[{i}] 取出: {piece.id}");
        }

        PrintQueue("取出 15 次后的队列");

        // PeekNext 测试
        var peekList = bagGen.PeekNext(5);
        Debug.Log("==== PeekNext(5) ====");
        foreach (var p in peekList)
            Debug.Log($"即将出现: {p.id}");

        // Reset 测试
        bagGen.Reset();
        PrintQueue("Reset 之后的队列");
    }

    private void PrintQueue(string title)
    {
        List<PolyominoAsset> snapshot = bagGen.PeekNext(bagGen.QueueCount);
        string line = $"{title}: ";
        for (int i = 0; i < snapshot.Count; i++)
        {
            line += snapshot[i].id + (i < snapshot.Count - 1 ? ", " : "");
        }
        Debug.Log(line);
    }
}

