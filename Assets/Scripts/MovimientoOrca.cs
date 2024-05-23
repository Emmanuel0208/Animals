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
    [SerializeField] private GameObject nuevoHijoPrefab; //prefab del nuevo hijo

    [SerializeField] private Vector3 limiteMinimo;
    [SerializeField] private Vector3 limiteMaximo;

    private float contadorCambioDireccion;
    private float tiempoDesdeUltimaReproduccion; //tiempo desde la �ltima reproducci�n
    private Rigidbody rb;
    private Vector3 objetivoPosicion; // Posici�n objetivo hacia la que se dirigir�

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        contadorCambioDireccion = 0f;
        tiempoDesdeUltimaReproduccion = 0f; //Inicializa el tiempo desde la �ltima reproducci�n
        vidaMaxima = Random.Range(2, 51);
        vidaActual = vidaMaxima;
        tiempoDeVida = Random.Range(60, 80);
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
                if (tiempoDesdeUltimaReproduccion >= 15f) //�ltima reproducci�n
                {
                    puedeReproducirse = true;
                    Reproducirse();
                    tiempoDesdeUltimaReproduccion = 0f; //Reinicia el tiempo desde la �ltima reproducci�n
                }
            }

            else if (segundosRestantesDeVida > 0)
            {
                if (segundosRestantesDeVida < 60)
                {
                    Debug.Log("Comenzando a buscar objetivos...");
                    BuscarObjetivo();
                    yield return new WaitForSeconds(5f); //Espera 5 segundos antes de buscar objetivo nuevamente
                }
                else
                {
                    MoverAleatoriamente();
                }
            }

            tiempoDesdeUltimaReproduccion += Time.deltaTime; //Incrementa el tiempo desde la �ltima reproducci�n
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
        //mueve gradualmente hacia �l objetivo
        if (objetivoPosicion != Vector3.zero)
        {
            MoverHaciaObjetivo();
        }
        else
        {
            //Si no hay un objetivo, mueve aleatoriamente
            rb.velocity = transform.forward * velocidadMaxima;
        }
    }

    //M�todo para destruir la orca
    void DestruirObjeto()
    {
        Destroy(gameObject);
    }

    //M�todo para recibir da�o
    public void RecibirDa�o(int cantidad)
    {
        vidaActual -= cantidad;
    }

    //M�todo llamado cuando se produce una colisi�n
    void OnTriggerEnter(Collider other)
    {
        foreach (string etiquetaObjetivo in etiquetasObjetivo)
        {
            if (other.CompareTag(etiquetaObjetivo))
            {
                //Aumenta la vida actual y el tiempo restante de vida, y destruye el objetivo
                vidaActual += Random.Range(5, 11);
                segundosRestantesDeVida += Random.Range(15f, 20f);
                Destroy(other.gameObject);
                break;
            }
        }
    }

    // M�todo para buscar un objetivo
    void BuscarObjetivo()
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

    //M�todo para mover hacia el objetivo
    void MoverHaciaObjetivo()
    {
        // Calcula la direcci�n hacia el objetivo
        Vector3 direccion = (objetivoPosicion - transform.position).normalized;
        //Mueve el objeto hacia el objetivo
        rb.velocity = direccion * Mathf.Min(velocidadMaxima, Vector3.Distance(transform.position, objetivoPosicion));
    }

    // M�todo para cambiar la direcci�n de la orca
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

            // Heredar caracter�sticas de los padres
            float velocidadPadre = velocidadMaxima;
            float tiempoPadre = tiempoDeVida;
            float vidaPadre = vidaMaxima;

            float velocidadMadre = pareja.GetComponent<MovimientoOrca>().velocidadMaxima;
            float tiempoMadre = pareja.GetComponent<MovimientoOrca>().tiempoDeVida;
            float vidaMadre = pareja.GetComponent<MovimientoOrca>().vidaMaxima;

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
            MovimientoOrca movimientoPareja = pareja.GetComponent<MovimientoOrca>();
            movimientoPareja.puedeReproducirse = false;

            // Mensaje de depuraci�n
            Debug.Log("Reproducci�n exitosa entre " + gameObject.name + " y " + pareja.name + ". Nuevo hijo creado con caracter�sticas heredadas y mutaci�n.");
        }

    }
}
