using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject graphicsPanel;
    public GameObject audioPanel;
    public GameObject gameplayPanel;
    public GameObject tipsPanel;
    public GameObject back;

    [Header("Graphics")]
    public Toggle fullscreenToggle;
    public Slider brightnessSlider;
    public Slider hueSlider;
    public Slider saturationSlider;
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Gameplay")]
    public Toggle ghostToggle;
    public Toggle previewToggle;
    public Toggle destoryToggle;

    [Header("Tips")]
    public Toggle tips1Toggle;
    public Toggle tips2Toggle;
    public GameObject tipe1;
    public GameObject tipe2;

    private void Start()
    {
        // 获取 Volume 里的 Color Adjustments
        if (globalVolume.profile.TryGet(out ColorAdjustments ca))
        {
            colorAdjustments = ca;
        }

        // 初始化 UI 控件
        fullscreenToggle.isOn = Screen.fullScreen;

        if (colorAdjustments != null)
        {
            // 亮度 (postExposure 默认是 0)
            brightnessSlider.minValue = -1f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.value = 0;
            // 色相 (范围 -180~180)
            hueSlider.minValue = -180f;
            hueSlider.maxValue = 180f;
            hueSlider.value = colorAdjustments.hueShift.value;
            // 饱和度 (范围 -100~100)
            saturationSlider.minValue = -100f;
            saturationSlider.maxValue = 100f;
            saturationSlider.value = 0;
        }

        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        hueSlider.onValueChanged.AddListener(SetHue);
        saturationSlider.onValueChanged.AddListener(SetSaturation);

        // 初始化音量滑条
        float value;

        // 初始化 Master
        if (audioMixer.GetFloat("MasterVolume", out value))
            masterSlider.value = Mathf.Pow(10f, value / 20f);

        // 初始化 BGM
        if (audioMixer.GetFloat("BGMVolume", out value))
            bgmSlider.value = Mathf.Pow(10f, value / 20f);

        // 初始化 SFX
        if (audioMixer.GetFloat("SFXVolume", out value))
            sfxSlider.value = Mathf.Pow(10f, value / 20f);

        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        ghostToggle.onValueChanged.AddListener(SetGhost);
        previewToggle.onValueChanged.AddListener(SetPreview);
        destoryToggle.onValueChanged.AddListener(SetDestroy);

        tips1Toggle.onValueChanged.AddListener(SetTips1);
        tips2Toggle.onValueChanged.AddListener(SetTips2);

        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    // -------- 图像 ----------
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        SoundManager.instance.PlaySFX(1);
    }

    public void SetBrightness(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.postExposure.Override(value); // 简单亮度调整
    }

    public void SetHue(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.hueShift.Override(value);
    }

    public void SetSaturation(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.saturation.Override(value);
    }

    // -------- 音频 ----------
    private void SetMasterVolume(float value)
    {
        if (value <= 0.0001f)
            audioMixer.SetFloat("MasterVolume", -80f);  // 静音
        else
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20f);
    }

    private void SetBGMVolume(float value)
    {
        if (value <= 0.0001f)
            audioMixer.SetFloat("BGMVolume", -80f);
        else
            audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20f);
    }

    private void SetSFXVolume(float value)
    {
        if (value <= 0.0001f)
            audioMixer.SetFloat("SFXVolume", -80f);
        else
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20f);
    }

    // -------- 游戏玩法 ----------

    public void SetGhost(bool enabled)
    {
        GameManager.instance.SetCanGhost(enabled);
        SoundManager.instance.PlaySFX(1);
    }

    public void SetPreview(bool enabled)
    {
        GameManager.instance.SetCanPreview(enabled);
        SoundManager.instance.PlaySFX(1);
    }

    public void SetDestroy(bool enabled)
    {
        GameManager.instance.SetCanDestory(enabled);
        SoundManager.instance.PlaySFX(1);
    }

    // -------- 提示 ----------
    public void SetTips1(bool enabled)
    {
        tipe1.gameObject.SetActive(enabled);
        SoundManager.instance.PlaySFX(1);
    }

    public void SetTips2(bool enabled)
    {
        tipe2.gameObject.SetActive(enabled);
        SoundManager.instance.PlaySFX(1);
    }

    // -------- 退出 ----------
    public void MainMenu()
    {
        SoundManager.instance.PlaySFX(1);
        StartCoroutine(StartMainMenu());
    }

    public IEnumerator StartMainMenu()
    {
        yield return new WaitForSeconds(0.2f);

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        SoundManager.instance.PlaySFX(1);
        StartCoroutine(StartQuit());
    }

    public IEnumerator StartQuit()
    {
        yield return new WaitForSeconds(0.3f);

#if UNITY_EDITOR
        // 在 Unity 编辑器里 → 停止播放模式
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    // 在 Windows / Mac / Linux 构建里 → 正常退出游戏
    Application.Quit();

#elif UNITY_WEBGL
    // 在 WebGL 构建里 → 刷新当前网页 
    Application.ExternalCall("window.location.reload")

#else
    // 其它平台默认调用 Application.Quit
    Application.Quit();
#endif
    }

    // -------- UI 切换 ----------
    public void OpenPanel(GameObject panel)
    {
        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        tipsPanel.SetActive(false);
        panel.SetActive(panel);
        back.SetActive(true);

        GameManager.instance.SettinPausedg(true);
        SoundManager.instance.PlaySFX(1);
    }

    public void Back()
    {
        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        tipsPanel.SetActive(false);
        back.SetActive(false);

        GameManager.instance.SettinPausedg(false);
        SoundManager.instance.PlaySFX(1);
    }

    public void Back2()
    {
        graphicsPanel.SetActive(false);
        audioPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        tipsPanel.SetActive(false);
        back.SetActive(false) ;
    }


}
