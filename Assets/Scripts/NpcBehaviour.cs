using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NpcBehaviour : MonoBehaviour
{
    /// <summary>
    /// patrol points ref for npc
    /// </summary>
    public List<Vector3> PatrolPoints;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private SnowmanState snowmanState;
    private LevelState levelState;
    private Gun selectedGun;
    private List<Gun> ownedGuns;
    private GameObject snowmanModel;
    private GameManager manager;


    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        snowmanState = GetComponent<SnowmanState>();
        selectedGun = snowmanState.SelectedGun;
        ownedGuns = snowmanState.OwnedGuns;
        levelState = State._state;
        snowmanModel = snowmanState.snowmanModel;
        manager = levelState.GameManagerScriptObj;
        //agent.SetDestination()
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Die()
    {
        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }

        int snowmanId = snowmanState.SnowmanId;
        int teamId = snowmanState.TeamId;
        snowmanState.IsAlive = false;

        levelState.GameManagerRef.GetComponent<GameManager>().RemoveDeadSnowmanIdFromSpawnPoints(snowmanState.SnowmanId);
        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        levelState.PlayersSnowmanRef.Remove(gameObject);
        //StopAudio(); //TODO
        Destroy(gameObject);

        //respawn
        GameObject snowman = manager.SpawnSnowman(true, teamId, snowmanId, GUN_TYPE.PISTOL, new GUN_TYPE[] { GUN_TYPE.PISTOL });
        levelState.NpcSnowmanRef.Add(snowman);
    }
}
