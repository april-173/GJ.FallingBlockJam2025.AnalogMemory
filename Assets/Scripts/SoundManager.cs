using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;   // 背景音乐
    public AudioSource sfxSource;   // 音效

    [Header("Audio Clips")]
    public AudioClip[] bgmClips;    // 背景音乐数组
    public AudioClip[] sfxClips;    // 音效数组
    public AudioClip switchClip;    // 切换歌曲时的过渡音效

    private int currentBgmIndex = 0;
    private bool isSwitching = false;

    private Keyboard keyboard;

    private float[] lastPlayTime;       // 记录每个音效上次播放时间
    public float sfxCooldown = 0.05f;   // 最短触发间隔，避免连续叠加太吵

    private void Awake()
    {
        if(instance == null) 
            instance = this;

        keyboard = Keyboard.current;
        lastPlayTime = new float[sfxClips.Length];
    }

    private void Start()
    {
        // 默认播放第一首 BGM
        if (bgmClips.Length > 0)
        {
            StartCoroutine(PlayInitialBGM());
        }
    }

    private IEnumerator PlayInitialBGM()
    {
        float fadeDuration = 0.3f;
        float startVolume = bgmSource.volume;
        float t = 0f;

        // 播放切换音效
        if (switchClip != null)
        {
            sfxSource.PlayOneShot(switchClip);
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
        }

        bgmSource.clip = bgmClips[0];
        bgmSource.loop = true;
        bgmSource.volume = 0f;
        bgmSource.Play();

        // 播放第一首 BGM
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }
    }

    private void Update()
    {
        // Q: 上一首
        if (keyboard[Key.Q].wasPressedThisFrame)
        {
            PlayPreviousBGM();
        }

        // E: 下一首
        if (keyboard[Key.E].wasPressedThisFrame)
        {
            PlayNextBGM();
        }
    }

    // 播放 BGM
    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Length) return;

        currentBgmIndex = index;
        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 播放 SFX
    public void PlaySFX(int index)
    {
        if (index < 0 || index >= sfxClips.Length) return;

        if (Time.time - lastPlayTime[index] < sfxCooldown)
            return;

        lastPlayTime[index] = Time.time;
        sfxSource.PlayOneShot(sfxClips[index]);
    }

    // 上一首
    public void PlayPreviousBGM()
    {
        if (!isSwitching)
        {
            int newIndex = (currentBgmIndex - 1 + bgmClips.Length) % bgmClips.Length;
            StartCoroutine(SwitchBGM(newIndex));
        }
    }

    // 下一首
    public void PlayNextBGM()
    {
        if (!isSwitching)
        {
            int newIndex = (currentBgmIndex + 1) % bgmClips.Length;
            StartCoroutine(SwitchBGM(newIndex));
        }
    }

    // 切换 BGM 协程
    private IEnumerator SwitchBGM(int newIndex)
    {
        isSwitching = true;

        // 停止当前 BGM
        float fadeDuration = 0.3f; // 淡出时间
        float startVolume = bgmSource.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.Stop();

        // 播放切换音效
        if (switchClip != null)
        {
            sfxSource.PlayOneShot(switchClip);
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            yield return new WaitForSeconds(0.25f); // 没有切换音效就等 0.3s
        }

        // 播放新 BGM
        bgmSource.clip = bgmClips[newIndex];
        bgmSource.loop = true;
        bgmSource.volume = 0f;
        bgmSource.Play();

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }

        currentBgmIndex = newIndex;
        isSwitching = false;
    }
}
