using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject GameManagerRef;
    public GameObject SnowmanPrefabRef;
    public GameObject ProjectilePrefabRef;
    public GameObject CanvasRef;

    public List<GameObject> SpawnPoints;
    
    private int _idSnowmanIdCounter = 1;

    void Start()
    {
        InitState();
        GameObject player = SpawnSnowman(false);
        State._state.PlayersSnowmanRef.Add(player);
        SpawnSnowman(true);
        Invoke("InitAfterStart", 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitAfterStart()
    {
        State._state.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
    }

    /// <summary>
    /// Initialzies global state of level
    /// </summary>
    private void InitState()
    {
        State._state.GameManagerRef = GameManagerRef;
        State._state.GameManagerScriptObj = this;
        State._state.SpawnPoints = SpawnPoints;
        State._state.Canvas = CanvasRef;
        State._prefabs.SnowmanPrefabRef = SnowmanPrefabRef;
        State._prefabs.ProjectilePrefabRef = ProjectilePrefabRef;
    }

    public GameObject SpawnSnowman(bool isNpc)
    {
        GameObject snowman = Instantiate(SnowmanPrefabRef, GetSpawnPosition(), Quaternion.identity);
        var newSnowman = snowman.GetComponent<SnowmanCombat>();
        newSnowman.snowmanId = _idSnowmanIdCounter++;
        newSnowman.isNpc = isNpc;
        return snowman;
    }

    private Vector3 GetSpawnPosition()
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            HashSet<int> visitedIdxs = new HashSet<int>();
            while(visitedIdxs.Count < SpawnPoints.Count)
            {
                int idx = Random.Range(0, SpawnPoints.Count - 1);
                var sp = SpawnPoints[idx].GetComponent<SpawnPoint>();
                if (sp.IsEmpty())
                {
                    return sp.transform.position;
                }
                else
                {
                    if (visitedIdxs.Contains(idx))
                    {
                        continue;
                    }
                    else
                    {
                        visitedIdxs.Add(idx);
                    }
                }
            }
            //all are filled, return first
            return SpawnPoints[0].transform.position;
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
    public List<GameObject> SpawnPoints = new List<GameObject>();
    public List<GameObject> PlayersSnowmanRef = new List<GameObject>();
    public GameObject Canvas;
}
