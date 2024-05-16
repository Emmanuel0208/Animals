using System.Collections;
using UnityEngine;
public class MovimientoPez : MonoBehaviour
{
    [Header("Configuración de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string etiquetaObjetivo = "Shrimp";

    [Header("Valores Aleatorios")]
    [SerializeField] private float vidaMaxima;
    [SerializeField] private float tiempoDeVida;

    [Header("Información Adicional")]
    [SerializeField] private float vidaActual;
    [SerializeField] private float segundosRestantesDeVida;
    [SerializeField] private bool puedeReproducirse = false;
    [SerializeField] private GameObject nuevoHijoPrefab; // Variable para el prefab del nuevo hijo

    [SerializeField] private Vector3 limiteMinimo;
    [SerializeField] private Vector3 limiteMaximo;

    private float contadorCambioDireccion;
    private float tiempoDesdeUltimaReproduccion; // Variable para rastrear el tiempo desde la última reproducción
    private Rigidbody rb;
    private Vector3 objetivoPosicion; // Posición objetivo hacia la que se dirigirá el objeto

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        contadorCambioDireccion = 0f;
        tiempoDesdeUltimaReproduccion = 0f; // Inicializa el tiempo desde la última reproducción
        vidaMaxima = Random.Range(30, 51);
        vidaActual = vidaMaxima;
        tiempoDeVida = Random.Range(40f, 60f);
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
                if (tiempoDesdeUltimaReproduccion >= 4f) // Verifica si ha pasado al menos 10 segundos desde la última reproducción
                {
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f; // Reinicia el tiempo desde la última reproducción
                }
            }

            if (segundosRestantesDeVida <= 40 && segundosRestantesDeVida > 0)
            {
                // Cambia el objetivo a la posición de un objeto "Shrimp"
                CambiarObjetivoAShrimp();
            }
            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 40)
                {
                    Debug.Log("Comenzando a buscar shrimp...");
                    BuscarShrimp();
                }
                else
                {
                    MoverAleatoriamente();
                }
            }

            tiempoDesdeUltimaReproduccion += Time.deltaTime; // Incrementa el tiempo desde la última reproducción
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

        // Verifica y ajusta la posición dentro de los límites
        Vector3 nuevaPosicion = transform.position + (transform.forward * velocidadMaxima * Time.deltaTime);
        nuevaPosicion.x = Mathf.Clamp(nuevaPosicion.x, limiteMinimo.x, limiteMaximo.x);
        nuevaPosicion.z = Mathf.Clamp(nuevaPosicion.z, limiteMinimo.z, limiteMaximo.z);
        transform.position = nuevaPosicion;
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
            MovimientoPez shrimpScript = other.GetComponent<MovimientoPez>();
            vidaActual += Random.Range(5, 11);
            segundosRestantesDeVida += Random.Range(25f, 30f);
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

    void Reproducirse()
    {
        // Verificar si el objeto puede reproducirse
        if (!puedeReproducirse)
        {
            return; // No se puede reproducir si no puedeReproducirse es false
        }

        // Busca a otro objeto con la etiqueta "Fish" que también esté buscando reproducirse
        GameObject[] fishes = GameObject.FindGameObjectsWithTag("Fish");
        GameObject pareja = null;

        foreach (GameObject fish in fishes)
        {
            MovimientoPez movimientoFish = fish.GetComponent<MovimientoPez>();
            if (movimientoFish != null && movimientoFish.puedeReproducirse)
            {
                // Si encuentra una pareja potencial, sal de la búsqueda
                pareja = fish;
                break;
            }
        }

        // Si encontró una pareja, instancie al nuevo hijo
        if (pareja != null)
        {
            // Calcula la posición del nuevo hijo
            Vector3 posicionNueva = (transform.position + pareja.transform.position) / 2f;

            // Instancia al nuevo hijo
            GameObject nuevoHijo = Instantiate(nuevoHijoPrefab, posicionNueva, Quaternion.identity);

            // Obtén el componente MovimientoPez del nuevo hijo
            MovimientoPez movimientoNuevoHijo = nuevoHijo.GetComponent<MovimientoPez>();

            // Heredar características de los padres y aplicar mutación
            movimientoNuevoHijo.velocidadMaxima = Random.Range(Mathf.Min(velocidadMaxima, pareja.GetComponent<MovimientoPez>().velocidadMaxima),
                                                                Mathf.Max(velocidadMaxima, pareja.GetComponent<MovimientoPez>().velocidadMaxima));
            movimientoNuevoHijo.tiempoDeVida = Random.Range(Mathf.Min(tiempoDeVida, pareja.GetComponent<MovimientoPez>().tiempoDeVida),
                                                              Mathf.Max(tiempoDeVida, pareja.GetComponent<MovimientoPez>().tiempoDeVida));
            movimientoNuevoHijo.vidaMaxima = Random.Range(Mathf.Min(vidaMaxima, pareja.GetComponent<MovimientoPez>().vidaMaxima),
                                                           Mathf.Max(vidaMaxima, pareja.GetComponent<MovimientoPez>().vidaMaxima));

            // Aplicar mutación a una característica aleatoria
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

            // Desactiva la capacidad de reproducción de ambos padres para evitar más reproducciones
            puedeReproducirse = false;
            MovimientoPez movimientoPareja = pareja.GetComponent<MovimientoPez>();
            movimientoPareja.puedeReproducirse = false;

            // Mensaje de depuración
            Debug.Log("Reproducción exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con características heredadas y mutación.");
        }
    }
}
