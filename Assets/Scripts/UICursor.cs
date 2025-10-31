using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 新输入系统

public class UICursor : MonoBehaviour
{
    [Header("UI 光标设置")]
    [SerializeField] private RectTransform cursorRect;      // UI 光标对象
    [SerializeField] private Canvas canvas;                 // 挂载的 Canvas
    [SerializeField] private Texture2D transparentCursor;   // 透明的光标图

    [Header("点击缩放设置")]
    [SerializeField] private bool enableClickScale = true;  // 是否启用点击缩放
    [SerializeField] private float scaleFactor = 0.9f;      // 缩小时的比例

    private Vector2 hotspot;
    private Vector3 originalScale;

    private void Awake()
    {
        // 使用透明光标替代系统光标，但不隐藏它，保证 UI 交互正常
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
                // 禁止 UI 光标阻挡按钮点击
                img.raycastTarget = false;

                if (img.sprite != null)
                {
                    hotspot = new Vector2(0, 0);
                }
            }

            // 记录初始缩放
            originalScale = cursorRect.localScale;
        }
    }

    private void Update()
    {
        if (cursorRect == null || canvas == null) return;

        // 更新位置
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

        // 检测点击缩放
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
    /// 切换光标 Sprite
    /// </summary>
    public void SetCursorSprite(Sprite sprite)
    {
        Image img = cursorRect.GetComponent<Image>();
        if (img != null && sprite != null)
        {
            img.sprite = sprite;
            hotspot = new Vector2(0, 0);

            // 保证不会阻挡点击
            img.raycastTarget = false;
        }
    }
}






