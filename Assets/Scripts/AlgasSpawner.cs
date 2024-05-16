using UnityEngine;

public class AlgaeSpawner : MonoBehaviour
{
    public GameObject algaePrefab; // Prefab del alga a instanciar
    public Vector3 spawnAreaCenter; // Centro del área de spawneo
    public Vector3 spawnAreaSize; // Tamaño del área de spawneo

    void Start()
    {
        InvokeRepeating("SpawnAlgae", 0f, 5f); // Invocar repetidamente la función de spawn cada segundo
    }

    void SpawnAlgae()
    {
        for (int i = 0; i < 10; i++) // Spawnear 5 algas en cada invocación
        {
            // Calcular una posición aleatoria dentro del área de spawneo
            Vector3 spawnPosition = new Vector3(
                Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2f, spawnAreaCenter.x + spawnAreaSize.x / 2f),
                Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2f, spawnAreaCenter.y + spawnAreaSize.y / 2f),
                Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2f, spawnAreaCenter.z + spawnAreaSize.z / 2f)
            );

            // Instanciar el prefab del alga en la posición aleatoria y con rotación predeterminada
            Instantiate(algaePrefab, spawnPosition, Quaternion.identity);
        }
    }
}

