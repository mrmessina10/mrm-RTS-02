using UnityEngine;

public enum CombatType { Melee, Ranged }

[RequireComponent(typeof(UnitMovement))]
public class UnitController : MonoBehaviour
{
    [Header("Combat Configuration")]
    public CombatType combatType;

    // Variables exclusivas para Ranged
    [Header("Ranged Config (Ignorar si es Melee)")]
    public GameObject projectilePrefab;
    public GameObject firePoint;

    [Header("Settings")]
    [SerializeField] private float interactionRange = 2f;

    [Header("Combat Settings")]
    [SerializeField] private float attackRate = 1.0f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private DamageType attackType = DamageType.Melee;

    [Header("Vision & Aggro Settings")]
    [SerializeField] private float visionRange = 7f; // Rango visual para detectar enemigos
    [SerializeField] private LayerMask enemyMask;

    // --- PROPIEDADES PÚBLICAS PARA LAS STATE MACHINES ---
    public UnitMovement Movement { get; private set; } 
    public float InteractionRange => interactionRange;
    public float AttackRate => attackRate;
    public int AttackDamage => attackDamage;
    public DamageType AttackType => attackType;
    public float VisionRange => visionRange;
    public LayerMask EnemyMask => enemyMask;
    public Animator UnitAnimator { get; private set; }

    // La Máquina de Estados
    private StateMachine stateMachine;

    private void Awake()
    {
        Movement = GetComponent<UnitMovement>();
        UnitAnimator = GetComponentInChildren<Animator>();
        stateMachine = new StateMachine();
    }

    private void Start()
    {
        // Estado inicial por defecto
        ChangeState(new UnitIdleState(this));
    }

    protected virtual void Update()
    {
        // Delega la ejecución frame a frame al estado actual
        stateMachine.Update();
    }

    // --- API PARA COMANDOS EXTERNOS (SelectionManager) ---

    public void ChangeState(IState newState)
    {
        if (newState != null)
        {
            stateMachine.ChangeState(newState);
        }
    }

    public virtual void SetCommand(Vector3 destination)
    {
        ChangeState(new UnitMoveState(this, destination));
    }

    public virtual void SetTarget(IInteractable newTarget)
    {
        // CORRECCIÓN 1: Usa el Factory Method en lugar de hardcodear el estado
        ChangeState(GetAttackState(newTarget));
    }

    public void TriggerAttackDamage()
    {
        // 1. Si es un ataque cuerpo a cuerpo
        if (stateMachine.CurrentState is MeleeAttackState meleeState)
        {
            meleeState.ApplyDamage();
        }
        // 2. Si es un ataque a distancia (NUEVO)
        else if (stateMachine.CurrentState is RangedAttackState rangedState)
        {
            rangedState.ApplyDamage(); // Esto es lo que instancia la flecha
        }
        // 3. Por si ocurre un evento en un estado incorrecto
        else
        {
            Debug.LogWarning($"El evento de ataque llegó, pero la unidad está en el estado {stateMachine.CurrentState?.GetType().Name}.");
        }
    }

    public IState GetAttackState(IInteractable target)
    {
        switch (combatType)
        {
            case CombatType.Ranged:
                return new RangedAttackState(this, target);
            case CombatType.Melee:
            default:
                return new MeleeAttackState(this, target);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}