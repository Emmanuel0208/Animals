using UnityEngine;
using TMPro;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance;

    public GameObject[] prefabsToSpawn;
    public TMP_InputField[] cantidadInstanciasPorPrefabInput;

    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        Time.timeScale = 0f;
    }

    public void ConfigurarInstancias()
    {
        int[] cantidadInstanciasPorPrefab = new int[prefabsToSpawn.Length];

        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            int.TryParse(cantidadInstanciasPorPrefabInput[i].text, out cantidadInstanciasPorPrefab[i]);
        }

        // Spawneo de los objetos
        SpawnearObjetos(cantidadInstanciasPorPrefab);
        Time.timeScale = 1f;
    }

    private void SpawnearObjetos(int[] cantidadInstanciasPorPrefab)
    {
        for (int i = 0; i < prefabsToSpawn.Length; i++)
        {
            for (int j = 0; j < cantidadInstanciasPorPrefab[i]; j++)
            {
                // Calcular una posición aleatoria dentro del área de spawneo
                Vector3 spawnPosition = new Vector3(
                    Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2f, spawnAreaCenter.x + spawnAreaSize.x / 2f),
                    Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2f, spawnAreaCenter.y + spawnAreaSize.y / 2f),
                    Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2f, spawnAreaCenter.z + spawnAreaSize.z / 2f)
                );

                // Instanciar el prefab en la posición aleatoria y con rotación predeterminada
                Instantiate(prefabsToSpawn[i], spawnPosition, Quaternion.identity);
            }
        }
    }
}
