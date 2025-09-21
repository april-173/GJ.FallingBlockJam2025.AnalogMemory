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
        // �� Unity �༭���� �� ֹͣ����ģʽ
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
    // �� Windows / Mac / Linux ������ �� �����˳���Ϸ
    Application.Quit();

#elif UNITY_WEBGL
    // �� WebGL ������ �� ˢ�µ�ǰ��ҳ 
    Application.ExternalCall("window.location.reload")

#else
    // ����ƽ̨Ĭ�ϵ��� Application.Quit
    Application.Quit();
#endif

    }
}
