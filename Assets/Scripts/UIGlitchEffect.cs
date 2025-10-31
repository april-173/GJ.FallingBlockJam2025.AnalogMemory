using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GlitchTextOnInteract : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target Text (if null, will search in children)")]
    [SerializeField] private TMP_Text targetText;

    [Header("Glitch Settings")]
    [SerializeField] private float glitchInterval = 0.02f; // 每次随机字符的间隔
    [SerializeField] private float glitchDuration = 0.3f;  // Button/Toggle 持续时间
    [SerializeField] private string randomChars = "!@#$%^&*ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    private Coroutine glitchCoroutine;
    private string originalText;
    private bool isSlider = false;

    private void Awake()
    {
        if (targetText == null)
            targetText = GetComponentInChildren<TMP_Text>();

        if (targetText != null)
            originalText = targetText.text;

        // 检测是否挂在 Slider 上
        if (GetComponent<Slider>() != null)
            isSlider = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetText == null) return;

        if (isSlider)
        {
            // Slider：按下开始乱码
            StartGlitch();
        }
        else
        {
            // Button / Toggle：固定时长乱码
            if (glitchCoroutine != null)
                StopCoroutine(glitchCoroutine);

            glitchCoroutine = StartCoroutine(GlitchForDuration(glitchDuration));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isSlider) return;

        // Slider：松开时停止乱码
        StopGlitch();
    }

    private void StartGlitch()
    {
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);

        glitchCoroutine = StartCoroutine(GlitchLoop());
    }

    private void StopGlitch()
    {
        if (glitchCoroutine != null)
        {
            StopCoroutine(glitchCoroutine);
            glitchCoroutine = null;
        }
        targetText.text = originalText;
    }

    private IEnumerator GlitchForDuration(float duration)
    {
        yield return StartCoroutine(GlitchLoop(duration));
        targetText.text = originalText;
        glitchCoroutine = null;
    }

    private IEnumerator GlitchLoop(float duration = -1f)
    {
        float timer = 0f;

        while (duration < 0f || timer < duration)
        {
            string glitched = "";
            foreach (char c in originalText)
            {
                if (Random.value > 0.3f)
                    glitched += randomChars[Random.Range(0, randomChars.Length)];
                else
                    glitched += c;
            }

            targetText.text = glitched;
            yield return new WaitForSeconds(glitchInterval);

            if (duration > 0f)
                timer += glitchInterval;
        }
    }
}


