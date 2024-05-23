using System.Collections;
using UnityEngine;

public class MovimientoAleatorio : MonoBehaviour
{
    [Header("Configuraci�n de Variables")]
    [SerializeField] private float velocidadMaxima;
    [SerializeField] private float cambioDeDireccionIntervalo = 2f;
    [SerializeField] private float distanciaDeColision = 2f;
    [SerializeField] private string etiquetaObjetivo = "Shrimp";

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
        tiempoDeVida = Random.Range(60f, 80f);
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

            if (segundosRestantesDeVida > 65)
            {
                if (tiempoDesdeUltimaReproduccion >= 5f) // Verifica si ha pasado al menos 10 segundos desde la �ltima reproducci�n
                {
                    puedeReproducirse = true;
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f; // Reinicia el tiempo desde la �ltima reproducci�n
                }
            }

            
            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 60)
                {
                    Debug.Log("Comenzando a buscar shrimp...");
                    BuscarShrimp();
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
            
            vidaActual += Random.Range(5, 11);
            segundosRestantesDeVida += Random.Range(15f, 20f);
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
            Debug.Log("No se encontr� ning�n Shrimp.");
        }
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

        // Busca a otro objeto con la etiqueta "Whale" que tambi�n est� buscando reproducirse
        GameObject[] whales = GameObject.FindGameObjectsWithTag("Whale");
        GameObject pareja = null;

        foreach (GameObject whale in whales)
        {
            MovimientoAleatorio movimientoWhale = whale.GetComponent<MovimientoAleatorio>();
            if (movimientoWhale != null && movimientoWhale.puedeReproducirse)
            {
                // Si encuentra una pareja potencial, sal de la b�squeda
                pareja = whale;
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

            // Obt�n el componente MovimientoAleatorio del nuevo hijo
            MovimientoAleatorio movimientoNuevoHijo = nuevoHijo.GetComponent<MovimientoAleatorio>();

            // Heredar caracter�sticas de los padres
            float velocidadPadre = velocidadMaxima;
            float tiempoPadre = tiempoDeVida;
            float vidaPadre = vidaMaxima;

            float velocidadMadre = pareja.GetComponent<MovimientoAleatorio>().velocidadMaxima;
            float tiempoMadre = pareja.GetComponent<MovimientoAleatorio>().tiempoDeVida;
            float vidaMadre = pareja.GetComponent<MovimientoAleatorio>().vidaMaxima;

            float nuevaVelocidadMaxima = (velocidadPadre + velocidadMadre) / 2f;
            float nuevoTiempoDeVida = (tiempoPadre + tiempoMadre) / 2f;
            float nuevaVidaMaxima = (vidaPadre + vidaMadre) / 2f;

            // Aplicar mutaci�n a una caracter�stica aleatoria
            switch (Random.Range(0, 3))
            {
                case 0:
                    nuevaVelocidadMaxima *= Random.Range(0.8f, 1.2f);
                    break;
                case 1:
                    nuevoTiempoDeVida *= Random.Range(0.8f, 1.2f);
                    break;
                case 2:
                    nuevaVidaMaxima *= Random.Range(0.8f, 1.2f);
                    break;
            }

            // Asignar las caracter�sticas al nuevo hijo
            movimientoNuevoHijo.velocidadMaxima = nuevaVelocidadMaxima;
            movimientoNuevoHijo.tiempoDeVida = nuevoTiempoDeVida;
            movimientoNuevoHijo.vidaMaxima = nuevaVidaMaxima;

            // Desactiva la capacidad de reproducci�n de ambos padres para evitar m�s reproducciones
            puedeReproducirse = false;
            MovimientoAleatorio movimientoPareja = pareja.GetComponent<MovimientoAleatorio>();
            movimientoPareja.puedeReproducirse = false;

            // Mensaje de depuraci�n
            Debug.Log("Reproducci�n exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con caracter�sticas heredadas y mutaci�n.");
        }

    }
}
