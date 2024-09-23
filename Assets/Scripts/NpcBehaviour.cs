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

    // Vision Parameters
    [Header("Vision Parameters")]
    [Tooltip("The maximum distance the NPC can see.")]
    public float visionRange = 5f;

    [Tooltip("The angle (in degrees) that defines the NPC's field of view.")]
    [Range(0, 360)]
    public float fieldOfView = 120f;

    [Tooltip("Layer mask to specify which layers are considered as potential targets.")]
    public LayerMask targetMask;

    [Tooltip("Layer mask to specify which layers can obstruct the NPC's vision.")]
    public LayerMask obstructionMask;

    [Tooltip("Time interval between vision checks in seconds.")]
    public float visionCheckInterval = 0.2f;

    // Detected target
    private Transform visibleTarget;

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

        agent.speed = 2f;
        StartCoroutine(VisionRoutine());
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
            case NPC_MOVEMENT_STATE.ENGAGE:
                HandleEngageState();
                break;
            //case State.Chase:
            //    HandleChaseState();
            //    break;
            //case State.Attack:
            //    HandleAttackState();
            //    break;
        }
    }

    IEnumerator VisionRoutine()
    {
        while (true)
        {
            FindVisibleTarget();
            yield return new WaitForSeconds(visionCheckInterval);
        }
    }

    private void FindVisibleTarget()
    {
        // Reset the current visible target
        visibleTarget = null;

        // Step 1: Find all potential targets within vision range
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, visionRange, targetMask);

        foreach (Collider targetCollider in targetsInRange)
        {
            if (targetCollider.gameObject.tag == Constants.TAG_PLAYER)
            {
                Transform target = targetCollider.transform;

                // Step 2: Calculate direction to target
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // Step 3: Check if target is within field of view
                if (Vector3.Angle(transform.forward, directionToTarget) < fieldOfView / 2)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    // Step 4: Check for line of sight using raycast
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        // Target is visible
                        visibleTarget = target;
                        EngageTarget();
                        return; // Engage the first visible target found
                    }
                }
            }
        }

        // No target is visible, resume patrol if not already in patrol state
        if (npcBehaviorState != NPC_MOVEMENT_STATE.PATROL)
        {
            npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
            agent.speed = 2.5f;
            SetDestination();
        }
    }

    private void EngageTarget()
    {
        if (visibleTarget != null)
        {
            npcBehaviorState = NPC_MOVEMENT_STATE.ENGAGE;
            agent.speed = 5f; // Increase speed for chasing
            agent.SetDestination(visibleTarget.position);
            Debug.Log("Engaging");
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

    private void HandleEngageState()
    {
        if (visibleTarget != null)
        {
            // Update the destination to the target's current position
            agent.SetDestination(visibleTarget.position);
        }
        else
        {
            // If the target is no longer visible, switch back to patrol
            npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
            agent.speed = 4f;
            SetDestination();
        }
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

    void OnDrawGizmos()
    {
        // Draw the vision range sphere
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Draw the field of view lines
        Vector3 forward = transform.forward * visionRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2, 0) * forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // Draw a line to the visible target
        if (visibleTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, visibleTarget.position);
        }
    }
}
