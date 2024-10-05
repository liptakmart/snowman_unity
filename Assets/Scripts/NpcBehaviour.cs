using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NpcBehaviour : MonoBehaviour
{
    // Components and references
    private NavMeshAgent agent;
    private SnowmanState snowmanState;
    private LevelState levelState;
    private Gun selectedGun;
    private List<Gun> ownedGuns;
    private GameObject snowmanModel;
    private GameManager gameManager;
    private int currentPatrolIndex;
    private NPC_MOVEMENT_STATE npcBehaviorState;
    private List<Vector3> patrolPoints;
    private SpawnManager spawnManager;

    public Gun SelectedGun { get { return this.selectedGun; } }

    // Vision Parameters
    [Header("Vision Parameters")]
    [Tooltip("The maximum distance the NPC can see.")]
    public float visionRange = 50f;

    [Tooltip("The angle (in degrees) that defines the NPC's field of view.")]
    [Range(0, 360)]
    public float fieldOfView = 120f;

    [Tooltip("Layer mask to specify which layers are considered as potential targets.")]
    public LayerMask targetMask;

    [Tooltip("Layer mask to specify which layers can obstruct the NPC's vision.")]
    public LayerMask obstructionMask;

    [Tooltip("Time interval between vision checks in seconds.")]
    public float visionCheckInterval = 0.2f;

    // Memory Parameters
    [Header("Memory Parameters")]
    [Tooltip("Duration (in seconds) the NPC will remember the player after losing sight.")]
    public float memoryDuration = 5f;

    // Movement Parameters
    [Header("Movement Parameters")]
    [Tooltip("The minimum distance the NPC maintains from the player's last known position.")]
    public float minDistanceToPlayer = 2f;

    // Rotation Parameters
    [Header("Rotation Parameters")]
    [Tooltip("Rotation speed when turning towards the player.")]
    public float rotationSpeed = 5f;

    // Search Parameters
    [Header("Search Parameters")]
    [Tooltip("Rotation speed when searching for the player.")]
    public float searchTurnSpeed = 60f;

    //[Header("Engagement Parameters")]
    //[Tooltip("Delay before the NPC engages the target after detection (in seconds).")]
    public float delayToEngage = 0.7f;

    private Coroutine engageCoroutine;

    // Timers and state tracking
    private float timeSinceLastSeen = 0f;
    private Transform visibleTarget;
    private Vector3 lastKnownPosition;
    private Coroutine visionCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize components and variables
        agent = GetComponent<NavMeshAgent>();
        snowmanState = GetComponent<SnowmanState>();
        selectedGun = snowmanState.SelectedGun;
        ownedGuns = snowmanState.OwnedGuns;
        levelState = State._state;
        snowmanModel = snowmanState.snowmanModel;
        gameManager = levelState.GameManagerScriptObj;
        currentPatrolIndex = 0;
        npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
        patrolPoints = levelState.PatrolPoints.Select(i => i.transform.position).ToList();
        spawnManager = levelState.SpawnManagerScriptObj;

        agent.speed = 1f;
        SetDestination();

        // Start vision checking coroutine
        visionCoroutine = StartCoroutine(VisionRoutine());

        SubscribeToEvents();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(npcBehaviorState.ToString());
        switch (npcBehaviorState)
        {
            case NPC_MOVEMENT_STATE.PATROL:
                HandlePatrolState();
                break;
            case NPC_MOVEMENT_STATE.FOLLOW:
                HandleFollowState();
                break;
            case NPC_MOVEMENT_STATE.ATTACK:
                HandleAttackState();
                break;
        }
    }

    private void SubscribeToEvents()
    {
        snowmanState.OnIKilledSomeone += HandleOnMyKill;
        //gun.OnReloadStarted += HandleReloadStarted;
        //gun.OnReloadFinished += HandleReloadFinished;
        //gun.OnReloadCanceled += HandleReloadCanceled;
        //gun.OnFired += HandleFired;
        //gun.OnReadyToFire += HandleReadyToFire; // Subscribe to new event
        //gun.OnAmmoChanged += HandleAmmoChanged;
    }

    private void UnsubscribeFromEvents()
    {
        snowmanState.OnIKilledSomeone -= HandleOnMyKill;
        //gun.OnReloadStarted -= HandleReloadStarted;
        //gun.OnReloadFinished -= HandleReloadFinished;
        //gun.OnReloadCanceled -= HandleReloadCanceled;
        //gun.OnFired -= HandleFired;
        //gun.OnReadyToFire -= HandleReadyToFire; // Unsubscribe from new event
        //gun.OnAmmoChanged -= HandleAmmoChanged;
    }

    private void HandleOnMyKill(int enemyId)
    {
        Debug.Log("Killed enemy:" + enemyId);
        ResetNpcState();
    }
    public void OnGunPicked(GUN_TYPE gunType)
    {
        snowmanState.AddGunOrAmmo(gunType);
        //Debug.Log("gun picked");
    }

    // Coroutine for periodic vision checks
    IEnumerator VisionRoutine()
    {
        while (true)
        {
            bool targetVisible = FindVisibleTarget();

            if (targetVisible)
            {
                // Reset timer when target is visible
                timeSinceLastSeen = 0f;
            }
            else
            {
                if (npcBehaviorState == NPC_MOVEMENT_STATE.ATTACK)
                {
                    // Lost sight of the player, switch to FOLLOW state
                    npcBehaviorState = NPC_MOVEMENT_STATE.FOLLOW;
                    agent.isStopped = false;
                    agent.speed = 1.8f;
                    agent.SetDestination(lastKnownPosition);
                    //Debug.Log($"{gameObject.name} lost sight, switching to FOLLOW state.");
                }
                else if (npcBehaviorState == NPC_MOVEMENT_STATE.FOLLOW)
                {
                    // Increment the timer in FOLLOW state
                    timeSinceLastSeen += visionCheckInterval;

                    if (timeSinceLastSeen >= memoryDuration)
                    {
                        // Time expired, switch back to PATROL
                        npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
                        agent.isStopped = false;
                        agent.speed = 1f; // Adjust patrol speed as needed
                        SetDestination();
                        //Debug.Log($"{gameObject.name} lost the target and is returning to PATROL.");
                    }
                }
            }

            yield return new WaitForSeconds(visionCheckInterval);
        }
    }

    private IEnumerator EngageTargetWithDelay()
    {
        Debug.Log("delay:" + delayToEngage);
        // Wait for the specified delay
        yield return new WaitForSeconds(delayToEngage);
        
        // Check again if the target is still visible before engaging
        if (visibleTarget != null)
        {
            if (visibleTarget != null)
            {
                npcBehaviorState = NPC_MOVEMENT_STATE.ATTACK;
                agent.isStopped = true; // Stop moving
                agent.ResetPath();
                //Debug.Log($"{gameObject.name} is now in ATTACK state.");
            }
        }

        // Reset the coroutine tracker
        engageCoroutine = null;
    }

    // Method to find visible targets
    private bool FindVisibleTarget()
    {
        // Reset the current visible target
        visibleTarget = null;
        bool targetDetected = false;

        // Determine if we should check FOV
        bool checkFOV = true;

        // If in FOLLOW state, expand FOV
        if (npcBehaviorState == NPC_MOVEMENT_STATE.FOLLOW)
        {
            checkFOV = false; // Don't limit by FOV when searching
        }

        // Step 1: Find all potential targets within vision range
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, visionRange, targetMask);

        Transform closestTarget = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider targetCollider in targetsInRange)
        {
            if (targetCollider.gameObject.GetComponent<SnowmanState>().TeamId != snowmanState.TeamId)
            {
                Transform target = targetCollider.transform;

                // Ensure the player is alive
                SnowmanState player = targetCollider.GetComponent<SnowmanState>();
                if (player != null && !player.IsAlive)
                    continue;

                // Step 2: Calculate direction to target
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                bool inFieldOfView = true;
                // Step 3: Check if target is within field of view
                if (checkFOV)
                {
                    if (Vector3.Angle(transform.forward, directionToTarget) > fieldOfView / 2)
                    {
                        inFieldOfView = false;
                    }
                }

                if (inFieldOfView)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);

                    // Optional: Visualize the raycast in the Scene view
                    Debug.DrawRay(transform.position, directionToTarget * distanceToTarget, Color.blue);

                    // Step 4: Check for line of sight using raycast
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    {
                        // Target is visible; prioritize the closest one
                        if (distanceToTarget < shortestDistance)
                        {
                            shortestDistance = distanceToTarget;
                            closestTarget = target;
                        }
                    }
                }
            }
        }

        if (closestTarget != null)
        {
            visibleTarget = closestTarget;
            lastKnownPosition = visibleTarget.position;

            // Start the engagement delay coroutine if not already running
            if (engageCoroutine == null)
            {
                engageCoroutine = StartCoroutine(EngageTargetWithDelay());
            }

            targetDetected = true;
            //Debug.Log($"{gameObject.name} detected and is engaging {visibleTarget.name}");
        }

        return targetDetected;
    }

    // Handle behavior in PATROL state
    private void HandlePatrolState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            // Move to next patrol point
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            SetDestination();
        }
    }

    // Handle behavior in FOLLOW state
    private void HandleFollowState()
    {
        if (visibleTarget != null)
        {
            // Regained sight of the player, switch back to ATTACK state
            npcBehaviorState = NPC_MOVEMENT_STATE.ATTACK;
            agent.isStopped = true;
            agent.ResetPath();
            //Debug.Log($"{gameObject.name} regained sight, switching to ATTACK state.");
        }
        else
        {
            // Move towards last known position
            float distanceToLastKnown = Vector3.Distance(transform.position, lastKnownPosition);

            if (distanceToLastKnown > minDistanceToPlayer)
            {
                agent.isStopped = false;
                agent.SetDestination(lastKnownPosition);
                //Debug.Log($"{gameObject.name} is following to last known position.");
            }
            else
            {
                // Reached close enough to last known position, stop and look around
                agent.isStopped = true;
                agent.ResetPath();

                // Rotate to look around
                SearchForTarget();
                //Debug.Log($"{gameObject.name} reached last known position and is searching.");
            }
        }
    }

    // Handle behavior in ATTACK state
    private void HandleAttackState()
    {
        if (visibleTarget != null)
        {
            // Ensure the agent is stopped
            agent.isStopped = true;
            agent.ResetPath();

            // Rotate to face the player
            RotateTowards(visibleTarget.position);

            // Perform attack logic here
            PerformAttack();
        }
        else
        {
            //reset
            StopCoroutine(engageCoroutine);
            engageCoroutine = null;

            // Lost sight of the player, switch to FOLLOW state
            npcBehaviorState = NPC_MOVEMENT_STATE.FOLLOW;
            agent.isStopped = false;
            agent.speed = 1.8f; // Set follow speed
            agent.SetDestination(lastKnownPosition);
            //Debug.Log($"{gameObject.name} lost sight, switching to FOLLOW state.");
        }
    }

    // Rotate towards a given position
    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    // Method to rotate when searching
    private void SearchForTarget()
    {
        // Rotate slowly to look around
        transform.Rotate(0, searchTurnSpeed * Time.deltaTime, 0);
    }

    // Placeholder for attack logic
    private void PerformAttack()
    {
        selectedGun.Fire(snowmanModel, snowmanState);
    }

    // Method to set destination for patrol
    private void SetDestination()
    {
        if (patrolPoints.Count == 0)
            return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex]);
    }
    public void Die()
    {
        //respawn
        spawnManager.RespawnSnowman(true, snowmanState.SnowmanId, snowmanState.TeamId);

        Collider col = GetComponent<Collider>();
        Rigidbody rb = GetComponent<Rigidbody>();
        if (col != null && rb != null)
        {
            Destroy(col);
            Destroy(rb);
        }
        snowmanState.IsAlive = false;

        levelState.Canvas.GetComponent<CanvasManager>().UpdateWeaponUI();
        levelState.NpcList.Remove(gameObject);
        //StopAudio(); //TODO
        Destroy(gameObject);
    }

    private void ResetNpcState()
    {
        npcBehaviorState = NPC_MOVEMENT_STATE.PATROL;
        agent.isStopped = false;
        agent.speed = 1f;
        SetDestination();
        lastKnownPosition = Vector3.zero;
        timeSinceLastSeen = 0f;
        visibleTarget = null;
    }

    // Gizmos for visualization
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

    // Stop the vision coroutine when disabled
    void OnDisable()
    {
        if (visionCoroutine != null)
        {
            StopCoroutine(visionCoroutine);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
        snowmanState.IsAlive = false;
    }
}
