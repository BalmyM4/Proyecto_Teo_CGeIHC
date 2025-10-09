using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamScene : MonoBehaviour
{
    [Header("Actions")]
    public InputActionReference moveAction;      
    public InputActionReference lookAction;
    public InputActionReference runAction;       
    public InputActionReference lookEnableAction;

    [Header("Movement")]
    public float baseSpeed = 6f;
    public float runMultiplier = 3f;

    [Header("Mouse Look")]
    [Tooltip("X/Y")]
    public Vector2 mouseSensitivity = new Vector2(0.1f, 0.1f);
    [Range(0f, 20f)] public float lookSmoothing = 0.5f;

    float yaw, pitch;
    Vector2 smoothedLook;

    void OnEnable()
    {
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        runAction?.action.Enable();
        lookEnableAction?.action.Enable();
    }

    void OnDisable()
    {
        moveAction?.action.Disable();
        lookAction?.action.Disable();
        runAction?.action.Disable();
        lookEnableAction?.action.Disable();
        SetCursorLocked(false);
    }

    void Start()
    {
        var e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180 ? e.x - 360f : e.x;
    }

    void Update()
    {
        HandleCursorLock();
        HandleLook();
        HandleMove();
    }

    void HandleCursorLock()
    {
        bool wantLook = lookEnableAction != null && lookEnableAction.action.IsPressed();
        if (wantLook && Cursor.lockState != CursorLockMode.Locked)
            SetCursorLocked(true);
        if (!wantLook && Cursor.lockState == CursorLockMode.Locked)
            SetCursorLocked(false);

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(false);
    }

    void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 delta = lookAction != null ? lookAction.action.ReadValue<Vector2>() : Vector2.zero;
        Vector2 scaled = new Vector2(delta.x * mouseSensitivity.x, delta.y * mouseSensitivity.y);

        if (lookSmoothing > 0.01f)
        {
            float t = 1f - Mathf.Exp(-lookSmoothing * Time.deltaTime);
            smoothedLook = Vector2.Lerp(smoothedLook, scaled, t);
        }
        else smoothedLook = scaled;

        yaw   += smoothedLook.x;
        pitch -= smoothedLook.y;
        pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMove()
    {
        Vector2 move2D = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        if (move2D.sqrMagnitude < 0.0001f) return; // sin teclas => no se mueve

        // Dirección relativa a la cámara (W/S incluyen componente vertical según el pitch)
        Vector3 dir = transform.right * move2D.x + transform.forward * move2D.y;

        float mult = (runAction != null && runAction.action.IsPressed()) ? runMultiplier : 1f;
        transform.position += dir * (baseSpeed * mult * Time.deltaTime);
    }

    void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}