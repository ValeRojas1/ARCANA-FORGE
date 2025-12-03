using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_PowerupTimer : MonoBehaviour
{
    public static UI_PowerupTimer Instance;
    
    [Header("UI Elements")]
    public GameObject panelPowerup; // Panel contenedor
    public Image barraFill; // La barra que se va vaciando
    public TMP_Text textoTiempo; // Texto "5s", "4s", etc. (opcional)
    
    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Ocultar al inicio
        if (panelPowerup != null)
        {
            panelPowerup.SetActive(false);
        }
    }
    
    public void MostrarPowerup()
    {
        if (panelPowerup != null)
        {
            panelPowerup.SetActive(true);
        }
    }
    
    public void OcultarPowerup()
    {
        if (panelPowerup != null)
        {
            panelPowerup.SetActive(false);
        }
    }
    
    public void ActualizarTimer(float tiempoRestante, float tiempoTotal)
    {
        // Actualizar barra
        if (barraFill != null)
        {
            barraFill.fillAmount = tiempoRestante / tiempoTotal;
        }
        
        // Actualizar texto (opcional)
        if (textoTiempo != null)
        {
            textoTiempo.text = Mathf.CeilToInt(tiempoRestante) + "s";
        }
    }
}
