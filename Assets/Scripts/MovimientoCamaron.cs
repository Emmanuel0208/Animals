using System.Collections;
using UnityEngine;

public class MovimientoCamaron : MonoBehaviour
{
    [Header("Configuración de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string etiquetaObjetivo = "Algae"; // Cambiado a "Algae"

    [Header("Valores Aleatorios")]
    [SerializeField] private float vidaMaxima;
    [SerializeField] private float tiempoDeVida;

    [Header("Información Adicional")]
    [SerializeField] private float vidaActual;
    [SerializeField] private float segundosRestantesDeVida;
    [SerializeField] private bool puedeReproducirse = false;
    [SerializeField] private GameObject nuevoHijoPrefab;

    [SerializeField] private Vector3 limiteMinimo;
    [SerializeField] private Vector3 limiteMaximo;

    private float contadorCambioDireccion;
    private float tiempoDesdeUltimaReproduccion;
    private Rigidbody rb;
    private Vector3 objetivoPosicion;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        contadorCambioDireccion = 0f;
        tiempoDesdeUltimaReproduccion = 0f;
        vidaMaxima = Random.Range(2, 51);
        vidaActual = vidaMaxima;
        tiempoDeVida = Random.Range(30f, 50f);
        velocidadMaxima = Random.Range(2f, 20f);
        segundosRestantesDeVida = tiempoDeVida;

        InvokeRepeating("CambiarDireccion", 0f, cambioDeDireccionIntervalo);
        StartCoroutine(ContadorTiempoVida());
    }

    IEnumerator ContadorTiempoVida()
    {
        while (segundosRestantesDeVida > 0)
        {
            segundosRestantesDeVida -= Time.deltaTime;

            if (segundosRestantesDeVida > 35)
            {
                if (tiempoDesdeUltimaReproduccion >= 2f)
                {
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f;
                }
            }

            if (segundosRestantesDeVida <= 30 && segundosRestantesDeVida > 0)
            {
                // Cambia el objetivo a la posición de un objeto "Algae"
                CambiarObjetivoAAlgae();
            }
            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 30)
                {
                    Debug.Log("Comenzando a buscar Algae...");
                    BuscarAlgae();
                }
                else
                {
                    MoverAleatoriamente();
                }
            }

            tiempoDesdeUltimaReproduccion += Time.deltaTime;
            yield return null;
        }

