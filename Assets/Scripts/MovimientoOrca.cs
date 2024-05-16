using System.Collections;
using UnityEngine;

public class MovimientoOrca : MonoBehaviour
{
    [Header("Configuraci�n de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string[] etiquetasObjetivo; // Array para las etiquetas de los objetivos

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
        tiempoDeVida = Random.Range(100f, 150f);
        velocidadMaxima = Random.Range(20f, 35f);
        segundosRestantesDeVida = tiempoDeVida;

        InvokeRepeating("CambiarDireccion", 0f, cambioDeDireccionIntervalo);
        StartCoroutine(ContadorTiempoVida());
    }

    IEnumerator ContadorTiempoVida()
    {
        while (segundosRestantesDeVida > 0)
        {
            segundosRestantesDeVida -= Time.deltaTime;

            if (segundosRestantesDeVida > 100)
            {
                if (tiempoDesdeUltimaReproduccion >= 10f) // Verifica si ha pasado al menos 10 segundos desde la �ltima reproducci�n
                {
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f; // Reinicia el tiempo desde la �ltima reproducci�n
                }
            }

            // Comprueba si hay segundos restantes de vida y si es as�, act�a en consecuencia
            if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida <= 100)
                {
                    // Cambia el objetivo a la posici�n de uno de los objetos objetivo
                    CambiarObjetivoAleatorio();
                }

                // Realiza el movimiento aleatorio o hacia el objetivo
                Mover();
            }

            tiempoDesdeUltimaReproduccion += Time.deltaTime; // Incrementa el tiempo desde la �ltima reproducci�n
            yield return null;
        }

        DestruirObjeto();
    }

    void Mover()
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
        foreach (string etiquetaObjetivo in etiquetasObjetivo)
        {
            if (other.CompareTag(etiquetaObjetivo))
            {
                MovimientoOrca orcaScript = other.GetComponent<MovimientoOrca>();
                vidaActual += Random.Range(5, 11);
                segundosRestantesDeVida += Random.Range(5f, 11f);
                Destroy(other.gameObject);
                break;
            }
        }
    }

    void CambiarObjetivoAleatorio()
    {
        GameObject[] objetivos = GameObject.FindGameObjectsWithTag(etiquetasObjetivo[Random.Range(0, etiquetasObjetivo.Length)]);

        if (objetivos.Length == 0)
        {
            Debug.Log("No se encontraron objetivos.");
            return;
        }

        GameObject objetivoAleatorio = objetivos[Random.Range(0, objetivos.Length)];
        objetivoPosicion = objetivoAleatorio.transform.position;
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
            return; // No se puede reproducir si puedeReproducirse es false
        }

        // Busca a otro objeto con la etiqueta "Orca" que tambi�n est� buscando reproducirse
        GameObject[] orcas = GameObject.FindGameObjectsWithTag("Orca");
        GameObject pareja = null;

        foreach (GameObject orca in orcas)
        {
            MovimientoOrca movimientoOrca = orca.GetComponent<MovimientoOrca>();
            if (movimientoOrca != null && movimientoOrca.puedeReproducirse)
            {
                // Si encuentra una pareja potencial, sal de la b�squeda
                pareja = orca;
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

            // Obt�n el componente MovimientoOrca del nuevo hijo
            MovimientoOrca movimientoNuevoHijo = nuevoHijo.GetComponent<MovimientoOrca>();

            // Heredar caracter�sticas de los padres y aplicar mutaci�n
            movimientoNuevoHijo.velocidadMaxima = Random.Range(Mathf.Min(velocidadMaxima, pareja.GetComponent<MovimientoOrca>().velocidadMaxima),
                                                                Mathf.Max(velocidadMaxima, pareja.GetComponent<MovimientoOrca>().velocidadMaxima));
            movimientoNuevoHijo.tiempoDeVida = Random.Range(Mathf.Min(tiempoDeVida, pareja.GetComponent<MovimientoOrca>().tiempoDeVida),
                                                              Mathf.Max(tiempoDeVida, pareja.GetComponent<MovimientoOrca>().tiempoDeVida));
            movimientoNuevoHijo.vidaMaxima = Random.Range(Mathf.Min(vidaMaxima, pareja.GetComponent<MovimientoOrca>().vidaMaxima),
                                                           Mathf.Max(vidaMaxima, pareja.GetComponent<MovimientoOrca>().vidaMaxima));

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
            MovimientoOrca movimientoPareja = pareja.GetComponent<MovimientoOrca>();
            movimientoPareja.puedeReproducirse = false;

            // Mensaje de depuraci�n
            Debug.Log("Reproducci�n exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con caracter�sticas heredadas y mutaci�n.");
        }
    }
}
