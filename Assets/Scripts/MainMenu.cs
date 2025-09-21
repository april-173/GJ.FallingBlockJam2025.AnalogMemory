using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private Keyboard keyboard;

    private void Awake()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (keyboard[Key.Escape].wasPressedThisFrame)
        {
            StartCoroutine(StartQuit());
        }
    }

    public void ExecuteGame()
    {
        SoundManager.instance.PlaySFX(1);
        StartCoroutine(StartExecute() );
    }

    private IEnumerator StartExecute()
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        SoundManager.instance.PlaySFX(1);
        StartCoroutine(StartQuit());
    }

    private IEnumerator StartQuit()
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
}
