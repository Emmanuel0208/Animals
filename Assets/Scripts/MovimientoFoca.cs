using System.Collections;
using UnityEngine;

public class MovimientoFoca : MonoBehaviour
{
    [Header("Configuraci�n de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string etiquetaObjetivo = "Fish";

    [Header("Valores Aleatorios")]
    [SerializeField] private float vidaMaxima;
    [SerializeField] private float tiempoDeVida;

    [Header("Informaci�n Adicional")]
    [SerializeField] private float vidaActual;
    [SerializeField] private float segundosRestantesDeVida;
    [SerializeField] private bool puedeReproducirse = false;
    [SerializeField] private GameObject nuevoHijoPrefab; // Variable para el prefab del nuevo hijo

    [SerializeField] private Vector3 limiteMinimo;
    [SerializeField] private Vector3 limiteMaximo;

    private float contadorCambioDireccion;
    private float tiempoDesdeUltimaReproduccion; // Variable para rastrear el tiempo desde la �ltima reproducci�n
    private Rigidbody rb;
    private Vector3 objetivoPosicion; // Posici�n objetivo hacia la que se dirigir� el objeto

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        contadorCambioDireccion = 0f;
        tiempoDesdeUltimaReproduccion = 0f; // Inicializa el tiempo desde la �ltima reproducci�n
        vidaMaxima = Random.Range(2, 51);
        vidaActual = vidaMaxima;
        tiempoDeVida = Random.Range(35f, 60f);
        velocidadMaxima = Random.Range(20f, 40f);
        segundosRestantesDeVida = tiempoDeVida;

        InvokeRepeating("CambiarDireccion", 0f, cambioDeDireccionIntervalo);
        StartCoroutine(ContadorTiempoVida());
    }

    IEnumerator ContadorTiempoVida()
    {
        while (segundosRestantesDeVida > 0)
        {
            segundosRestantesDeVida -= Time.deltaTime;

            if (segundosRestantesDeVida > 45)
            {
                if (tiempoDesdeUltimaReproduccion >= 5f) // Verifica si ha pasado al menos 10 segundos desde la �ltima reproducci�n
                {
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f; // Reinicia el tiempo desde la �ltima reproducci�n
                }
            }

            if (segundosRestantesDeVida <= 25 && segundosRestantesDeVida > 0)
            {
                // Cambia el objetivo a la posici�n de un objeto "Fish"
                CambiarObjetivoAFish();
            }
            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 25)
                {
                    Debug.Log("Comenzando a buscar fish...");
                    BuscarFish();
                }
                else
                {
                    MoverAleatoriamente();
                }
            }

            tiempoDesdeUltimaReproduccion += Time.deltaTime; // Incrementa el tiempo desde la �ltima reproducci�n
            yield return null;
        }

