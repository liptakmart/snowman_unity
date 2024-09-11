using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject GameManagerRef;
    public GameObject SnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;

    public List<GameObject> SpawnPoints;
    
    private int _idSnowmanIdCounter = 1;

    // Start is called before the first frame update
    void Start()
    {
        InitState();
        SpawnSnowman(false);
        SpawnSnowman(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Initialzies global state of level
    /// </summary>
    private void InitState()
    {
        State._state.GameManagerRef = GameManagerRef;
        State._state.GameManagerScriptObj = this;
        State._state.SpawnPoints = SpawnPoints;
        State._prefabs.SnowmanPrefabRef = SnowmanPrefabRef;
        State._prefabs.ProjectilePrefabRef = ProjectilePrefabRef;
    }

    public void SpawnSnowman(bool isNpc)
    {
        GameObject snowman = Instantiate(SnowmanPrefabRef, GetSpawnPosition(), Quaternion.identity);
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

public static class State
{
    public static LevelState _state = new LevelState();
    public static Prefabs _prefabs = new Prefabs();
}

public class Prefabs
{
    public GameObject SnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;
}

public class LevelState
{
    public GameObject GameManagerRef;
    public GameManager GameManagerScriptObj;
    public List<GameObject> SpawnPoints;
}
