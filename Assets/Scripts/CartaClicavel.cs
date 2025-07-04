using UnityEngine;
using UnityEngine.EventSystems;
public class CartaClicavel : MonoBehaviour, IPointerDownHandler
{

    public Carta cartaData;
    private Baralho baralhoManager;
    public bool isClicavel = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        baralhoManager = FindObjectOfType<Baralho>();
        if (baralhoManager == null)
        {
            Debug.LogError("Nenhum objeto com o script baralho encontrado!");
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Carta clicada");
        if (isClicavel)
        {
            if (baralhoManager != null)
            {
                Debug.Log("Carta clicada: " + cartaData.ToString());
                baralhoManager.jogarCartaClicadaPlayer(this);
            }
        }
    }
}
