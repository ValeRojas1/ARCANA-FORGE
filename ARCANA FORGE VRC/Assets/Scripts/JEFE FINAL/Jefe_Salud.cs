using UnityEngine;
using System.Collections;

public class Jefe_Salud : MonoBehaviour
{
    [Header("Salud y Fases")]
    public int saludMaxima = 300;
    public int saludActual;
    private int faseActual = 1;
    public bool estaMuerto = false;

    [Header("UI de Salud (Auto-Instanciar)")]
    public GameObject barraDeSaludPrefab;
    private UI_BarraDeSalud barraDeSaludUI_Instancia;
    private Canvas canvasWorldSpace;

    [Header("Componentes")]
    private Animator anim;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;

    [Header("Diálogos (MP3)")]
    public AudioClip dialogoIntro;
    public AudioClip dialogoFase2;
    public AudioClip dialogoFase3;
    public AudioClip dialogoMuerte;
    public AudioClip[] dialogosCombateAleatorios;

    [Header("Efectos de Fase")]
    public Color colorFase2 = Color.red;
    public Color colorFase3 = new Color(0.8f, 0.4f, 1f);

    [Header("Recompensa")]
    public GameObject prefabDelOrbe;

    void Awake()
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        saludActual = saludMaxima;

        // Busca el Canvas en la escena
        GameObject canvasGO = GameObject.Find("Canvas_WorldSpace");
        if (canvasGO != null) { canvasWorldSpace = canvasGO.GetComponent<Canvas>(); }

        // Crea la barra de vida
        if (barraDeSaludPrefab != null && canvasWorldSpace != null)
        {
            GameObject barraInstanciada = Instantiate(barraDeSaludPrefab, canvasWorldSpace.transform);
            barraDeSaludUI_Instancia = barraInstanciada.GetComponent<UI_BarraDeSalud>();
            if (barraDeSaludUI_Instancia != null)
            {
                barraDeSaludUI_Instancia.targetToFollow = this.transform;
            }
        }

        // Diálogo de introducción
        StartCoroutine(ReproducirDialogoConPausa(dialogoIntro, 1.0f));
    }

    // *** FUNCIÓN PRINCIPAL DE DAÑO - CORREGIDA ***
    public void TakeDamage(int dano)
    {
        if (estaMuerto) return;

        saludActual -= dano;
        
        // *** CORREGIDO: Usa el trigger "Hurt" que existe en tu Animator ***
        anim.SetTrigger("Hurt"); // ✅ Activa la animación de golpe
        
        // Actualiza la barra de salud
        if (barraDeSaludUI_Instancia != null)
        {
            barraDeSaludUI_Instancia.ActualizarBarra(saludActual, saludMaxima);
        }

        // --- LÓGICA DE FASES ---

        // FASE 2: se activa al 70% de vida
        if (saludActual <= (saludMaxima * 0.70f) && faseActual == 1)
        {
            EmpezarFase2();
        }
        // FASE 3: se activa al 35% de vida
        else if (saludActual <= (saludMaxima * 0.35f) && faseActual == 2)
        {
            EmpezarFase3();
        }
        // MUERTE
        else if (saludActual <= 0)
        {
            Morir();
        }
    }

    // *** FASE 2 - CORREGIDA ***
    void EmpezarFase2()
    {
        faseActual = 2;
        
        // *** CORREGIDO: Usa el trigger "Transforrr" para la transformación ***
        anim.SetTrigger("Transform"); // ✅ Activa la animación JF HIT
        
        spriteRenderer.color = colorFase2; // Se tiñe de rojo
        
        // Reproduce el diálogo de fase 2
        StartCoroutine(ReproducirDialogoConPausa(dialogoFase2, 0.5f));
    }

    // *** FASE 3 - CORREGIDA ***
    void EmpezarFase3()
    {
        faseActual = 3;
        
        // *** CORREGIDO: Usa el trigger "Transforrr" para la transformación ***
        anim.SetTrigger("Transform"); // ✅ Activa la animación JF HIT
        
        spriteRenderer.color = colorFase3; // Se tiñe de morado
        
        // Reproduce el diálogo de fase 3
        StartCoroutine(ReproducirDialogoConPausa(dialogoFase3, 0.5f));
    }

    // *** MUERTE - CORREGIDA ***
    void Morir()
    {
        estaMuerto = true;

        // Activa la animación de muerte
        anim.SetTrigger("Death");

        // Desactiva los colliders para que el jugador no pueda seguir atacando
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        // *** REPRODUCE EL DIÁLOGO DE MUERTE ***
        if (dialogoMuerte != null)
        {
            audioSource.PlayOneShot(dialogoMuerte);

            // *** ESPERA A QUE TERMINE EL AUDIO ANTES DE DESTRUIR ***
            float duracionAudio = dialogoMuerte.length;
            StartCoroutine(EsperarYSpawnearRecompensa(duracionAudio));
        }
        else
        {
            // Si no hay audio, spawns inmediatamente después de la animación
            StartCoroutine(EsperarYSpawnearRecompensa(2f)); // 2 segundos de espera
        }
    }
    
    // *** NUEVA COROUTINE: Espera a que termine el audio ***
    IEnumerator EsperarYSpawnearRecompensa(float tiempoEspera)
    {
        // Espera el tiempo especificado (duración del audio)
        yield return new WaitForSeconds(tiempoEspera);
        
        // Ahora sí spawns la recompensa y destruye al jefe
        SpawmearRecompensa();
    }

    // --- FUNCIONES DE DIÁLOGOS ---

    public void ReproducirDialogoFase()
    {
        if (faseActual == 2)
        {
            PlayDialogue(dialogoFase2);
        }
        else if (faseActual == 3)
        {
            PlayDialogue(dialogoFase3);
        }
    }

    public void ReproducirDialogoMuerte()
    {
        PlayDialogue(dialogoMuerte);
    }

    void PlayDialogue(AudioClip clip)
    {
        if (clip != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ReproducirDialogoCombateAleatorio()
    {
        if (dialogosCombateAleatorios.Length == 0) return;
        
        int indiceAleatorio = Random.Range(0, dialogosCombateAleatorios.Length);
        PlayDialogue(dialogosCombateAleatorios[indiceAleatorio]);
    }

    IEnumerator ReproducirDialogoConPausa(AudioClip clip, float pausa)
    {
        yield return new WaitForSeconds(pausa);
        PlayDialogue(clip);
    }

    // *** REEMPLAZA ESTA FUNCIÓN EN Jefe_Salud.cs ***

    public void SpawmearRecompensa()
    {
        if(prefabDelOrbe != null)
        {
            // Crea la gema en la posición del jefe
            Instantiate(prefabDelOrbe, transform.position, Quaternion.identity);
            
            Debug.Log("¡Gema de victoria spawneada!");
        }

        // Destruye la barra de salud
        if (barraDeSaludUI_Instancia != null)
        {
            Destroy(barraDeSaludUI_Instancia.gameObject);
        }

        // Destruye al jefe
        Destroy(gameObject);
    }

}
