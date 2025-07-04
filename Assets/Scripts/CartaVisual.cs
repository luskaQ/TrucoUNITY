using UnityEngine;
public class CartaVisual : MonoBehaviour
{
    public Renderer frenteRenderer; // Referência ao renderer do Quad da frente
    public Renderer versoRenderer;  // Referência ao renderer do Quad do verso

    private Texture2D texturaAtual;
    private bool virada = false;


    public void AplicarTextura(Texture2D novaTextura)
    {
        texturaAtual = novaTextura;
        frenteRenderer.material.mainTexture = novaTextura;
        //Debug.Log("TEXTURA APLICADA BUS BUS BUS");
        MostrarFrente();
    }

    public void MostrarFrente()
    {
        //Debug.Log("TEXTURA VIRADA!");
        virada = true;
        AtualizarVisual();
    }

    public void MostrarVerso()
    {
        virada = true;
        AtualizarVisual();
    }

    private void AtualizarVisual()
    {
        frenteRenderer.enabled = virada;
        versoRenderer.enabled = virada;
        Material matFrente = frenteRenderer.material;

        matFrente.EnableKeyword("_ALPHATEST_ON");

        matFrente.SetFloat("_Cutoff", 0.5f);

    }
}