using UnityEngine;

public class PlanetGravity : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("The center point of the planet (usually the planet's transform)")]
    public Transform planetCenter;

    [Tooltip("Gravity strength (positive value, direction is calculated automatically)")]
    public float gravityStrength = 9.81f;

    [Tooltip("Maximum distance from planet center where gravity still applies")]
    public float maxGravityDistance = 1000f;

    [Tooltip("If true, gravity weakens with distance (inverse square law)")]
    public bool useDistanceFalloff = false;

    [Tooltip("Reference distance for falloff calculation (surface radius)")]
    public float surfaceRadius = 50f;

    void Awake()
    {
        // If no planet center is assigned, use this object's transform
        if (planetCenter == null)
        {
            planetCenter = transform;
        }
    }

    /// <summary>
    /// Returns the gravity vector at the given world position. 
    /// The vector points toward the planet center with the configured strength.
    /// </summary>
    /// <param name="worldPosition">The position to calculate gravity for</param>
    /// <returns>Gravity vector (direction * strength)</returns>
    public Vector3 GetGravity(Vector3 worldPosition)
    {
        if (planetCenter == null)
            return Vector3.zero;

        // Calculate direction from position to planet center
        Vector3 directionToCenter = planetCenter.position - worldPosition;
        float distance = directionToCenter.magnitude;

        // No gravity if too far away
        if (distance > maxGravityDistance || distance < 0.001f)
            return Vector3.zero;

        // Normalize direction
        Vector3 gravityDirection = directionToCenter.normalized;

        // Calculate gravity magnitude
        float gravityMagnitude = gravityStrength;

        if (useDistanceFalloff && distance > surfaceRadius)
        {
            // Inverse square law: gravity = g * (r0/r)^2
            float ratio = surfaceRadius / distance;
            gravityMagnitude = gravityStrength * ratio * ratio;
        }

        return gravityDirection * gravityMagnitude;
    }

    /// <summary>
    /// Returns the normalized direction toward the planet center (down direction for the player)
    /// </summary>
    /// <param name="worldPosition">The position to calculate direction for</param>
    /// <returns>Normalized direction pointing to planet center</returns>
    public Vector3 GetGravityDirection(Vector3 worldPosition)
    {
        if (planetCenter == null)
            return Vector3.down;

        Vector3 directionToCenter = planetCenter.position - worldPosition;

        if (directionToCenter.sqrMagnitude < 0.001f)
            return Vector3.down;

        return directionToCenter.normalized;
    }

    /// <summary>
    /// Returns the up direction for an object at the given position (opposite of gravity)
    /// </summary>
    /// <param name="worldPosition">The position to calculate up direction for</param>
    /// <returns>Normalized up direction (away from planet center)</returns>
    public Vector3 GetUpDirection(Vector3 worldPosition)
    {
        return -GetGravityDirection(worldPosition);
    }

    /// <summary>
    /// Returns the distance from the given position to the planet's surface
    /// </summary>
    /// <param name="worldPosition">The position to measure from</param>
    /// <returns>Distance to surface (negative if inside the planet)</returns>
    public float GetAltitude(Vector3 worldPosition)
    {
        if (planetCenter == null)
            return 0f;

        float distanceToCenter = Vector3.Distance(worldPosition, planetCenter.position);
        return distanceToCenter - surfaceRadius;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform center = planetCenter != null ? planetCenter : transform;
        
        // Draw surface radius
        Gizmos.color = Color. green;
        Gizmos.DrawWireSphere(center.position, surfaceRadius);
        
        // Draw max gravity distance
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(center.position, maxGravityDistance);
    }
#endif
}
