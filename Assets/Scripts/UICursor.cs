using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // ������ϵͳ

public class UICursor : MonoBehaviour
{
    [Header("UI �������")]
    [SerializeField] private RectTransform cursorRect;      // UI ������
    [SerializeField] private Canvas canvas;                 // ���ص� Canvas
    [SerializeField] private Texture2D transparentCursor;   // ͸���Ĺ��ͼ

    [Header("�����������")]
    [SerializeField] private bool enableClickScale = true;  // �Ƿ����õ������
    [SerializeField] private float scaleFactor = 0.9f;      // ��Сʱ�ı���

    private Vector2 hotspot;
    private Vector3 originalScale;

    private void Awake()
    {
        // ʹ��͸��������ϵͳ��꣬��������������֤ UI ��������
        if (transparentCursor != null)
        {
            Cursor.SetCursor(transparentCursor, Vector2.zero, CursorMode.Auto);
        }
        Cursor.visible = true;

        if (cursorRect != null)
        {
            Image img = cursorRect.GetComponent<Image>();
            if (img != null)
            {
                // ��ֹ UI ����赲��ť���
                img.raycastTarget = false;

                if (img.sprite != null)
                {
                    hotspot = new Vector2(0, 0);
                }
            }

            // ��¼��ʼ����
            originalScale = cursorRect.localScale;
        }
    }

    private void Update()
    {
        if (cursorRect == null || canvas == null) return;

        // ����λ��
        Vector2 screenPos = Mouse.current.position.ReadValue();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos
        );

        cursorRect.anchoredPosition = localPos - hotspot;

        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            SoundManager.instance.PlaySFX(5);
        }

        // ���������
        if (enableClickScale && Mouse.current.leftButton.isPressed)
        {
            cursorRect.localScale = originalScale * scaleFactor;
        }
        else
        {
            cursorRect.localScale = originalScale;
        }
    }

    /// <summary>
    /// �л���� Sprite
    /// </summary>
    public void SetCursorSprite(Sprite sprite)
    {
        Image img = cursorRect.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            hotspot = new Vector2(0, 0);

            // ��֤�����赲���
            img.raycastTarget = false;
        }
    }
}






