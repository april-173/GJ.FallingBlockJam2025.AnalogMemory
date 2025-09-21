using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Debug")]
    public bool eventDebug;

    public event Action OnMoveLeft;
    public event Action OnMoveRight;
    public event Action OnRotate;
    public event Action OnMirror;
    public event Action OnSoftDrop;
    public event Action OnHardDrop;

    [Header("Key bindings")]
    [SerializeField] private Key leftKey1 = Key.LeftArrow;
    [SerializeField] private Key leftKey2 = Key.A;
    [SerializeField] private Key rightKey1 = Key.RightArrow;
    [SerializeField] private Key rightKey2 = Key.D;
    [SerializeField] private Key softDropKey1 = Key.DownArrow;
    [SerializeField] private Key softDropKey2 = Key.Z;
    [SerializeField] private Key softDropKey3 = Key.S;
    [SerializeField] private Key rotateKey1 = Key.UpArrow;
    [SerializeField] private Key rotateKey2 = Key.X;
    [SerializeField] private Key rotateKey3 = Key.W;
    [SerializeField] private Key mirrorKey1 = Key.LeftCtrl;
    [SerializeField] private Key mirrorKey2 = Key.RightCtrl;
    [SerializeField] private Key mirrorKey3 = Key.C;

    [Header("Timing")]
    [Tooltip("移动事件重复间隔")]
    [SerializeField] private float moveRepeatRate;
    [Tooltip("软下落的事件重复间隔")]
    [SerializeField] private float softDropRepeatRate;
    [Tooltip("在此时间内连续按下两次下降键被判为硬下落")]
    [SerializeField] private float hardDropDoubleTapTime;

    [Header("Settings")]
    [Tooltip("是否允许进行输入")]
    public bool enableInput = true;

    // 内部状态
    private float lastMoveTime = -999f;
    private float lastSoftDropTime = -999f;
    private float lastDownPressTime = -999f;

    private Keyboard keyboard;

    private void Awake()
    {
        // 获取键盘设备
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        // 如果没有检测到键盘，直接返回
        if (keyboard == null || !enabled) return;

        HandleMovement();
        HandleRotation();
        HandleMirror();
        HandleDrop();
    }

    #region < 操作处理 >
    /// <summary>
    /// 处理来自键盘的移动输入，并触发相应的移动事件。
    /// </summary>
    /// <remarks>此方法监听特定的按键操作，并在相应按键被按下时调用移动事件。
    /// 支持单次按键操作以及基于可配置重复率的连续移动操作。</remarks>
    private void HandleMovement()
    {
        float now = Time.time;

        if (keyboard[leftKey1].wasPressedThisFrame || keyboard[leftKey2].wasPressedThisFrame)
        {
            OnMoveLeft?.Invoke(); lastMoveTime = now;
            EventDebug("OnMoveLeft","KeyDown");
        }

        if (keyboard[rightKey1].wasPressedThisFrame || keyboard[rightKey2].wasPressedThisFrame) 
        {
            OnMoveRight?.Invoke(); lastMoveTime = now;
            EventDebug("OnMoveRight","KeyDown");
        }

        if (keyboard[leftKey1].isPressed || keyboard[leftKey2].isPressed)
        {
            if(now - lastMoveTime >= moveRepeatRate)
            {
                OnMoveLeft?.Invoke(); lastMoveTime = now;
                EventDebug("OnMoveLeft","KeyHold");
            }
        }
        if (keyboard[rightKey1].isPressed || keyboard[rightKey2].isPressed)
        {
            if(now - lastMoveTime >= moveRepeatRate)
            {
                OnMoveRight?.Invoke(); lastMoveTime = now;
                EventDebug("OnMoveRight","KeyHold");
            }
        }
    }

    /// <summary>
    /// 处理来自键盘的旋转输入，并触发 <see cref="OnRotate"/> 事件。
    /// </summary>
    /// <remarks>此方法会检查当前帧内是否有一个已配置的旋转按键被按下。如果检测到按键按下，则会调用 <see cref="OnRotate"/> 事件。</remarks>
    private void HandleRotation()
    {
        if (keyboard[rotateKey1].wasPressedThisFrame || keyboard[rotateKey2].wasPressedThisFrame || keyboard[rotateKey3].wasPressedThisFrame) 
        {
            OnRotate?.Invoke(); 
            EventDebug("OnRotate");
        }
    }

    private bool mirrorPressed = false;

    /// <summary>
    /// 处理来自键盘的镜像输入，并触发 <see cref="OnMirror"/> 事件。
    /// </summary>
    /// <remarks>此方法会检查当前帧内是否有一个已配置的镜像按键被按下。如果检测到按键按下，则会调用 <see cref="OnMirror"/> 事件。</remarks>
    private void HandleMirror()
    {
        bool pressed = keyboard[mirrorKey1].wasPressedThisFrame
            || keyboard[mirrorKey2].wasPressedThisFrame
            || keyboard[mirrorKey3].wasPressedThisFrame;

        if (pressed && !mirrorPressed)
        {
            mirrorPressed = true;
            OnMirror?.Invoke();
            EventDebug("OnMirror");
        }

        // 重置状态，当所有键都松开时
        if (!keyboard[mirrorKey1].isPressed && !keyboard[mirrorKey2].isPressed && !keyboard[mirrorKey3].isPressed)
        {
            mirrorPressed = false;
        }
    }

    /// <summary>
    /// 处理来自键盘的下落输入，并触发相应的下落事件。
    /// </summary>
    /// <remarks>此方法监听特定的按键操作，并根据情况调用下落事件。
    /// 如果在指定的时间间隔内连续两次按下落键，则触发硬下落操作。
    /// 否则，当按下该键或保持该键按住时会触发软下落操作，并应用重复率。</remarks>
    private void HandleDrop()
    {
        bool pressed = keyboard[softDropKey1].wasPressedThisFrame || keyboard[softDropKey2].wasPressedThisFrame || keyboard[softDropKey3].wasPressedThisFrame;
        bool held = keyboard[softDropKey1].isPressed || keyboard[softDropKey2].isPressed || keyboard[softDropKey3].isPressed;

        float now = Time.time;

        if (pressed)
        {
            if (now - lastDownPressTime <= hardDropDoubleTapTime)
            {
                lastDownPressTime = -999f;  // 重置，防止三连击被误判为硬下落
                lastSoftDropTime = now;     // 抑制同帧软下落
                OnHardDrop?.Invoke();
                EventDebug("OnHardDrop");
                return;
            }

            lastDownPressTime = now;
            lastSoftDropTime = now;
            OnSoftDrop?.Invoke();
            EventDebug("OnSoftDrop", "KeyDown");
        }

        if (held)
        {
            if (now - lastSoftDropTime >= softDropRepeatRate)
            {
                OnSoftDrop?.Invoke();lastSoftDropTime = now;
                EventDebug("OnSoftDrop");
            }
        }
    }

    /// <summary>
    /// 外部重置用于跟踪移动和下落操作的计时器。
    /// </summary>
    /// <remarks>此方法将计时器重置为默认值，从而清除之前的所有计时数据。</remarks>
    public void ResetDropTimers()
    {
        lastMoveTime = -999f;
        lastSoftDropTime = -999f;
        lastDownPressTime = -999f;
    }

    #endregion

    #region < 调试 >
    /// <summary>
    /// 在 Unity 编辑器中记录特定输入事件的调试信息。
    /// </summary>
    /// <remarks>此方法仅在启用调试功能时在 Unity 编辑器中执行。它使用 Unity 的 <see cref="Debug.Log"/> 方法来记录已识别输入事件的消息，并使用 <see cref="Debug.LogWarning"/> 来记录未识别的事件。</remarks>
    /// <param name="triggerEvent">要记录的输入事件的名称。支持的值包括 "OnMoveLeft"、"OnMoveRight"、"OnRotate"、"OnMirror"、"OnSoftDrop" 和 "OnHardDrop"。如果提供的是不支持的值，则会记录警告消息。</param>
    /// <param name="comment">可选的注释，用于添加到调试日志中。如果未指定，则默认为空字符串。</param>
    private void EventDebug(string triggerEvent, string comment = " ")
    {
#if UNITY_EDITOR
        if(!eventDebug) return;

        switch(triggerEvent)
        {
            case "OnMoveLeft":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMoveLeft 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnMoveRight":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMoveRight 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnRotate":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnRotate 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnMirror":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMirror 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnSoftDrop":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnSoftDrop 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnHardDrop":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnHardDrop 事件触发</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            default:
                Debug.LogWarning("<color=#92BFD1><b>[InputManager]</b></color> <color=#F17D7C>未知调试信息</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
        }

#endif
    }
    #endregion
}
