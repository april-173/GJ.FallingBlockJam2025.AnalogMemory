using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;   // ��������
    public AudioSource sfxSource;   // ��Ч

    [Header("Audio Clips")]
    public AudioClip[] bgmClips;    // ������������
    public AudioClip[] sfxClips;    // ��Ч����
    public AudioClip switchClip;    // �л�����ʱ�Ĺ�����Ч

    private int currentBgmIndex = 0;
    private bool isSwitching = false;

    private Keyboard keyboard;

    private float[] lastPlayTime;       // ��¼ÿ����Ч�ϴβ���ʱ��
    public float sfxCooldown = 0.05f;   // ��̴��������������������̫��

    private void Awake()
    {
        if(instance == null) 
            instance = this;

        keyboard = Keyboard.current;
        lastPlayTime = new float[sfxClips.Length];
    }

    private void Start()
    {
        // Ĭ�ϲ��ŵ�һ�� BGM
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

        // �����л���Ч
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

        // ���ŵ�һ�� BGM
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
            yield return null;
        }
    }

    private void Update()
    {
        // Q: ��һ��
        if (keyboard[Key.Q].wasPressedThisFrame)
        {
            PlayPreviousBGM();
        }

        // E: ��һ��
        if (keyboard[Key.E].wasPressedThisFrame)
        {
            PlayNextBGM();
        }
    }

    // ���� BGM
    public void PlayBGM(int index)
    {
        if (index < 0 || index >= bgmClips.Length) return;

        currentBgmIndex = index;
        bgmSource.clip = bgmClips[index];
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // ���� SFX
    public void PlaySFX(int index)
    {
        if (index < 0 || index >= sfxClips.Length) return;

        if (Time.time - lastPlayTime[index] < sfxCooldown)
            return;

        lastPlayTime[index] = Time.time;
        sfxSource.PlayOneShot(sfxClips[index]);
    }

    // ��һ��
    public void PlayPreviousBGM()
    {
        if (!isSwitching)
        {
            int newIndex = (currentBgmIndex - 1 + bgmClips.Length) % bgmClips.Length;
            StartCoroutine(SwitchBGM(newIndex));
        }
    }

    // ��һ��
    public void PlayNextBGM()
    {
        if (!isSwitching)
        {
            int newIndex = (currentBgmIndex + 1) % bgmClips.Length;
            StartCoroutine(SwitchBGM(newIndex));
        }
    }

    // �л� BGM Э��
    private IEnumerator SwitchBGM(int newIndex)
    {
        isSwitching = true;

        // ֹͣ��ǰ BGM
        float fadeDuration = 0.3f; // ����ʱ��
        float startVolume = bgmSource.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
        bgmSource.Stop();

        // �����л���Ч
        if (switchClip != null)
        {
            sfxSource.PlayOneShot(switchClip);
            yield return new WaitForSeconds(0.25f);
        }
        else
        {
            yield return new WaitForSeconds(0.25f); // û���л���Ч�͵� 0.3s
        }

        // ������ BGM
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
