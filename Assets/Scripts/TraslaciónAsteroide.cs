using UnityEngine;

public class MovimientoCurvoSimple : MonoBehaviour
{
    public float distancia = 5f;    // qué tanto avanza hacia adelante
    public float alturaCurva = 2f;  // tamaño de la curva (solo efecto visual)
    public float velocidad = 1f;

    private Vector3 posicionInicial;
    private float t = 0f;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime * velocidad;
        if (t > 1f) t = 0f;

        // Movimiento hacia adelante en el eje Z (puedes cambiarlo)
        float desplazamiento = Mathf.Lerp(0, distancia, t);

        // Curva sin cambiar la altura original
        float offsetY = Mathf.Sin(t * Mathf.PI) * alturaCurva;

        // Nueva posición
        transform.position = new Vector3(
            posicionInicial.x,
            posicionInicial.y + offsetY,   // sube y baja sin perder altura base
            posicionInicial.z + desplazamiento
        );
    }
}


