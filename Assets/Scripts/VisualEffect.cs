using System.Collections;
using UnityEngine;

public class VisualEffect : MonoBehaviour
{
    public static VisualEffect instance;

    private void Awake()
    {
        if(instance == null )
            instance = this;
    }

    #region < ������� >
    [Header("Shake Settings")]
    public float duration = 0.1f;     // ��������ʱ��
    public float magnitude = 0.05f;   // ��󶶶����ȣ����絥λ��

    private Vector3 originalPos;

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>
    /// Ӳ���䴥��ʱ����
    /// </summary>
    public void TriggerShake()
    {
        StopAllCoroutines();
        StartCoroutine(DoShake());
    }

    private IEnumerator DoShake()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // �������΢ƫ��
            float offsetY = Random.Range(-magnitude, magnitude);
            transform.localPosition = originalPos + new Vector3(0, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // �ָ�ԭλ��
        transform.localPosition = originalPos;
    }
    #endregion

}
