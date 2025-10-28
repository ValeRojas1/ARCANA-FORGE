using UnityEngine;
using UnityEngine.AI; // ¡Importante añadir esta línea!

public class FrogAI_NavMesh : MonoBehaviour
{
    [Header("Navigation")]
    public float patrolRadius = 7f; // Radio en el que buscará un nuevo punto para patrullar
    private NavMeshAgent agent;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;
    private float attackTimer;

    // ... (variables de Animator, Player, etc. que ya tenías)
    private Transform player;
    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();

        // Configuración para que el agente funcione en 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        GoToRandomPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Atacar
            agent.isStopped = true; // Detiene el movimiento del agente
            animator.SetBool("isJumping", false);
            // Lógica de ataque (girar, disparar trigger de animación)
            // ...
        }
        else
        {
            // Perseguir al jugador
            agent.isStopped = false; // Reanuda el movimiento
            agent.SetDestination(player.position);
            animator.SetBool("isJumping", true);
        }

        // Si el agente llega a su destino de patrulla, busca uno nuevo
        if (!agent.pathPending && agent.remainingDistance < 0.5f && distanceToPlayer > attackRange)
        {
            GoToRandomPatrolPoint();
        }
    }

    void GoToRandomPatrolPoint()
    {
        Vector2 randomDirection = Random.insideUnitCircle * patrolRadius;
        Vector3 targetPosition = transform.position + new Vector3(randomDirection.x, randomDirection.y, 0);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
            animator.SetBool("isJumping", true);
        }
    }

    // Añade esta función al final de tu script FrogAI_NavMesh.cs
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Comprueba si el objeto que nos ha tocado tiene el nombre "StompBox"
        // Y si el jugador está cayendo (para asegurar que es un pisotón)
        Rigidbody2D playerRb = other.GetComponentInParent<Rigidbody2D>();
        
        if (other.gameObject.name == "StompBox" && playerRb != null && playerRb.linearVelocity.y < -0.1f)
        {
            Debug.Log("¡Sapo aplastado!");
            // Aquí pones la lógica para que el sapo reciba daño o muera
            // Por ejemplo, podrías llamar a una función TakeDamage()
            
            // Y para darle un pequeño rebote al jugador:
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 8f);
            
            // Desactivar o destruir al sapo
            Destroy(gameObject);
        }
    }
}