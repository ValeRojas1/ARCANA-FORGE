// FrogEnemy.cs (Versión final con Patrulla de Salto y ForceMode Corregido)
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class FrogAI : MonoBehaviour
{
     [Header("Movimiento/Patrulla")]
     public float patrolSpeed = 1.2f;
     public float jumpHorizontalSpeed = 10f; // VELOCIDAD horizontal del salto
     public float jumpVerticalSpeed = 8f;    // VELOCIDAD vertical (altura) del salto   
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
   	 public AttackType attackType = AttackType.Tongue; // Por defecto es Melee
   	 public enum AttackType { Tongue, Fireball } // Nuestros 2 tipos

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
     	 	 // Actualiza los temporizadores
     	 	 attackTimer += Time.deltaTime;
     	 	 jumpTimer += Time.deltaTime; 
     	 	 
     	 	 CheckGrounded();
     	 	 UpdateAnimatorParams();

     	 	 if (player == null)
     	 	 {
       	 	 	 // Si no hay jugador, solo patrulla
       	 	 	 if (patrol) PatrolBehavior();
       	 	 	 return;
     	 	 }

     	 	 // --- NUEVA LÓGICA DE DETECCIÓN (X, Y por separado) ---
     	 	 Vector3 distVector = player.position - transform.position;
     	 	 float distHorizontal = Mathf.Abs(distVector.x);
     	 	 float distVertical = Mathf.Abs(distVector.y);

     	 	 // ¿El jugador está fuera de rango (horizontal O verticalmente)?
     	 	 if (distHorizontal > detectionRange || distVertical > verticalDetectionRange)
     	 	 {
       	 	 	 // 1. JUGADOR LEJOS O EN OTRA PLATAFORMA -> PATRULLAR
       	 	 	 if (patrol) PatrolBehavior();
     	 	 }
     	 	 else
     	 	 {
       	 	 	 // 2. ¡JUGADOR CERCA! (En rango H y V) -> MODO COMBATE
       	 	 	 FaceTarget(player.position);
       	 	 	 //anim.SetBool("isWalking", false); // Deja de caminar/patrullar

       	 	 	 // Si estoy en el aire, no tomo decisiones.
       	 	 	 if (!isGrounded) return; 

       	 	 	 if (attackType == AttackType.Tongue)
       	 	 	 {
         	 	 	 	 // --- LÓGICA SAPO NORMAL (MELEE) ---
         	 	 	 	 float attackRange = (minAttackDistance + maxAttackDistance) * 0.5f;

         	 	 	 	 if (distHorizontal <= attackRange)
         	 	 	 	 {
           	 	 	 	 	 // ESTOY EN RANGO: Atacar (si el cooldown lo permite)
           	 	 	 	 	 if (attackTimer >= attackCooldown)
           	 	 	 	 	 {
             	 	 	 	 	 	 AttackTongue();
             	 	 	 	 	 	 attackTimer = 0f;
           	 	 	 	 	 }
         	 	 	 	 }
         	 	 	 	 else
         	 	 	 	 {
           	 	 	 	 	 // ESTOY FUERA DE RANGO: Acercarme (si el cooldown de salto lo permite)
           	 	 	 	 	 if (jumpTimer >= jumpCooldown)
           	 	 	 	 	 {
             	 	 	 	 	 	 JumpTowards(player.position);
             	 	 	 	 	 	 jumpTimer = 0f;
           	 	 	 	 	 }
         	 	 	 	 }
       	 	 	 }
       	 	 	 else // attackType == AttackType.Fireball
       	 	 	 {
         	 	 	 	 // --- LÓGICA SAPO DE FUEGO (RANGED) ---
         	 	 	 	 if (distHorizontal < safeDistance)
         	 	 	 	 {
           	 	 	 	 	 // ¡DEMASIADO CERCA!: Saltar hacia atrás (si el cooldown lo permite)
           	 	 	 	 	 if (jumpTimer >= jumpCooldown)
           	 	 	 	 	 {
             	 	 	 	 	 	 JumpAway(player.position);
             	 	 	 	 	 	 jumpTimer = 0f;
           	 	 	 	 	 }
         	 	 	 	 }
         	 	 	 	 else
         	 	 	 	 {
           	 	 	 	 	 // A DISTANCIA SEGURA: Atacar (si el cooldown lo permite)
           	 	 	 	 	 if (attackTimer >= attackCooldown)
           	 	 	 	 	 {
             	 	 	 	 	 	 AttackFireball();
             	 	 	 	 	 	 attackTimer = 0f;
           	 	 	 	 	 }
         	 	 	 	 }
       	 	 	 }
       	 	 }
   	 }

   	 void CheckGrounded()
   	 {
     	 	 isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
   	 }

   	 void UpdateAnimatorParams()
   	 {
     	 	 anim.SetBool("isGrounded", isGrounded);
     	 	 //anim.SetFloat("velocityY", rb.linearVelocity.y);
   	 }

    // --- FUNCIÓN PatrolBehavior() CORREGIDA (PARA SALTAR) ---
   	 void PatrolBehavior()
   	 {
     	 	 // 1. Lógica para detectar paredes o bordes
     	 	 Vector2 dir = facingRight ? Vector2.right : Vector2.left;
     	 	 float checkDistance = 0.6f;
     	 	 Vector2 origin = transform.position;

     	 	 RaycastHit2D hitFront = Physics2D.Raycast(origin, dir, checkDistance, groundLayer);
     	 	 Vector2 groundAheadPos = (Vector2)groundCheck.position + dir * 0.4f;
     	 	 bool groundAhead = Physics2D.OverlapCircle(groundAheadPos, groundCheckRadius, groundLayer);

     	 	 // Si hay una pared o no hay suelo, gira.
     	 	 if (hitFront.collider != null || !groundAhead)
     	 	 {
       	 	 	 Flip();
     	 	 }

     	 	 // --- LÓGICA DE "SALTO DE PATRULLA" ---

     	 	 // 2. Si estamos en el suelo Y nuestro temporizador de salto está listo...
     	 	 if (isGrounded && jumpTimer >= jumpCooldown)
     	 	 {
       	 	 	 // 3. Saltamos en la dirección que estamos mirando
       	 	 	 float horizontal = facingRight ? 1f : -1f;

       	 	 	 // 4. Calculamos la VELOCIDAD de salto (más X que Y)
       	 	 	 Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);

       	 	 	 // 5. Calculamos la FUERZA (Velocidad * Masa) para vencer la Masa de 1000
       	 	 	 Vector2 forceToApply = jumpVelocity * rb.mass;

       	 	 	 // 6. Aplicamos el salto (con el ForceMode correcto)
       	 	 	 rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Limpia velocidad horizontal anterior
       	 	 	 rb.AddForce(forceToApply, ForceMode2D.Impulse);

       	 	 	 // 7. ¡Disparamos la animación de salto y reiniciamos el timer!
       	 	 	 anim.SetTrigger("isJumping");
       	 	 	 jumpTimer = 0f; // Reinicia el temporizador de salto
     	 	 }

     	 	 // 8. Nos aseguramos de que NUNCA intente "caminar"
     	 	 //anim.SetBool("isWalking", false);
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

    // --- FUNCIÓN JumpTowards CORREGIDA ---
   	 void JumpTowards(Vector3 targetPos)
   	 {
     	 	 if (!isGrounded) return;
     	 	 Vector2 dir = (targetPos - transform.position);
     	 	 float horizontal = Mathf.Clamp(dir.x, -1f, 1f);
     	 	 
     	 	 // 1. Calculamos la VELOCIDAD de salto deseada
     	 	 Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);

        // 2. Calculamos la FUERZA necesaria (Velocidad * Masa)
        Vector2 forceToApply = jumpVelocity * rb.mass;

        // 3. Limpiamos la velocidad anterior y aplicamos la fuerza como IMPULSO
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Limpia velocidad horizontal
     	 	 rb.AddForce(forceToApply, ForceMode2D.Impulse); // Usa Impulse
     	 	 
     	 	 anim.SetTrigger("isJumping");
     	 	 if (jumpSfx != null) AudioSource.PlayClipAtPoint(jumpSfx, transform.position);
   	 }

    // --- FUNCIÓN JumpAway CORREGIDA ---
   	 void JumpAway(Vector3 targetPos)
   	 {
     	 	 if (!isGrounded) return;
     	 	 float horizontal = Mathf.Sign(transform.position.x - targetPos.x);

     	 	 if (Mathf.Abs(transform.position.x - targetPos.x) < 0.1f)
     	 	 {
       	 	 	 horizontal = (Random.Range(0, 2) == 0) ? 1f : -1f;
     	 	 }

        // 1. Calculamos la VELOCIDAD de salto deseada
   	 	Vector2 jumpVelocity = new Vector2(horizontal * jumpHorizontalSpeed, jumpVerticalSpeed);

        // 2. Calculamos la FUERZA necesaria (Velocidad * Masa)
        Vector2 forceToApply = jumpVelocity * rb.mass;

        // 3. Limpiamos la velocidad anterior y aplicamos la fuerza como IMPULSO
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Limpia velocidad horizontal
      	 	 rb.AddForce(forceToApply, ForceMode2D.Impulse); // Usa Impulse

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
     	 	 SaludJugador saludDelJugador = collision.gameObject.GetComponent<SaludJugador>();
     	 	 Rigidbody2D rbDelJugador = collision.gameObject.GetComponent<Rigidbody2D>();

     	 	 // 1. APLICAR DAÑO
     	 	 if (saludDelJugador != null)
      	 {
       	 	 	 saludDelJugador.TakeDamage(damagePorContacto);
     	 	 }

     	 	 // 2. APLICAR KNOCKBACK (EMPUJÓN)
     	 	 if (rbDelJugador != null)
     	 	 {
       	 	 	 rbDelJugador.linearVelocity = Vector2.zero;
       	 	 	 float direccionHorizontal = Mathf.Sign(collision.transform.position.x - transform.position.x);
       	 	 	 Vector2 fuerzaDeEmpuje = new Vector2(direccionHorizontal * knockbackForceHorizontal, knockbackForceVertical);
      	 	 rbDelJugador.AddForce(fuerzaDeEmpuje, ForceMode2D.Impulse);
     	 	 }
   	 }
}