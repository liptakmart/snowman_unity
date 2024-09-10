using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject SnowmanPrefab;
    public List<GameObject> SpawnPoints;

    private int _idSnowmanIdCounter = 1;

    // Start is called before the first frame update
    void Start()
    {
        SpawnSnowman(false);
        SpawnSnowman(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void SpawnSnowman(bool isNpc)
    {
        GameObject snowman = Instantiate(SnowmanPrefab, GetSpawnPosition(), Quaternion.identity);
        var newSnowman = snowman.GetComponent<SnowmanCombat>();
        newSnowman.snowmanId = _idSnowmanIdCounter++;
        newSnowman.isNpc = isNpc;
    }

    private Vector3 GetSpawnPosition()
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            var spawnPoint = SpawnPoints[Random.Range(0, SpawnPoints.Count - 1)];
            var script = spawnPoint.GetComponent<SpawnPoint>();
            if (script.IsEmpty())
            {
                //TODO handle if spawn point is filled, generate again
                return spawnPoint.transform.position;
            }
            
        }
        throw new UnityException("Spawn points not initialzied properly!");
    }
}

public static class Constants
{

}
