using UnityEngine;

public class PlayerPlanetOrientation : MonoBehaviour
{
    public PlanetGravity planetGravity;

    [Tooltip("How fast the player rotates to align with gravity")]
    public float alignmentSpeed = 10f;

    void Update()
    {
        if (planetGravity == null)
            return;

        // Get the up direction for current position
        Vector3 targetUp = planetGravity.GetUpDirection(transform.position);

        // Calculate the rotation needed to align with the planet surface
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;

        // Smoothly rotate toward the target orientation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, alignmentSpeed * Time.deltaTime);
    }
}
