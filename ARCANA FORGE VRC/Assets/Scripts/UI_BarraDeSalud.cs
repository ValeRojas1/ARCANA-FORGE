// UI_BarraDeSalud.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UI_BarraDeSalud : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    public Transform targetToFollow; // El personaje al que debe seguir
    public Vector3 offset = new Vector3(0, 0.8f, 0); // Distancia sobre la cabeza

    [Header("Configuración de UI")]
    public Slider slider;
    public float tiempoVisible = 3f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Camera mainCamera;
    private bool esLaPrimeraActualizacion = true;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (slider == null)
        {
            slider = GetComponentInChildren<Slider>();
        }
        mainCamera = Camera.main;
        
        // FORZAR configuración inicial del slider
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1; // Empezar al 100%
            
            Debug.Log($"[{gameObject.name}] Slider inicializado: Value={slider.value}, Max={slider.maxValue}");
        }
        
        // ✅ CAMBIO: La barra empieza OCULTA (solo se muestra cuando recibe daño)
        canvasGroup.alpha = 0; // Cambiado de 1 a 0
    }


    void LateUpdate()
    {
        if (targetToFollow != null)
        {
            // Actualiza la posición de la barra para que siga al objetivo + offset
            transform.position = targetToFollow.position + offset;
            if (mainCamera != null)
            {
                transform.rotation = mainCamera.transform.rotation;
            }
        }
        else
        {
            // Si el objetivo muere, esconderse (para enemigos)
            if (canvasGroup.alpha > 0.01f)
            {
                canvasGroup.alpha = 0;
            }
        }
    }

    public void ActualizarBarra(int valorActual, int valorMaximo)
    {
        if (valorMaximo <= 0)
        {
            Debug.LogError($"[{gameObject.name}] valorMaximo debe ser mayor que 0");
            return;
        }

        // CALCULAR EL PORCENTAJE (normalizado entre 0 y 1)
        float porcentaje = (float)valorActual / (float)valorMaximo;
        
        // CONFIGURAR el slider con valores normalizados
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = porcentaje;
        
        if (esLaPrimeraActualizacion)
        {
            // Primera vez: actualizar pero NO mostrar si está al 100%
            esLaPrimeraActualizacion = false;
            
            if (valorActual < valorMaximo)
            {
                // Si empieza con daño (ej: después de morir y reiniciar), mostrar
                MostrarBarra();
                Debug.Log($"[{gameObject.name}] ⚠ Barra inicializada con daño: {valorActual}/{valorMaximo} = {porcentaje:F2} ({porcentaje*100:F0}%)");
            }
            else
            {
                // Si empieza a salud completa, mantener oculta
                canvasGroup.alpha = 0;
                Debug.Log($"[{gameObject.name}] ✓ Barra inicializada (oculta): {valorActual}/{valorMaximo} = {porcentaje:F2} (100%)");
            }
        }
        else
        {
            // Actualizaciones posteriores: siempre mostrar la barra
            MostrarBarra();
            Debug.Log($"[{gameObject.name}] ⚠ Salud actualizada: {valorActual}/{valorMaximo} = {porcentaje:F2} ({porcentaje*100:F0}%)");
        }
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
