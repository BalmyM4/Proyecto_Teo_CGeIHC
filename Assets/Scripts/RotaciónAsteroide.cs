using UnityEngine;

public class RotacionIrregular : MonoBehaviour
{
    public float velocidad = 30f;

    void Update()
    {
        transform.Rotate(
            velocidad * Time.deltaTime * Random.Range(0.8f, 1.2f),
            velocidad * Time.deltaTime * Random.Range(0.8f, 1.2f),
            velocidad * Time.deltaTime * Random.Range(0.8f, 1.2f)
        );
    }
}