        DestruirObjeto();
    }

    void MoverAleatoriamente()
    {
        contadorCambioDireccion += Time.deltaTime;
        if (contadorCambioDireccion >= cambioDeDireccionIntervalo)
        {
            CambiarDireccion();
            contadorCambioDireccion = 0f;
        }

        Vector3 nuevaPosicion = transform.position + (transform.forward * velocidadMaxima * Time.deltaTime);
        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, limiteMinimo.x, limiteMaximo.x);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, limiteMinimo.z, limiteMaximo.z);
        transform.position = nuevaPosicion;
    }

    void FixedUpdate()
    {
        if (objetivoPosicion != Vector3.zero)
        {
            MoverHaciaObjetivo();
        }
        else
        {
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
            MovimientoCamaron algaeScript = other.GetComponent<MovimientoCamaron>(); // Cambiado a "MovimientoCamaron"
            vidaActual += Random.Range(5, 11);
            segundosRestantesDeVida += Random.Range(30f, 35f);
            Destroy(other.gameObject);
        }
    }

    void BuscarAlgae() // Cambiado a "BuscarAlgae"
    {
        GameObject[] algae = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (algae.Length == 0)
        {
            Debug.Log("No se encontraron algas.");
            objetivoPosicion = Vector3.zero;
            MoverAleatoriamente();
            return;
        }

        GameObject algaeMasCercano = null;
        float distanciaMasCorta = Mathf.Infinity;
        foreach (GameObject algaeObj in algae)
        {
            float distancia = Vector3.Distance(transform.position, algaeObj.transform.position);
            if (distancia < distanciaMasCorta)
            {
                distanciaMasCorta = distancia;
                algaeMasCercano = algaeObj;
            }
        }

        if (algaeMasCercano != null)
        {
            Debug.Log("Alga encontrado.");
            objetivoPosicion = algaeMasCercano.transform.position;
        }
        else
        {
            Debug.Log("No se encontró ninguna Alga.");
        }
    }

    void CambiarObjetivoAAlgae() // Cambiado a "CambiarObjetivoAAlgae"
    {
        GameObject[] algae = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (algae.Length == 0)
        {
            Debug.Log("No se encontraron algas.");
            return;
        }

        GameObject algaeAleatorio = algae[Random.Range(0, algae.Length)];
        objetivoPosicion = algaeAleatorio.transform.position;
    }

    void MoverHaciaObjetivo()
    {
        Vector3 direccion = (objetivoPosicion - transform.position).normalized;
        rb.velocity = direccion * Mathf.Min(velocidadMaxima, Vector3.Distance(transform.position, objetivoPosicion));
    }

    void CambiarDireccion()
    {
        Quaternion nuevaRotacion = Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f);
        transform.rotation *= nuevaRotacion;
    }

    void Reproducirse()
    {
        if (!puedeReproducirse)
        {
            return;
        }

        GameObject[] shrimps = GameObject.FindGameObjectsWithTag("Shrimp"); // Cambiado a "Shrimp"
        GameObject pareja = null;

        foreach (GameObject shrimp in shrimps)
        {
            MovimientoCamaron movimientoShrimp = shrimp.GetComponent<MovimientoCamaron>(); // Cambiado a "MovimientoCamaron"
            if (movimientoShrimp != null && movimientoShrimp.puedeReproducirse)
            {
                pareja = shrimp;
                break;
            }
        }

        if (pareja != null)
        {
            Vector3 posicionNueva = (transform.position + pareja.transform.position) / 2f;

            GameObject nuevoHijo = Instantiate(nuevoHijoPrefab, posicionNueva, Quaternion.identity);
            MovimientoCamaron movimientoNuevoHijo = nuevoHijo.GetComponent<MovimientoCamaron>(); // Cambiado a "MovimientoCamaron"

            movimientoNuevoHijo.velocidadMaxima = Random.Range(Mathf.Min(velocidadMaxima, pareja.GetComponent<MovimientoCamaron>().velocidadMaxima), // Cambiado a "MovimientoCamaron"
                                                                Mathf.Max(velocidadMaxima, pareja.GetComponent<MovimientoCamaron>().velocidadMaxima));
            movimientoNuevoHijo.tiempoDeVida = Random.Range(Mathf.Min(tiempoDeVida, pareja.GetComponent<MovimientoCamaron>().tiempoDeVida), // Cambiado a "MovimientoCamaron"
                                                              Mathf.Max(tiempoDeVida, pareja.GetComponent<MovimientoCamaron>().tiempoDeVida));
            movimientoNuevoHijo.vidaMaxima = Random.Range(Mathf.Min(vidaMaxima, pareja.GetComponent<MovimientoCamaron>().vidaMaxima), // Cambiado a "MovimientoCamaron"
                                                           Mathf.Max(vidaMaxima, pareja.GetComponent<MovimientoCamaron>().vidaMaxima));

            switch (Random.Range(0, 3))
            {
                case 0:
                    movimientoNuevoHijo.velocidadMaxima *= Random.Range(0.8f, 1.2f);
                    break;
                case 1:
                    movimientoNuevoHijo.tiempoDeVida *= Random.Range(0.8f, 1.2f);
                    break;
                case 2:
                    movimientoNuevoHijo.vidaMaxima *= Random.Range(0.8f, 1.2f);
                    break;
            }

            puedeReproducirse = false;
            MovimientoCamaron movimientoPareja = pareja.GetComponent<MovimientoCamaron>(); // Cambiado a "MovimientoCamaron"
            movimientoPareja.puedeReproducirse = false;

            Debug.Log("Reproducción exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con características heredadas y mutación.");
        }
    }
}

