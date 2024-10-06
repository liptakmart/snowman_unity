using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private List<GameObject> gunSpawnPointsGo;
    private List<GunSpawnPoint> gunSpawnPointScripts;

    private List<GameObject> snowmanSpawnPointsGo;
    private List<SpawnPoint> snowmanSpawnPointScripts;

    private LevelState levelState;
    private Prefabs prefabs;
    // Start is called before the first frame update
    void Start()
    {
        levelState = State._state;
        prefabs = State._prefabs;
        gunSpawnPointsGo = levelState.GunSpawnPoints;
        gunSpawnPointScripts = gunSpawnPointsGo.Select(x => x.GetComponent<GunSpawnPoint>()).ToList();

        snowmanSpawnPointsGo = levelState.SnowmanSpawnPoints;
        snowmanSpawnPointScripts = snowmanSpawnPointsGo.Select(x => x.GetComponent<SpawnPoint>()).ToList();
        InitialSnowmenSpawn();

        StartCoroutine(HandleGunSpawningCoroutine());
    }

    /// <summary>
    /// Handles initial spawning in level for snowmen
    /// </summary>
    private void InitialSnowmenSpawn()
    {
        var player1 = SpawnSnowman(false, 0, null, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL, GUN_TYPE.SHOTGUN, GUN_TYPE.SMG });
        State._state.PlayerStats.Add(new PlayerStat(player1.GetComponent<SnowmanState>().SnowmanId, "Player1", 0, true, 0, 0, 0, false));

        var player2 = SpawnSnowman(false, 1, null, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL, GUN_TYPE.SHOTGUN, GUN_TYPE.SMG });
        State._state.PlayerStats.Add(new PlayerStat(player2.GetComponent<SnowmanState>().SnowmanId, "Player2", 1, true, 1, 0, 0, false));

        var npc02 = SpawnSnowman(true, 2, null, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        var npc03 = SpawnSnowman(true, 2, null, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
    }

    public void RespawnSnowman(bool isNpc, int snowmanId, int teamId)
    {
        StartCoroutine(RespawnAfterDelay(isNpc, snowmanId, teamId));
    }

    private IEnumerator RespawnAfterDelay(bool isNpc, int snowmanId, int teamId)
    {
        yield return new WaitForSeconds(Constants.RESPAWN_TIME_SEC);
        if (!isNpc)
        {
            var player = State._state.PlayerStats.FirstOrDefault(x => x.Id == snowmanId);
            SpawnSnowman(false, player.TeamId, player.Id, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        }
        else
        {
            SpawnSnowman(true, teamId, snowmanId, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        }
    }

    private GameObject SpawnSnowman(bool isNpc, int teamId, int? snowmanId, GUN_TYPE gunType, GUN_TYPE[] ownedGuns)
    {
        GameObject snowmanSpawn = GetSnowmanSpawn();
        GameObject snowman = Instantiate(isNpc ? State._prefabs.NpcSnowmanPrefabRef : State._prefabs.PlayerSnowmanPrefabRef, snowmanSpawn.transform.position, Quaternion.identity);
        
        // add snowman inside manually, because at initial spawn physics check may not fire yet to check if someone is inside
        snowmanSpawnPointScripts[snowmanSpawnPointsGo.IndexOf(snowmanSpawn)].AddObjectInside(snowman);
        SnowmanState snowmanState = snowman.GetComponent<SnowmanState>();
        snowmanState.Initialize(snowmanId, isNpc, teamId, gunType, ownedGuns);
        if (isNpc)
        {
            State._state.NpcList.Add(snowman);
        }
        else
        {
            State._state.PlayerList.Add(snowman);
        }
        return snowman;
    }

    private GameObject GetSnowmanSpawn()
    {
        if (snowmanSpawnPointsGo != null && snowmanSpawnPointsGo.Count > 0)
        {
            ShuffleList(snowmanSpawnPointsGo);
            snowmanSpawnPointScripts = snowmanSpawnPointsGo.Select(x => x.GetComponent<SpawnPoint>()).ToList();

            for (int i = 0; i < snowmanSpawnPointsGo.Count; i++)
            {
                if (snowmanSpawnPointScripts[i].GetIfSomeoneInside() == null)
                {
                    return snowmanSpawnPointsGo[i];
                }
            }

            // All spawn points are occupied
            // Handle as needed: wait and retry, throw exception, or return a default position
            throw new Exception("No empty spawn points available");
        }
        throw new UnityException("Spawn points not initialized properly!");
    }

    /// <summary>
    /// Spawns one gun
    /// </summary>
    private void SpawnGun()
    {
        GameObject gunGo = null;
        int rng = UnityEngine.Random.Range(0, 3);
        switch (rng)
        {
            case 0: gunGo = prefabs.PickablePistol; break;
            case 1: gunGo = prefabs.PickableShotgun; break;
            case 2: gunGo = prefabs.PickableSmg; break;
        }

        var spawnPoint = GetFreeGunSpawnPoint();
        if (spawnPoint == null)
        {
            return;
        }
        var spawnPos = gunSpawnPointsGo[gunSpawnPointScripts.IndexOf(spawnPoint)].transform.position;
        var gun = Instantiate(gunGo, spawnPos, gunGo.transform.rotation);
        spawnPoint.pickableGun = gun;
    }

    /// <summary>
    /// Gets empty gun spawn point
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnityException"></exception>
    private GunSpawnPoint GetFreeGunSpawnPoint()
    {
        ShuffleList(gunSpawnPointsGo);
        gunSpawnPointScripts = gunSpawnPointsGo.Select(x => x.GetComponent<GunSpawnPoint>()).ToList();

        for (int i = 0; i < gunSpawnPointScripts.Count; i++)
        {
            if (gunSpawnPointScripts[i].pickableGun == null)
            {
                return gunSpawnPointScripts[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Returns number of guns that are actually actively spawned in level
    /// </summary>
    /// <returns></returns>
    private int GetNumberOfSpawnedGuns()
    {
        int counter = 0;
        foreach (var item in gunSpawnPointScripts)
        {
            if (item.pickableGun != null)
            {
                counter++;
            }
        }
        return counter;
    }

    /// <summary>
    /// Manages gun spawning
    /// </summary>
    private IEnumerator HandleGunSpawningCoroutine()
    {
        yield return new WaitForSeconds(Constants.DELAY_TO_START_SPAWN_GUNS);
        while (true)
        {
            if (GetNumberOfSpawnedGuns() < Constants.MAX_NUM_OF_PICKABLE_GUNS_IN_LEVEL)
            {
                SpawnGun();
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(Constants.DELAY_TO_SPAWN_GUNS_LOWER_BOUND, Constants.DELAY_TO_SPAWN_GUNS_UPPER_BOUND));
        }
    }

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
}
