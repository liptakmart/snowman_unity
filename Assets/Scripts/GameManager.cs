using System;
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
        GameObject[] snowmenArr = SpawnSnowmen(new bool[] { false, true}, new Color[] { Color.blue, Color.red});
        State._state.PlayersSnowmanRef.Add(snowmenArr[0]);
        Invoke("InitAfterStart", 0.5f); //TODO necessary?
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

    public GameObject[] SpawnSnowmen(bool[] isNpcArr, Color[] cylinderColorArr)
    {
        List<GameObject> snowmenList = new List<GameObject>();
        Vector3[] spawnPositions = GetSpawnPositions(isNpcArr.Length);

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject snowman = Instantiate(SnowmanPrefabRef, spawnPositions[i], Quaternion.identity);
            snowmenList.Add(snowman);
            SnowmanCombat snowmanLogic = snowman.GetComponent<SnowmanCombat>();
            snowmanLogic.cylinderColor = cylinderColorArr[i];
            snowmanLogic.snowmanId = _idSnowmanIdCounter++;
            if (isNpcArr[i])
            {
                snowmanLogic.isNpc = true;
            }
            else
            {
                snowmanLogic.isNpc = false;
            }
        }
        return snowmenList.ToArray();
    }

    public GameObject SpawnSnowman(bool isNpc, Color cylinderColor)
    {
        GameObject snowman = Instantiate(SnowmanPrefabRef, GetSpawnPosition(), Quaternion.identity);
        var newSnowman = snowman.GetComponent<SnowmanCombat>();
        newSnowman.snowmanId = _idSnowmanIdCounter++;
        newSnowman.cylinderColor = cylinderColor;
        newSnowman.isNpc = isNpc;
        return snowman;
    }

    private Vector3 GetSpawnPosition()
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            // Create a copy of the spawn points list
            List<GameObject> shuffledSpawnPoints = new List<GameObject>(SpawnPoints);
            // Shuffle the list
            ShuffleList(shuffledSpawnPoints);

            // Iterate over the shuffled list
            foreach (var spawnPoint in shuffledSpawnPoints)
            {
                var sp = spawnPoint.GetComponent<SpawnPoint>();
                if (sp.NoSnowmanInside())
                {
                    return sp.transform.position;
                }
            }
            // All spawn points are occupied
            // Handle as needed: wait and retry, throw exception, or return a default position
            throw new Exception("No empty spawn points available");
        }
        throw new UnityException("Spawn points not initialized properly!");
    }

    private Vector3[] GetSpawnPositions(int numOfPos)
    {
        if (SpawnPoints != null && SpawnPoints.Count > 0)
        {
            List<Vector3> positions = new List<Vector3>();
            // Create a copy of the spawn points list
            List<GameObject> shuffledSpawnPoints = new List<GameObject>(SpawnPoints);
            // Shuffle the list
            ShuffleList(shuffledSpawnPoints);

            // Iterate over the shuffled list
            foreach (var spawnPoint in shuffledSpawnPoints)
            {
                var sp = spawnPoint.GetComponent<SpawnPoint>();
                if (sp.NoSnowmanInside())
                {
                    positions.Add(sp.transform.position);
                }

                if (positions.Count == numOfPos)
                {
                    return positions.ToArray();
                }
            }

            throw new Exception("No empty spawn points available");
        }
        throw new UnityException("Spawn points not initialized properly!");
    }

    // Helper method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = n - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    /// <summary>
    /// Removes dead snowman id from spawn points where he entered. Because he is no longer there, cause he is dead.
    /// </summary>
    /// <param name="deadSnowmanId"></param>
    public void RemoveDeadSnowmanIdFromSpawnPoints(int deadSnowmanId)
    {
        foreach (GameObject item in SpawnPoints)
        {
            SpawnPoint script = item.GetComponent<SpawnPoint>();
            script.TryRemoveSnowman(deadSnowmanId);
        }
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

public static class Constants
{
    public static readonly string TAG_SNOWMAN = "SnowmanTag";
}