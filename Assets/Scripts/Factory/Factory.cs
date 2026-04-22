using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
    public class EnemyEntry
    {
        public string id;
        public GameObject prefab;
    }


public class Factory : MonoBehaviour
{

    [SerializeField] private List<EnemyEntry> enemyList;

    public GameObject CreateEnemy(string id, Vector3 position)
    {
        foreach (var entry in enemyList)
        {
            if (entry.id == id)
            {
                return Instantiate(entry.prefab, position, Quaternion.identity);
            }
        }

        Debug.LogWarning("Tipo de enemigo no encontrado");
        return null;
    }

    public GameObject CreateRandomEnemy(Vector3 position)
    {
        if (enemyList.Count == 0)
        {
            Debug.LogWarning("La lista de enemigos estÃ¡ vacÃ­a");
            return null;
        }

        int index = Random.Range(0, enemyList.Count);
        return Instantiate(enemyList[index].prefab, position, Quaternion.identity);
    }
}
