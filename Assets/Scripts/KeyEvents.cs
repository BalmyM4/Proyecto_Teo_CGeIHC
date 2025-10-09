using UnityEngine;
using UnityEngine.Events;

public class KeyEvents : MonoBehaviour
{
    [Header("Tecla a detectar")]
    public KeyCode key = KeyCode.Space;

    [Header("Eventos")]
    public UnityEvent onKeyDown;
    public UnityEvent onKey;
    public UnityEvent onKeyUp;

    void Update()
    {
        if (Input.GetKeyDown(key))
        {
            onKeyDown?.Invoke(); 
        }

        if (Input.GetKey(key))
        {
            onKey?.Invoke(); 
        }

        if (Input.GetKeyUp(key))
        {
            onKeyUp?.Invoke(); 
        }
    }
}
