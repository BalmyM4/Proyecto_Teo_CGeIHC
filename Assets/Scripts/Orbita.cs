using UnityEngine;

public class Orbita : MonoBehaviour
{
    [Tooltip("Ejes de rotación para el plano orbital (grados/segundo)")]
    public Vector3 velocidadOrbital = new Vector3(0f, 36f, 0f); // 10s por vuelta ~ 36°/s

    void Update()
    {
        // Rota el pivote (este GameObject) en espacio local
        transform.Rotate(velocidadOrbital * Time.deltaTime, Space.Self);
    }
}
