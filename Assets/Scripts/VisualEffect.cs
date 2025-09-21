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

    #region < 相机抖动 >
    [Header("Shake Settings")]
    public float duration = 0.1f;     // 抖动持续时间
    public float magnitude = 0.05f;   // 最大抖动幅度（世界单位）

    private Vector3 originalPos;

    private void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    /// <summary>
    /// 硬下落触发时调用
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
            // 随机上下微偏移
            float offsetY = Random.Range(-magnitude, magnitude);
            transform.localPosition = originalPos + new Vector3(0, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 恢复原位置
        transform.localPosition = originalPos;
    }
    #endregion

}
