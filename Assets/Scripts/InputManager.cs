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
    [Tooltip("�ƶ��¼��ظ����")]
    [SerializeField] private float moveRepeatRate;
    [Tooltip("��������¼��ظ����")]
    [SerializeField] private float softDropRepeatRate;
    [Tooltip("�ڴ�ʱ�����������������½�������ΪӲ����")]
    [SerializeField] private float hardDropDoubleTapTime;

    [Header("Settings")]
    [Tooltip("�Ƿ������������")]
    public bool enableInput = true;

    // �ڲ�״̬
    private float lastMoveTime = -999f;
    private float lastSoftDropTime = -999f;
    private float lastDownPressTime = -999f;

    private Keyboard keyboard;

    private void Awake()
    {
        // ��ȡ�����豸
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        // ���û�м�⵽���̣�ֱ�ӷ���
        if (keyboard == null || !enabled) return;

        HandleMovement();
        HandleRotation();
        HandleMirror();
        HandleDrop();
    }

    #region < �������� >
    /// <summary>
    /// �������Լ��̵��ƶ����룬��������Ӧ���ƶ��¼���
    /// </summary>
    /// <remarks>�˷��������ض��İ���������������Ӧ����������ʱ�����ƶ��¼���
    /// ֧�ֵ��ΰ��������Լ����ڿ������ظ��ʵ������ƶ�������</remarks>
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
    /// �������Լ��̵���ת���룬������ <see cref="OnRotate"/> �¼���
    /// </summary>
    /// <remarks>�˷������鵱ǰ֡���Ƿ���һ�������õ���ת���������¡������⵽�������£������� <see cref="OnRotate"/> �¼���</remarks>
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
    /// �������Լ��̵ľ������룬������ <see cref="OnMirror"/> �¼���
    /// </summary>
    /// <remarks>�˷������鵱ǰ֡���Ƿ���һ�������õľ��񰴼������¡������⵽�������£������� <see cref="OnMirror"/> �¼���</remarks>
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

        // ����״̬�������м����ɿ�ʱ
        if (!keyboard[mirrorKey1].isPressed && !keyboard[mirrorKey2].isPressed && !keyboard[mirrorKey3].isPressed)
        {
            mirrorPressed = false;
        }
    }

    /// <summary>
    /// �������Լ��̵��������룬��������Ӧ�������¼���
    /// </summary>
    /// <remarks>�˷��������ض��İ���������������������������¼���
    /// �����ָ����ʱ�������������ΰ���������򴥷�Ӳ���������
    /// ���򣬵����¸ü��򱣳ָü���סʱ�ᴥ���������������Ӧ���ظ��ʡ�</remarks>
    private void HandleDrop()
    {
        bool pressed = keyboard[softDropKey1].wasPressedThisFrame || keyboard[softDropKey2].wasPressedThisFrame || keyboard[softDropKey3].wasPressedThisFrame;
        bool held = keyboard[softDropKey1].isPressed || keyboard[softDropKey2].isPressed || keyboard[softDropKey3].isPressed;

        float now = Time.time;

        if (pressed)
        {
            if (now - lastDownPressTime <= hardDropDoubleTapTime)
            {
                lastDownPressTime = -999f;  // ���ã���ֹ������������ΪӲ����
                lastSoftDropTime = now;     // ����ͬ֡������
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
    /// �ⲿ�������ڸ����ƶ�����������ļ�ʱ����
    /// </summary>
    /// <remarks>�˷�������ʱ������ΪĬ��ֵ���Ӷ����֮ǰ�����м�ʱ���ݡ�</remarks>
    public void ResetDropTimers()
    {
        lastMoveTime = -999f;
        lastSoftDropTime = -999f;
        lastDownPressTime = -999f;
    }

    #endregion

    #region < ���� >
    /// <summary>
    /// �� Unity �༭���м�¼�ض������¼��ĵ�����Ϣ��
    /// </summary>
    /// <remarks>�˷����������õ��Թ���ʱ�� Unity �༭����ִ�С���ʹ�� Unity �� <see cref="Debug.Log"/> ��������¼��ʶ�������¼�����Ϣ����ʹ�� <see cref="Debug.LogWarning"/> ����¼δʶ����¼���</remarks>
    /// <param name="triggerEvent">Ҫ��¼�������¼������ơ�֧�ֵ�ֵ���� "OnMoveLeft"��"OnMoveRight"��"OnRotate"��"OnMirror"��"OnSoftDrop" �� "OnHardDrop"������ṩ���ǲ�֧�ֵ�ֵ������¼������Ϣ��</param>
    /// <param name="comment">��ѡ��ע�ͣ�������ӵ�������־�С����δָ������Ĭ��Ϊ���ַ�����</param>
    private void EventDebug(string triggerEvent, string comment = " ")
    {
#if UNITY_EDITOR
        if(!eventDebug) return;

        switch(triggerEvent)
        {
            case "OnMoveLeft":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMoveLeft �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnMoveRight":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMoveRight �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnRotate":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnRotate �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnMirror":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnMirror �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnSoftDrop":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnSoftDrop �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            case "OnHardDrop":
                Debug.Log("<color=#92BFD1><b>[InputManager]</b></color> <color=#4EC9B0>OnHardDrop �¼�����</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
            default:
                Debug.LogWarning("<color=#92BFD1><b>[InputManager]</b></color> <color=#F17D7C>δ֪������Ϣ</color>" + $"<i>{(comment == " " ? " " : $" <{comment}>")}</i>");
                break;
        }

#endif
    }
    #endregion
}
