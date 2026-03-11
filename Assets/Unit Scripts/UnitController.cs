using UnityEngine;

[RequireComponent(typeof(UnitMovement))]
public class UnitController : MonoBehaviour
{
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

    // La Máquina de Estados
    private StateMachine stateMachine;

    /* --- LEGACY VARIABLES ---
    private IInteractable currentTarget;
    private float attackCooldown = 0f;
    */

    private void Awake()
    {
        Movement = GetComponent<UnitMovement>();
        stateMachine = new StateMachine();
    }

    private void Start()
    {
        // Estado inicial por defecto
        ChangeState(new UnitIdleState(this));
    }

    private void Update()
    {
        // Delega la ejecución frame a frame al estado actual
        stateMachine.Update();

        /* --- LEGACY UPDATE LOGIC ---
        (Toda la lógica de movimiento, rangos y ataque ahora está en UnitAttackState y UnitMoveState)
        */
    }

    // --- API PARA COMANDOS EXTERNOS (SelectionManager) ---

    public void ChangeState(IState newState)
    {
        stateMachine.ChangeState(newState);
    }

    public void SetCommand(Vector3 destination)
    {
        /* --- LEGACY ---
        currentTarget = null; 
        Movement.MoveTo(destination);
        */
        ChangeState(new UnitMoveState(this, destination));
    }

    public void SetTarget(IInteractable newTarget)
    {
        /* --- LEGACY ---
        currentTarget = newTarget;
        */
        ChangeState(new UnitAttackState(this, newTarget));
    }

    private void OnDrawGizmosSelected()
    {
        // Círculo Rojo: Rango de Ataque / Interacción
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        // Círculo Amarillo: Rango de Visión
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}