using UnityEngine;

// Controlador de IA para enemigos: gobierna el movimiento de patrullaje mediante su StateMachine.
[RequireComponent(typeof(UnitMovement))]
public class EnemyController : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolCenter;
    [SerializeField] private float patrolRadius = 8f;
    [SerializeField] private float waitTimeAtPoint = 2f;

    public UnitMovement Movement { get; private set; }
    public Animator UnitAnimator { get; private set; }
    public Vector3 PatrolCenter { get; private set; }
    public float PatrolRadius => patrolRadius;
    public float WaitTimeAtPoint => waitTimeAtPoint;

    private StateMachine stateMachine;

    private void Awake()
    {
        Movement = GetComponent<UnitMovement>();
        UnitAnimator = GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();

        PatrolCenter = patrolCenter != null ? patrolCenter.position : transform.position;
    }

    private void Start()
    {
        ChangeState(new EnemyPatrolState(this));
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void ChangeState(IState newState)
    {
        if (newState != null)
        {
            stateMachine.ChangeState(newState);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = patrolCenter != null ? patrolCenter.position : transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, patrolRadius);
    }
}
