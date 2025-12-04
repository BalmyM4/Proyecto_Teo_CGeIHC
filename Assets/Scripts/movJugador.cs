using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class movJugador : MonoBehaviour
{
    public InputActionReference moveAction;
    public InputActionReference runAction;
    public InputActionReference jumpAction; // opcional, asignar en inspector si usas nuevo Input System

    public Transform cameraTransform;
    public float baseSpeed = 6f;
    public float runMultiplier = 3f;
    public bool includeVertical = false;  // normalmente false para movimiento en superficie

    // Nota: ahora puede haber múltiples planetas en escena.
    PlanetGravity[] allPlanets;          // cache de planetas
    PlanetGravity currentPlanet;         // planeta actualmente "activo" (el más cercano)

    CharacterController cc;
    Vector3 gravityVelocity;

    // --- estado de "pegado" a la superficie ---
    bool isStuckOnPlanet = false;
    PlanetGravity stuckPlanet = null;
    Vector3 stuckNormal = Vector3.up;
    public float jumpImpulse = 6f;      // impulso inicial al saltar
    public float alignSpeed = 20f;      // velocidad de alineado del up

    //Animación
    Animator animator;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        // Cachear todos los PlanetGravity presentes en la escena
        PlanetGravity[] allPlanets = FindObjectsByType<PlanetGravity>(FindObjectsSortMode.None);

        animator = GetComponent<Animator>();
        Debug.Log(animator);
    }

    void Update()
    {
        Movement();

        // Si estamos pegados y se pulsa salto -> despegar
        if (isStuckOnPlanet)
        {
            bool caminar = Input.GetKey("w");
            bool correr = Input.GetKey("left shift");

            //Para controlar la caminata
            if (caminar)
            {
                animator.SetBool("Caminando", true);
            }
            if (!caminar)
            {
                animator.SetBool("Caminando", false);
            }

            //Para controlar el sprint
            if (correr && caminar)
            {
                animator.SetBool("Corriendo", true);
            }
            if (!correr || !caminar)
            {
                animator.SetBool("Corriendo", false);
            }
            bool jumpPressed = false;
            if (jumpAction != null && jumpAction.action != null)
                jumpPressed = jumpAction.action.WasPerformedThisFrame();
            else if (Keyboard.current != null)
                jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;

            if (jumpPressed)
            {
                // aplicar impulso en dirección opuesta a la normal de la superficie
                gravityVelocity = -stuckNormal * jumpImpulse;
                isStuckOnPlanet = false;
                animator.SetBool("Piso", false);
                stuckPlanet = null;
            }
        }
    }

    void Movement()
    {
        // --- 0. seleccionar planeta activo (el más cercano) ---
        SelectNearestPlanet();

        // --- 1. Read input ---
        Vector2 move2D = moveAction != null ? moveAction.action.ReadValue<Vector2>() : Vector2.zero;
        float mult = (runAction != null && runAction.action.IsPressed()) ? runMultiplier : 1f;

        // --- 2. Compute desired motion ---
        Vector3 desiredMotion = Vector3.zero;

        if (move2D.sqrMagnitude > 0.0001f && cameraTransform != null)
        {
            if (isStuckOnPlanet)
            {
                // proyectar forward/right sobre el plano tangente definido por stuckNormal
                Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, stuckNormal).normalized;
                Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, stuckNormal).normalized;
                Vector3 planar = right * move2D.x + forward * move2D.y;
                desiredMotion = planar.normalized * (baseSpeed * mult);
            }
            else
            {
                // comportamiento normal (fuera de estar pegado)
                desiredMotion = FlyCamScene.CameraRelativeDirection(cameraTransform, move2D, includeVertical) * (baseSpeed * mult);
            }
        }

        // --- 3. Gravity handling usando el planeta seleccionado (si existe) ---
        if (!isStuckOnPlanet)
        {
            if (currentPlanet != null && !includeVertical)
            {
                Vector3 grav = currentPlanet.GetGravity(transform.position);

                bool grounded = grav.sqrMagnitude > 0f && IsGroundedPlanetary(grav.normalized);

                if (grounded)
                {
                    // pequeña fuerza hacia la superficie para mantener contacto
                    gravityVelocity = grav.normalized * 0.5f;
                    // notamos que OnControllerColliderHit será responsable de setear el estado "stuck"
                }
                else
                {
                    // acumular gravedad cuando estamos en el aire
                    gravityVelocity += grav * Time.deltaTime;
                }
            }
            else
            {
                gravityVelocity = Vector3.zero;
            }
        }
        else
        {
            // cuando estamos pegados, mantener una ligera corrección hacia la superficie
            gravityVelocity = -stuckNormal * 0.5f;
        }

        // --- 4. Orientación ---
        if (cameraTransform != null && !FlyCamScene.getCursor())
        {
            float yaw = cameraTransform.eulerAngles.y;
            Quaternion bodyYaw = Quaternion.Euler(0f, yaw, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, bodyYaw, 10f * Time.deltaTime);
        }

        if (isStuckOnPlanet)
        {
            // alinear 'up' con stuckNormal
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, stuckNormal) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 1f - Mathf.Exp(-alignSpeed * Time.deltaTime));
        }

        // --- 5. Apply movement ---
        Vector3 finalMotion = desiredMotion * Time.deltaTime + gravityVelocity * Time.deltaTime;
        cc.Move(finalMotion);
    }

    // Selecciona el planeta más cercano al jugador (por centro del planeta)
    void SelectNearestPlanet()
    {
        currentPlanet = null;
        if (allPlanets == null || allPlanets.Length == 0) return;

        float bestDistSqr = float.MaxValue;
        Vector3 pos = transform.position;

        for (int i = 0; i < allPlanets.Length; i++)
        {
            var p = allPlanets[i];
            if (p == null) continue;

            // distancia al centro del planeta (usa planetCenter si el PlanetGravity lo expone)
            Transform center = p.planetCenter != null ? p.planetCenter : p.transform;
            float d2 = (center.position - pos).sqrMagnitude;

            if (d2 < bestDistSqr)
            {
                bestDistSqr = d2;
                currentPlanet = p;
            }
        }

        // opcional: si el planeta seleccionado está fuera de su alcance, descartarlo
        if (currentPlanet != null)
        {
            // si PlanetGravity define maxGravityDistance, descartamos si estamos fuera:
            // (usamos reflexión suave: si la propiedad existe y es > 0, comprobar; si no, asumimos válido)
            // Para evitar dependencias fuertes, hacemos una comprobación simple con GetGravity
            if (currentPlanet.GetGravity(transform.position).sqrMagnitude < 1e-8f)
            {
                currentPlanet = null;
            }
        }
    }

    bool IsGroundedPlanetary(Vector3 gravityDirection)
    {
        if (cc.isGrounded) return true;

        float checkDistance = 0.15f;
        if (gravityDirection.sqrMagnitude < 1e-6f) return false;

        // Raycast en la dirección de la gravedad hacia el planeta
        return Physics.Raycast(transform.position, gravityDirection, checkDistance);
    }

    // OnControllerColliderHit para detectar contacto con planetas y fijar stuck state
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        PlanetGravity pg = hit.collider.GetComponent<PlanetGravity>();
        if (pg == null)
            pg = hit.collider.GetComponentInParent<PlanetGravity>();

        if (pg != null)
        {
            // normal radial aproximada desde el centro del planeta hacia el jugador
            Vector3 radialNormal = (transform.position - pg.transform.position).normalized;

            // comprobar la orientación del contacto real
            float dot = Vector3.Dot(hit.normal, radialNormal);

            // umbral para considerar "surface contact"
            if (dot > 0.2f)
            {
                isStuckOnPlanet = true;
                animator.SetBool("Piso", true);
                stuckPlanet = pg;
                stuckNormal = radialNormal;
                gravityVelocity = Vector3.zero;
            }
        }
    }
}
