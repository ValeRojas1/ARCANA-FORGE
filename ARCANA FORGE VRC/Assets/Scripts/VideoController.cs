using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la escena a cargar cuando termine el video")]
    public string siguienteEscena = "MainMenu";
    
    [Header("Referencias")]
    public VideoPlayer videoPlayer;
    
    private bool videoPausado = false;
    private bool puedeAvanzar = true;
    
    void Start()
    {
        // Verificar que el VideoPlayer esté asignado
        if (videoPlayer == null)
        {
            videoPlayer = FindFirstObjectByType<VideoPlayer>();
        }
        
        if (videoPlayer == null)
        {
            Debug.LogError("❌ No se encontró VideoPlayer en la escena");
            return;
        }
        
        // Suscribirse al evento de fin de video
        videoPlayer.loopPointReached += OnVideoTerminado;
        
        // Iniciar el video
        videoPlayer.Play();
        Debug.Log("▶ Video iniciado");
    }
    
    void Update()
    {
        // Detectar input para saltar (Enter, Espacio o Click)
        if (Input.GetKeyDown(KeyCode.Return) || 
            Input.GetKeyDown(KeyCode.Space) || 
            Input.GetMouseButtonDown(0))
        {
            SaltarVideo();
        }
    }
    
    void OnVideoTerminado(VideoPlayer vp)
    {
        Debug.Log("✓ Video terminado - Cargando siguiente escena");
        CargarSiguienteEscena();
    }
    
    void SaltarVideo()
    {
        if (puedeAvanzar)
        {
            Debug.Log("⏩ Video saltado por el jugador");
            CargarSiguienteEscena();
        }
    }
    
    void CargarSiguienteEscena()
    {
        puedeAvanzar = false;
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        
        Debug.Log($"Cargando escena: {siguienteEscena}");
        SceneManager.LoadScene(siguienteEscena);
    }
    
    void OnDestroy()
    {
        // Limpiar el evento cuando se destruya el objeto
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoTerminado;
        }
    }
}
