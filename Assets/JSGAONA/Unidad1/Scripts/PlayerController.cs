
using UnityEngine;
using System.Collections;
namespace Assets.JSGAONA.Unidad1.Scripts {

    // Se emplea para obligar al GameObject asignar el componente para su correcto funcionamiento
    [RequireComponent(typeof(CharacterController))]

    // Se emplea este script para gestionar el controlador del personaje jugable
    public class PlayerController : MonoBehaviour {
        
        [Header("Adjust movement")]
        [SerializeField] private float speedMove;
        [SerializeField] private float speedRotation;
        [SerializeField] private float jumpForce = 3.0f;
        [SerializeField] private int maxJumpCount = 1;
        [SerializeField]  private float minFallVelocity = -2;
        [SerializeField] [Range(0, 5)] private float gravityMultiplier = 1;
        [SerializeField] private float dashDistance = 5.0f;
        [SerializeField] private float dashDuration = 2.0f;
        [SerializeField] private float dashCooldown = 3.0f;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Joystick joystick;

        [Header("Adjust to gorund")]
        [SerializeField] private float radiusDetectedGround = 0.2f;
        [SerializeField] private float groundCheckDistance = 0.0f;
        [SerializeField] private LayerMask ignoreLayer;

        // Variables ocultas desde el inspector de Unity
        private readonly float gravity = -9.8f;
        private int currentJumpCount = 0;
        public bool onGround = false;
        public float fallVelocity = 0;
        private Vector3 dirMove;
        private Vector3 positionFoot;
        private CharacterController charController;
        private bool isDashing = false;
        private bool canDash = true;
        private float originalGravityMultiplier;


        // Metodo de llamada de Unity, se llama una unica vez al iniciar el app, es el primer
        // metodo en ejecutarse, se realiza la asignacion de componentes
        private void Awake(){
            charController = GetComponent<CharacterController>();
            originalGravityMultiplier = gravityMultiplier;
        }
        
        
        // Metodo de llamada de Unity, se llama en cada frame del computador
        // Se realiza la logica de control del personaje jugable
        private void Update() {
            if (!isDashing) 
            {
                // Movimiento normal solo si no está en dash
                dirMove = new Vector3(joystick.Horizontal, 0, joystick.Vertical).normalized;

                // Rotación del personaje
                if (dirMove != Vector3.zero) 
                {
                    Quaternion targetRotation = Quaternion.LookRotation(dirMove);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speedRotation * Time.deltaTime);
                }

                // Gravedad
                if (onGround) 
                {
                    fallVelocity = Mathf.Max(minFallVelocity, fallVelocity + gravity * Time.deltaTime);
                } 
                else 
                {
                    fallVelocity += gravity * gravityMultiplier * Time.deltaTime;
                }

                dirMove *= speedMove;
                dirMove.y = fallVelocity;
                charController.Move(dirMove * Time.deltaTime);
            }

            // Ejemplo de input para dash (puedes cambiarlo a un botón UI)
            if (Input.GetKeyDown(KeyCode.Space) && canDash) 
            {
                TryDash();
            }
        }


        // Metodo de llamada de Unity, se llama en cada actualizacion constante 0.02 seg
        // Se realiza la logica de gestion de fisicas del motor
        private void FixedUpdate() {
            positionFoot = transform.position + Vector3.down * groundCheckDistance;
            onGround = Physics.CheckSphere(positionFoot, radiusDetectedGround, ~ignoreLayer);
        }

        private void TryDash() 
        {
            if (isDashing || !canDash) return;

            // Verificar obstáculos con Raycast
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, dashDistance, obstacleLayer)) 
            {
                Debug.Log("Dash bloqueado por obstáculo: " + hit.collider.name);
                return;
            }

            StartCoroutine(PerformDash());
        }

        // Corrutina para el dash
        private IEnumerator PerformDash() 
        {
            isDashing = true;
            canDash = false;
            float originalSpeed = speedMove;
            gravityMultiplier = 0; // Bonus: Desactiva gravedad durante el dash

            Vector3 dashDirection = transform.forward;
            float dashSpeed = dashDistance / dashDuration;
            float elapsedTime = 0f;

            while (elapsedTime < dashDuration) 
            {
                charController.Move(dashDirection * dashSpeed * Time.deltaTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Restablecer valores
            gravityMultiplier = originalGravityMultiplier;
            isDashing = false;

            // Cooldown
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }


        // Metodo publico que se emplea para gestionar los saltos del personaje
        public void Jump() {
            if (isDashing) return; // No saltar durante dash

            if (onGround) 
            {
                onGround = false;
                CountJump(false);
            } 
            else if (currentJumpCount < maxJumpCount) 
            {
                dirMove.y = 0;
                CountJump(true);
            }

        }


        // Metodo que se utiliza para poder controlar el contador de los saltos
        private void CountJump(bool accumulate) {
            currentJumpCount = accumulate ? (currentJumpCount + 1): 1;
            fallVelocity = jumpForce;
        }


    #if UNITY_EDITOR
        // Metodo de llamada de Unity, se emplea para visualizar en escena, acciones del codigo
        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            positionFoot = transform.position + Vector3.down * groundCheckDistance;
            Gizmos.DrawSphere(positionFoot, radiusDetectedGround);
            // Visualizar dirección del dash
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * dashDistance);
        }
    #endif
    }
}