        DestruirObjeto();
    }

    void MoverAleatoriamente()
    {
        // Realiza el movimiento aleatorio cambiando la direcci�n del objeto
        contadorCambioDireccion += Time.deltaTime;
        if (contadorCambioDireccion >= cambioDeDireccionIntervalo)
        {
            CambiarDireccion();
            contadorCambioDireccion = 0f;
        }

        // Verifica y ajusta la posici�n dentro de los l�mites
        Vector3 nuevaPosicion = transform.position + (transform.forward * velocidadMaxima * Time.deltaTime);
        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, limiteMinimo.x, limiteMaximo.x);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, limiteMinimo.z, limiteMaximo.z);
        transform.position = nuevaPosicion;
    }

    void FixedUpdate()
    {
        // Si hay un objetivo, mueve gradualmente hacia �l
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
            MovimientoFoca fishScript = other.GetComponent<MovimientoFoca>();
            vidaActual += Random.Range(5, 11);
            segundosRestantesDeVida += Random.Range(9f, 11f);
            Destroy(other.gameObject);
        }
    }

    void BuscarFish()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (fishes.Length == 0)
        {
            Debug.Log("No se encontraron fish.");
            // Si no se encontraron objetos "Fish", reanuda el movimiento aleatorio
            objetivoPosicion = Vector3.zero;
            MoverAleatoriamente();
            return;
        }

        GameObject fishMasCercano = null;
        float distanciaMasCorta = Mathf.Infinity;
        foreach (GameObject fish in fishes)
        {
            float distancia = Vector3.Distance(transform.position, fish.transform.position);
            if (distancia < distanciaMasCorta)
            {
                distanciaMasCorta = distancia;
                fishMasCercano = fish;
            }
        }

        if (fishMasCercano != null)
        {
            Debug.Log("Fish encontrado.");
            objetivoPosicion = fishMasCercano.transform.position;
        }
        else
        {
            Debug.Log("No se encontr� ning�n Fish.");
        }
    }

    void CambiarObjetivoAFish()
    {
        GameObject[] fishes = GameObject.FindGameObjectsWithTag(etiquetaObjetivo);

        if (fishes.Length == 0)
        {
            Debug.Log("No se encontraron fish.");
            return;
        }

        GameObject fishAleatorio = fishes[Random.Range(0, fishes.Length)];
        objetivoPosicion = fishAleatorio.transform.position;
    }

    void MoverHaciaObjetivo()
    {
        // Calcula la direcci�n hacia el objetivo
        Vector3 direccion = (objetivoPosicion - transform.position).normalized;
        // Mueve el objeto hacia el objetivo con velocidad gradual
        rb.velocity = direccion * Mathf.Min(velocidadMaxima, Vector3.Distance(transform.position, objetivoPosicion));
    }

    void CambiarDireccion()
    {
        Quaternion nuevaRotacion = Quaternion.Euler(0f, Random.Range(-90f, 90f), 0f);
        transform.rotation *= nuevaRotacion;
    }

    void Reproducirse()
    {
        // Verificar si el objeto puede reproducirse
        if (!puedeReproducirse)
        {
            return; // No se puede reproducir si no puedeReproducirse es false
        }

        // Busca a otro objeto con la etiqueta "Seal" que tambi�n est� buscando reproducirse
        GameObject[] seals = GameObject.FindGameObjectsWithTag("Seal");
        GameObject pareja = null;

        foreach (GameObject seal in seals)
        {
            MovimientoFoca movimientoSeal = seal.GetComponent<MovimientoFoca>();
            if (movimientoSeal != null && movimientoSeal.puedeReproducirse)
            {
                // Si encuentra una pareja potencial, sal de la b�squeda
                pareja = seal;
                break;
            }
        }

        // Si encontr� una pareja, instancie al nuevo hijo
        if (pareja != null)
        {
            // Calcula la posici�n del nuevo hijo
            Vector3 posicionNueva = (transform.position + pareja.transform.position) / 2f;

            // Instancia al nuevo hijo
            GameObject nuevoHijo = Instantiate(nuevoHijoPrefab, posicionNueva, Quaternion.identity);

            // Obt�n el componente MovimientoFoca del nuevo hijo
            MovimientoFoca movimientoNuevoHijo = nuevoHijo.GetComponent<MovimientoFoca>();

            // Heredar caracter�sticas de los padres y aplicar mutaci�n
            movimientoNuevoHijo.velocidadMaxima = Random.Range(Mathf.Min(velocidadMaxima, pareja.GetComponent<MovimientoFoca>().velocidadMaxima),
                                                                Mathf.Max(velocidadMaxima, pareja.GetComponent<MovimientoFoca>().velocidadMaxima));
            movimientoNuevoHijo.tiempoDeVida = Random.Range(Mathf.Min(tiempoDeVida, pareja.GetComponent<MovimientoFoca>().tiempoDeVida),
                                                              Mathf.Max(tiempoDeVida, pareja.GetComponent<MovimientoFoca>().tiempoDeVida));
            movimientoNuevoHijo.vidaMaxima = Random.Range(Mathf.Min(vidaMaxima, pareja.GetComponent<MovimientoFoca>().vidaMaxima),
                                                           Mathf.Max(vidaMaxima, pareja.GetComponent<MovimientoFoca>().vidaMaxima));

            // Aplicar mutaci�n a una caracter�stica aleatoria
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

            // Desactiva la capacidad de reproducci�n de ambos padres para evitar m�s reproducciones
            puedeReproducirse = false;
            MovimientoFoca movimientoPareja = pareja.GetComponent<MovimientoFoca>();
            movimientoPareja.puedeReproducirse = false;

            // Mensaje de depuraci�n
            Debug.Log("Reproducci�n exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con caracter�sticas heredadas y mutaci�n.");
        }
    }
}
