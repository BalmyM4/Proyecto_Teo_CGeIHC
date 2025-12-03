using UnityEngine;

public class RotacionPlanetaEjes : MonoBehaviour
{
    [Header("Velocidad de rotación")]
    public float velocidadX = 0f;
    public float velocidadY = 10f;
    public float velocidadZ = 0f;

    [Header("Usar rotación en espacio local")]
    public bool usarEjeLocal = true;

    void Update()
    {
        Vector3 rotacion = new Vector3(
            velocidadX * Time.deltaTime,
            velocidadY * Time.deltaTime,
            velocidadZ * Time.deltaTime
        );

        if (usarEjeLocal)
            transform.Rotate(rotacion, Space.Self);
        else
            transform.Rotate(rotacion, Space.World);
    }
}
