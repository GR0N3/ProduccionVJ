using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float respawnDelay = 3f;
    [SerializeField] private Factory factory;

    private GameObject currentEnemy;
    private float timer;

    void Start()
    {
        SpawnEnemy();
    }

    void Update()
    {
        if (currentEnemy == null)
        {
            timer += Time.deltaTime;

            if (timer >= respawnDelay)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }
    }

    void SpawnEnemy()
    {
        factory.CreateRandomEnemy(transform.position);
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }
}