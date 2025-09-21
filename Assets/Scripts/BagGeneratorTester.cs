using System.Collections.Generic;
using UnityEngine;

public class BagGeneratorTester : MonoBehaviour
{
    private BagGenerator bagGen;

    void Start()
    {
        var assets = Resources.LoadAll<PolyominoAsset>("Polyomino");

        // �̶�������ӣ�����۲������ĳ� null ����ÿ�ζ������
        bagGen = new BagGenerator(new List<PolyominoAsset>(assets), seed: null);

        Debug.Log("==== ��ʼ�� BagGenerator ��� ====");

        // ��ӡ��ʼ��ʱ�����������
        PrintQueue("��ʼ����");

        // ����ȡ 15 �Σ���һ���󣩣��۲��Ƿ���Զ�����
        Debug.Log("==== ��ʼ����ȡ Piece ====");
        for (int i = 0; i < 15; i++)
        {
            var piece = bagGen.GetNext();
            Debug.Log($"[{i}] ȡ��: {piece.id}");
        }

        PrintQueue("ȡ�� 15 �κ�Ķ���");

        // PeekNext ����
        var peekList = bagGen.PeekNext(5);
        Debug.Log("==== PeekNext(5) ====");
        foreach (var p in peekList)
            Debug.Log($"��������: {p.id}");

        // Reset ����
        bagGen.Reset();
        PrintQueue("Reset ֮��Ķ���");
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

