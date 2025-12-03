using UnityEngine;

public class GiroPropio : MonoBehaviour
{
    public Vector3 velocidadGiro = new Vector3(0f, 90f, 0f); // 90°/s
    void Update()
    {
        transform.Rotate(velocidadGiro * Time.deltaTime, Space.Self);
    }
}
