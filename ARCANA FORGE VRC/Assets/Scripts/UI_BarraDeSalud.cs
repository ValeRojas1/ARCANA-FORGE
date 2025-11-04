// UI_BarraDeSalud.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UI_BarraDeSalud : MonoBehaviour
{
    // --- NUEVAS LÍNEAS ---
    [Header("Configuración de Seguimiento")]
    public Transform targetToFollow; // El personaje al que debe seguir
    public Vector3 offset = new Vector3(0, 0.8f, 0); // Distancia sobre la cabeza
    // --- FIN DE NUEVAS LÍNEAS ---

    [Header("Configuración de UI")]
    public Slider slider;
    public float tiempoVisible = 3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Camera mainCamera; // Para optimizar

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }
        mainCamera = Camera.main; // Guardamos la cámara
        canvasGroup.alpha = 0; 
    }

    // --- NUEVA FUNCIÓN ---
    // LateUpdate se ejecuta después de todo el movimiento (Update)
    void LateUpdate()
    {
        if (targetToFollow != null)
        {
            // Actualiza la posición de la barra para que siga al objetivo + offset
            // Nos aseguramos de que la barra siempre mire a la cámara
            transform.position = targetToFollow.position + offset;
            transform.rotation = mainCamera.transform.rotation;
        }
        else
        {
            // Si el objetivo (enemigo) muere y es destruido, nos escondemos
            canvasGroup.alpha = 0;
        }
    }
    // --- FIN DE NUEVA FUNCIÓN ---

    public void ActualizarBarra(float valorActual, float valorMaximo)
    {
        slider.value = valorActual / valorMaximo;
        MostrarBarra();
    }

    private void MostrarBarra()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        canvasGroup.alpha = 1;
        fadeCoroutine = StartCoroutine(EsconderDespuesDe(tiempoVisible));
    }

    private IEnumerator EsconderDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);

        float duracionFade = 0.5f;
        float tiempo = 0;
        while (tiempo < duracionFade)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, tiempo / duracionFade);
            tiempo += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}