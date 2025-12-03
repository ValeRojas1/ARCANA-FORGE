using UnityEngine;
using TMPro;

/// <summary>
/// Script simple que conecta el texto UI de monedas con el GameManager
/// Debe estar adjunto al GameObject que tiene el TextMeshPro de las monedas
/// </summary>
public class CoinUIConnector : MonoBehaviour
{
    void Start()
    {
        // Obtener el componente TextMeshPro de este mismo GameObject
        TMP_Text coinText = GetComponent<TMP_Text>();

        if (coinText == null)
        {
            Debug.LogError("CoinUIConnector: No se encontró TMP_Text en este GameObject!");
            return;
        }

        // Conectar al GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCoinTextUI(coinText);
            Debug.Log("✓ Texto de monedas conectado correctamente al GameManager");
        }
        else
        {
            Debug.LogError("CoinUIConnector: GameManager no existe en la escena!");
        }
    }
}
