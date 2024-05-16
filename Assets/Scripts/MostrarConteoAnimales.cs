using UnityEngine;
using TMPro;

public class MostrarConteoAnimales : MonoBehaviour
{
    public TextMeshProUGUI textoConteo;
    public string[] etiquetasAnimales;

    void Update()
    {
        ActualizarConteo();
    }

    void ActualizarConteo()
    {
        string texto = "";
        foreach (string etiqueta in etiquetasAnimales)
        {
            int conteo = ContarAnimales(etiqueta);
            texto += etiqueta + ": " + conteo.ToString() + "\n";
        }
        textoConteo.text = texto;
    }

    int ContarAnimales(string etiqueta)
    {
        GameObject[] animales = GameObject.FindGameObjectsWithTag(etiqueta);
        return animales.Length;
    }
}
