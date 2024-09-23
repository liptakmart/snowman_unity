using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NpcBehaviour : MonoBehaviour
{
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private SnowmanState snowmanState;
    private LevelState levelState;
    private Gun selectedGun;
    private List<Gun> ownedGuns;
    private GameObject snowmanModel;
    private GameManager manager;
    private int currentPatrolIndex;
    private NPC_MOVEMENT_STATE npcBehaviorState;
    private List<Vector3> patrolPoints;

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
        currentPatrolIndex = 0;
        npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
        patrolPoints = levelState.PatrolPoints.Select(i => i.transform.position).ToList();

        agent.speed = 4f;
        //agent.SetDestination()
    }

    // Update is called once per frame
    void Update()
    {
        switch (npcBehaviorState)
        {
            //case State.Idle:
            //    HandleIdleState();
            //    break;
            case NPC_MOVEMENT_STATE.PATROL:
                HandlePatrolState();
                break;
            //case State.Chase:
            //    HandleChaseState();
            //    break;
            //case State.Attack:
            //    HandleAttackState();
            //    break;
        }
    }

    private void HandlePatrolState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            SetDestination();
        }

        //TODO if see player, than engage
        // Check for player
        //if (IsPlayerInRange(detectionRadius))
        //{
        //    npcBehaviorState = NPC_MOVEMENT_STATE.ENGAGE;
        //    //agent.speed = 5f; // Increase speed for chasing
        //}
    }

    private void SetDestination()
    {
        if (patrolPoints.Count == 0)
            return;

        agent.SetDestination(patrolPoints[currentPatrolIndex]);
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
