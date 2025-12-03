using UnityEngine;

public class MiraCamara : MonoBehaviour
{
    [Header("Cámara objetivo (si se deja vacío usa Main Camera)")]
    public Transform camara;

    [Header("Corrección de frente (0 o 180 según se vea)")]
    public float yawOffset = 180f;

    [Header("Bloquear posición para evitar cualquier traslación")]
    public bool bloquearPosicion = true;

    // Cache
    private Vector3  posBase;
    private Vector3  eulerBase;   
    private Transform tf;

    void Awake()
    {
        tf = transform;
    }

    void OnEnable()
    {
        // Guardamos estado base sólo cuando se habilita
        posBase  = tf.position;
        eulerBase = tf.rotation.eulerAngles;

        if (camara == null)
        {
            Camera m = Camera.main;
            if (m != null) camara = m.transform;
        }
    }

    void LateUpdate()
    {
        if (camara == null) return;

        // Asegura NO traslación
        if (bloquearPosicion && tf.position != posBase)
            tf.position = posBase;

        // Calcula yaw hacia la cámara
        Vector3 dir = camara.position - tf.position;            
        Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
        float yaw = look.eulerAngles.y + yawOffset;            

        tf.rotation = Quaternion.Euler(eulerBase.x, yaw, eulerBase.z);
    }
}
