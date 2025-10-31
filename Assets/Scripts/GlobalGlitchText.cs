using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class GlobalGlitchText : MonoBehaviour
{
    [Header("Glitch Settings")]
    [SerializeField] private float glitchInterval = 0.05f;          // 每次随机字符的间隔
    [SerializeField] private float baseRecoveryDelay = 2f;          // 最底部的恢复时间
    [SerializeField] private float reappearGlitchDuration = 0.6f;   // 重新出现时乱码持续时间
    [SerializeField] private string randomChars = "!@#$%^&*ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private class TMPInfo
    {
        public TMP_Text text;
        public string original;
        public float recoveryTime;
        public bool wasActive;              // 记录之前的激活状态
        public Coroutine runningCoroutine;  // 当前运行的协程（防止重复）
    }

    private List<TMPInfo> allTexts = new List<TMPInfo>();

    private void Start()
    {
        TMP_Text[] texts = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (var t in texts)
        {
            float y = t.transform.position.y;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }

        foreach (var t in texts)
        {
            TMPInfo info = new TMPInfo
            {
                text = t,
                original = t.text,
                wasActive = t.gameObject.activeInHierarchy && t.enabled,
                recoveryTime = Mathf.Lerp(0.5f, baseRecoveryDelay, Mathf.InverseLerp(maxY, minY, t.transform.position.y))
            };

            allTexts.Add(info);

            // 游戏开始时统一乱码
            //info.runningCoroutine = StartCoroutine(GlitchAndRecover(info, info.recoveryTime));
        }
    }

    private void Update()
    {
        foreach (var info in allTexts)
        {
            bool isActive = info.text.gameObject.activeInHierarchy && info.text.enabled;

            // 检测 “隐藏 → 显示”
            if (!info.wasActive && isActive)
            {
                if (info.runningCoroutine != null)
                    StopCoroutine(info.runningCoroutine);

                // 触发一次短时乱码
                info.runningCoroutine = StartCoroutine(GlitchAndRecover(info, reappearGlitchDuration));
            }

            info.wasActive = isActive;
        }
    }

    private IEnumerator GlitchAndRecover(TMPInfo info, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            string glitched = "";
            foreach (char c in info.original)
            {
                if (Random.value > 0.3f)
                    glitched += randomChars[Random.Range(0, randomChars.Length)];
                else
                    glitched += c;
            }

            info.text.text = glitched;

            yield return new WaitForSeconds(glitchInterval);
            elapsed += glitchInterval;
        }

        info.text.text = info.original;
        info.runningCoroutine = null;
    }
}




