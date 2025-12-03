// FrogEnemy.cs (Versión CORREGIDA - Sin deslizamiento)
using UnityEngine;
using System.Collections; // ← *** NECESARIO PARA IEnumerator ***


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FrogAI : MonoBehaviour
{
     [Header("Movimiento/Patrulla")]
     public float patrolSpeed = 1.2f;
     public float jumpHorizontalSpeed = 5f; // *** REDUCIDO de 10 a 5 ***
     public float jumpVerticalSpeed = 10f;  // *** AUMENTADO de 8 a 10 ***
     public bool patrol = true;
     public Transform groundCheck;
     public LayerMask groundLayer;
     public float groundCheckRadius = 0.12f;

     [Header("Detección y ataque")]
     public float detectionRange = 6f;
     public float minAttackDistance = 0.8f;
     public float maxAttackDistance = 3.5f;
     public float attackCooldown = 1.8f;
     public Transform tongueRoot;

     [Header("Referencias")]
     public Collider2D hurtBox;
     public AudioClip jumpSfx;
     public AudioClip tongueSfx;

     [Header("Tipo de Ataque")]
     public AttackType attackType = AttackType.Tongue;
     public enum AttackType { Tongue, Fireball }

     [Header("Ataque a Distancia (Fireball)")]
     public GameObject fireballPrefab;
     public Transform fireballSpawnPoint;
     public AudioClip fireballSfx;

     [Header("Combate por Contacto")]
     public int damagePorContacto = 10;
     public float knockbackForceHorizontal = 5f;
     public float knockbackForceVertical = 3f;

     [Header("Temporizadores de Acción")]
     public float jumpCooldown = 2.0f;
     private float jumpTimer = 0f;

     [Header("Comportamiento Sapo de Fuego")]
     public float safeDistance = 4f;
     public float verticalDetectionRange = 1.5f;

     Rigidbody2D rb;
     Animator anim;
     Transform player;
     bool facingRight = true;
     bool isGrounded = false;
     float attackTimer = 0f;

     void Awake()
     {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        if (groundCheck == null)
        {
              GameObject go = new GameObject("GroundCheck");
              go.transform.SetParent(transform);
              go.transform.localPosition = Vector3.down * 0.5f;
              groundCheck = go.transform;
        }
     }

     void Start()
     {
        var p = GameObject.FindGameObjectWithTag("Player");
        player = p != null ? p.transform : null;
        attackTimer = attackCooldown * 0.5f;
        if (tongueRoot != null)
        {
              var col = tongueRoot.GetComponent<Collider2D>();
              if (col != null) col.enabled = false;
        }
     }

     void Update()
     {
        attackTimer += Time.deltaTime;
        jumpTimer += Time.deltaTime; 
        
        CheckGrounded();
        UpdateAnimatorParams();

        if (player == null)
        {
              if (patrol) PatrolBehavior();
              return;
        }

        Vector3 distVector = player.position - transform.position;
        float distHorizontal = Mathf.Abs(distVector.x);
        float distVertical = Mathf.Abs(distVector.y);

        if (distHorizontal > detectionRange || distVertical > verticalDetectionRange)
        {
              if (patrol) PatrolBehavior();
        }
        else
        {
              FaceTarget(player.position);

              if (!isGrounded) return; 

              if (attackType == AttackType.Tongue)
              {
                    float attackRange = (minAttackDistance + maxAttackDistance) * 0.5f;

                    if (distHorizontal <= attackRange)
                    {
                       if (attackTimer >= attackCooldown)
                       {
                             AttackTongue();
                             attackTimer = 0f;
                       }
                    }
                    else
                    {
                       if (jumpTimer >= jumpCooldown)
                       {
                             JumpTowards(player.position);
                             jumpTimer = 0f;
                       }
                    }
              }
              else
              {
                    if (distHorizontal < safeDistance)
                    {
                       if (jumpTimer >= jumpCooldown)
                       {
                             JumpAway(player.position);
                             jumpTimer = 0f;
                       }
                    }
                    else
                    {
                       if (attackTimer >= attackCooldown)
                       {
                             AttackFireball();
                             attackTimer = 0f;
                       }
                    }
              }
           }
     }

     // *** NUEVO: Aplica fricción manual cuando está en el suelo ***
      void FixedUpdate()
      {
         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         // EVITA QUE EL JUGADOR EMPUJE AL SAPO
         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         if (isGrounded)
         {
            float velocidadActual = Mathf.Abs(rb.linearVelocity.x);
            
            if (velocidadActual > 0.5f)
            {
                  float nuevaVelocidad = rb.linearVelocity.x * 0.7f;
                  rb.linearVelocity = new Vector2(nuevaVelocidad, rb.linearVelocity.y);
            }
            else if (velocidadActual <= 0.5f && velocidadActual > 0.1f)
            {
                  rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
         }
      }


     void CheckGrounded()
     {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
     }

     void UpdateAnimatorParams()
    {
       if (HasParameter(anim, "isGrounded"))
       {
          anim.SetBool("isGrounded", isGrounded);
       }
    }

    bool HasParameter(Animator animator, string paramName)
    {
       foreach (AnimatorControllerParameter param in animator.parameters)
       {
          if (param.name == paramName)
          {
                return true;
          }
       }
       return false;
    }

     void PatrolBehavior()
     {
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;
        float checkDistance = 0.6f;
        Vector2 origin = transform.position;

        RaycastHit2D hitFront = Physics2D.Raycast(origin, dir, checkDistance, groundLayer);
        Vector2 groundAheadPos = (Vector2)groundCheck.position + dir * 0.4f;
        bool groundAhead = Physics2D.OverlapCircle(groundAheadPos, groundCheckRadius, groundLayer);

        if (hitFront.collider != null || !groundAhead)
        {
              Flip();
        }

        if (isGrounded && jumpTimer >= jumpCooldown)
        {
              float horizontal = facingRight ? 1f : -1f;
              Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);
              
              // *** CORREGIDO: Aplica velocidad directamente ***
              rb.linearVelocity = jumpVelocity;

              anim.SetTrigger("isJumping");
              jumpTimer = 0f;
        }
     }

     void Flip()
     {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1f : -1f);
        transform.localScale = s;
     }

     void FaceTarget(Vector3 targetPos)
     {
        if ((targetPos.x > transform.position.x && !facingRight) ||
              (targetPos.x < transform.position.x && facingRight))
        {
              Flip();
        }
     }

     void JumpTowards(Vector3 targetPos)
     {
        if (!isGrounded) return;
        Vector2 dir = (targetPos - transform.position);
        float horizontal = Mathf.Clamp(dir.x, -1f, 1f);
        
        Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);
        
        // *** CORREGIDO: Aplica velocidad directamente ***
        rb.linearVelocity = jumpVelocity;
        
        anim.SetTrigger("isJumping");
        if (jumpSfx != null) AudioSource.PlayClipAtPoint(jumpSfx, transform.position);
     }

     void JumpAway(Vector3 targetPos)
     {
        if (!isGrounded) return;
        float horizontal = Mathf.Sign(transform.position.x - targetPos.x);

        if (Mathf.Abs(transform.position.x - targetPos.x) < 0.1f)
        {
              horizontal = (Random.Range(0, 2) == 0) ? 1f : -1f;
        }

        Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);
        
        // *** CORREGIDO: Aplica velocidad directamente ***
        rb.linearVelocity = jumpVelocity;

        anim.SetTrigger("isJumping");
        if (jumpSfx != null) AudioSource.PlayClipAtPoint(jumpSfx, transform.position);
     }

     void AttackTongue()
     {
        anim.SetTrigger("attackTongue");
        if (tongueSfx != null) AudioSource.PlayClipAtPoint(tongueSfx, transform.position);
     }

     void AttackFireball()
     {
        anim.SetTrigger("attack");
        if (fireballSfx != null) AudioSource.PlayClipAtPoint(fireballSfx, transform.position);
     }

     public void SpawnFireball()
     {
        if (fireballPrefab == null)
        {
              Debug.LogError("Fireball Prefab no está asignado en " + gameObject.name);
              return;
        }
        
        Vector2 spawnPos = (fireballSpawnPoint != null) ? (Vector2)fireballSpawnPoint.position : (Vector2)transform.position;
        GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        ProyectilEnemigo proj = fireball.GetComponent<ProyectilEnemigo>();

        if (proj != null)
        {
              Vector2 direction = facingRight ? Vector2.right : Vector2.left;
              proj.SetDirection(direction);
        }
     }

     public void EnableTongue()
     {
        if (tongueRoot == null) return;
        var col = tongueRoot.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
     }

     public void DisableTongue()
     {
        if (tongueRoot == null) return;
        var col = tongueRoot.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
     }

     void OnDrawGizmosSelected()
     {
        if (groundCheck != null)
        {
              Gizmos.color = Color.yellow;
              Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
     }

      private void OnCollisionEnter2D(Collision2D collision)
      {
         // Solo procesa colisiones con el jugador
         if (!collision.gameObject.CompareTag("Player")) return;

         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         // 1. DAÑO AL JUGADOR (EFECTO CACTUS)
         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         SaludJugador saludDelJugador = collision.gameObject.GetComponent<SaludJugador>();
         if (saludDelJugador != null)
         {
            saludDelJugador.TakeDamage(damagePorContacto);
         }

         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         // 2. KNOCKBACK AL JUGADOR (EMPUJÓN)
         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         Rigidbody2D rbDelJugador = collision.gameObject.GetComponent<Rigidbody2D>();
         if (rbDelJugador != null)
         {
            rbDelJugador.linearVelocity = Vector2.zero;
            float direccionHorizontal = Mathf.Sign(collision.transform.position.x - transform.position.x);
            Vector2 fuerzaDeEmpuje = new Vector2(direccionHorizontal * knockbackForceHorizontal, knockbackForceVertical);
            rbDelJugador.AddForce(fuerzaDeEmpuje, ForceMode2D.Impulse);
         }

         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         // 3. SAPO NO SE MUEVE (RESETEA SU VELOCIDAD)
         // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
      }


      // *** NUEVA FUNCIÓN: Restaura el Rigidbody después de un tiempo ***
      System.Collections.IEnumerator RestaurarRigidbodyDespuesDe(RigidbodyType2D tipoOriginal, float segundos)
      {
         yield return new WaitForSeconds(segundos);
         
         // Vuelve a ser Dynamic (puede moverse normalmente)
         rb.bodyType = tipoOriginal;
         
         Debug.Log("¡Sapo vuelve a ser Dynamic!");
      }


}
