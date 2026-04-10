using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float respawnDelay = 3f;

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
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }
}