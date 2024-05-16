using UnityEngine;
using System.Collections;

public class MovimientoAleatorio : MonoBehaviour
{
    [Header("Configuración de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string etiquetaObjetivo = "Shrimp";

    [Header("Valores Aleatorios")]
    [SerializeField] private int vidaMaxima;
    [SerializeField] private float tiempoDeVida;

    [Header("Información Adicional")]
    [SerializeField] private int vidaActual;
    [SerializeField] private float segundosRestantesDeVida;

    private float contadorCambioDireccion;
    private Rigidbody rb;
    private Vector3 objetivoPosicion; // Posición objetivo hacia la que se dirigirá el objeto

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        contadorCambioDireccion = 0f;
        vidaMaxima = Random.Range(2, 51);
        vidaActual = vidaMaxima;
        tiempoDeVida = Random.Range(2f, 51f);
        velocidadMaxima = Random.Range(2f, 51f);
        segundosRestantesDeVida = tiempoDeVida;

        InvokeRepeating("CambiarDireccion", 0f, cambioDeDireccionIntervalo);
        StartCoroutine(ContadorTiempoVida());
    }

    IEnumerator ContadorTiempoVida()
    {
        while (segundosRestantesDeVida > 0)
        {
            segundosRestantesDeVida -= Time.deltaTime;

            if (segundosRestantesDeVida <= 15 && segundosRestantesDeVida > 0)
            {
                // Cambia el objetivo a la posición de un objeto "Shrimp"
                CambiarObjetivoAShrimp();
            }
            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 30)
                {
                    Debug.Log("Comenzando a buscar shrimp...");
                    BuscarShrimp();
                }
                else
                {
                    MoverAleatoriamente();
                }
            }

            yield return null;
        }

        DestruirObjeto();
    }

    void MoverAleatoriamente()
    {
        // Realiza el movimiento aleatorio cambiando la dirección del objeto
        contadorCambioDireccion += Time.deltaTime;
        if (contadorCambioDireccion >= cambioDeDireccionIntervalo)
        {
            CambiarDireccion();
            contadorCambioDireccion = 0f;
        }
    }

    void FixedUpdate()
    {
        // Si hay un objetivo, mueve gradualmente hacia él
        if (objetivoPosicion != Vector3.zero)
        {
            MoverHaciaObjetivo();
        }
        else
        {
            // Si no hay un objetivo, mueve aleatoriamente
            rb.velocity = transform.forward * velocidadMaxima;
        }
    }

    void DestruirObjeto()
    {
        Destroy(gameObject);
    }

    public void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(etiquetaObjetivo))
        {
            MovimientoAleatorio shrimpScript = other.GetComponent<MovimientoAleatorio>();
            vidaActual += Random.Range(5, 11);
            segundosRestantesDeVida += Random.Range(5f, 11f);
            Destroy(other.gameObject);
        }
    }

    void BuscarShrimp()
    {
        GameObject[] shrimps = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (shrimps.Length == 0)
        {
            Debug.Log("No se encontraron shrimps.");
            // Si no se encontraron objetos "Shrimp", reanuda el movimiento aleatorio
            objetivoPosicion = Vector3.zero;
            MoverAleatoriamente();
            return;
        }

        GameObject shrimpMasCercano = null;
        float distanciaMasCorta = Mathf.Infinity;
        foreach (GameObject shrimp in shrimps)
        {
            float distancia = Vector3.Distance(transform.position, shrimp.transform.position);
            if (distancia < distanciaMasCorta)
            {
                distanciaMasCorta = distancia;
                shrimpMasCercano = shrimp;
            }
        }

        if (shrimpMasCercano != null)
        {
            Debug.Log("Shrimp encontrado.");
            objetivoPosicion = shrimpMasCercano.transform.position;
        }
        else
        {
            Debug.Log("No se encontró ningún Shrimp.");
        }
    }


    void CambiarObjetivoAShrimp()
    {
        GameObject[] shrimps = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (shrimps.Length == 0)
        {
            Debug.Log("No se encontraron shrimps.");
            return;
        }

        GameObject shrimpAleatorio = shrimps[Random.Range(0, shrimps.Length)];
        objetivoPosicion = shrimpAleatorio.transform.position;
    }

    void MoverHaciaObjetivo()
    {
        // Calcula la dirección hacia el objetivo
        Vector3 direccion = (objetivoPosicion - transform.position).normalized;
        // Mueve el objeto hacia el objetivo con velocidad gradual
        rb.velocity = direccion * Mathf.Min(velocidadMaxima, Vector3.Distance(transform.position, objetivoPosicion));
    }

    void CambiarDireccion()
    {
        Quaternion nuevaRotacion = Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f);
        transform.rotation *= nuevaRotacion;
    }
}
