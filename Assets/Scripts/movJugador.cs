using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movJugador : MonoBehaviour
{
    public InputActionReference moveAction;
    public InputActionReference runAction;
    public Transform cameraTransform; // Cámara principal
    public float baseSpeed = 6f;
    public float runMultiplier = 3f;
    public bool includeVertical = true; // Incluir verticalidad

    CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
    }


    void Update()
    {
        Movement();
    }

    void Movement()
    {
        Vector2 move2D = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        if (move2D.sqrMagnitude < 0.0001f) return;

        Vector3 dir = FlyCamScene.CameraRelativeDirection(cameraTransform, move2D, includeVertical);

        float mult = (runAction != null && runAction.action.IsPressed()) ? runMultiplier : 1f;
        Vector3 motion = dir * (baseSpeed * mult);

        // Añade gravedad si quieres:
        //motion.y += Physics.gravity.y * Time.deltaTime;
        if (!FlyCamScene.getCursor())
        {
            float yaw = cameraTransform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }


        cc.Move(motion * Time.deltaTime);
    }

}
