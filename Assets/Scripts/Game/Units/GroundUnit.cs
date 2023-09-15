using System;
using System.Collections;
using Core;
using Core.Utils;
using UnityEngine;

public class GroundUnit : MonoBehaviour, IUnit
{
    public event Action<Transform> OnTargetSpotted;
    public event Action<IUnit> OnUnitDown;
    public event Action<float, float> OnHealthChanged;
    public event Action OnAttackComplete;

    public float Life
    {
        get => _life;
        private set
        {
            _life = Mathf.Clamp(value, 0, _maxLife);
            OnHealthChanged?.Invoke(_life, _maxLife);
        }
    }

    public GameObject GameObject => gameObject;

    [field: SerializeField] public int TeamID { get; set; } = 0;

    private float _life;
    [SerializeField] private float _maxLife = 100f;
    [SerializeField] private float _lifeThreshold = 35f;

    [SerializeField] private float _speed = 2;
    [SerializeField] private float _speedRot = 0.01f;
    [SerializeField] private float _attackDistance = 1f;
    [SerializeField] private float _attackCooldown = .25f;
    
    [Header("Obstacle avoidance")]
    [SerializeField] private float _radius;
    [SerializeField] private float _avoidWeight;
    [SerializeField] private LayerMask _mask;
    [SerializeField] private int _rayAmount = 18;
    [SerializeField] private float _angle = 90f;
    private ObstacleAvoidance _obstacleAvoidance;

    [Header("LoS")]
    [SerializeField] private LayerMask _entitiesMask;
    [SerializeField] private float _losRange;
    [SerializeField] private int _losAngle;

    private FSM _fsm;
    private FlockState _flockState;
    private AttackState _attackState;
    private FleeState _fleeState;

    private Rigidbody _rb;
    private Animator _animator;
    private Weapon _weapon;

    private QuestionNode _isLifeAboveThreshold;
    private QuestionNode _isEnemyInSight;
    private ActionNode _flee;
    private ActionNode _seekLeader;
    private ActionNode _attackEnemy;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int Death = Animator.StringToHash("Death");

    private void Awake()
    {
        // Initial setup
        Life = _maxLife;
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _obstacleAvoidance = new ObstacleAvoidance(transform, _radius, _avoidWeight, _mask, _rayAmount, _angle);
        _weapon = GetComponentInChildren<Weapon>();
        _weapon.gameObject.SetActive(false);

        // Set FSM
        _flockState = new FlockState(this);
        _attackState = new AttackState(this, _attackDistance, _attackCooldown);
        _fleeState = new FleeState(this);

        _flockState.AddTransition(_attackState);
        _flockState.AddTransition(_fleeState);
        
        _attackState.AddTransition(_flockState);
        _attackState.AddTransition(_fleeState);

        _fleeState.AddTransition(_flockState);
        
        _fsm = new FSM(_flockState);

        // Set desition tree
        _attackEnemy = new ActionNode(() => _fsm.Transition(_attackState));
        _seekLeader = new ActionNode(() => _fsm.Transition(_flockState));
        _flee = new ActionNode(() => _fsm.Transition(_fleeState));
        _isEnemyInSight = new QuestionNode(CheckForEnemy, _attackEnemy, _seekLeader);
        _isLifeAboveThreshold = new QuestionNode(() => _life >= _lifeThreshold, _isEnemyInSight, _flee);

        // Subscribe to events
        OnHealthChanged += EvaluateActions;
        OnHealthChanged += CheckDead;
        OnAttackComplete += EvaluateActions;
        _attackState.OnTargetGone += EvaluateActions;
    }

    private void Update()
    {
        if (CheckForEnemy()) EvaluateActions();
    }

    private void LateUpdate()
    {
        _fsm.OnUpdate();
    }

    private void EvaluateActions()
    {
        _isLifeAboveThreshold.Execute();
    }

    private void EvaluateActions(float healthAmount, float maxHealth)
    {
        if (healthAmount <= 0) return;
        EvaluateActions();
    }

    public void Move(Vector3 dir)
    {
        if (Life <= 0) return;
        var oaDir = _obstacleAvoidance.GetDir();
        var direction = _obstacleAvoidance.Detected
            ? Vector3.Lerp(oaDir, dir, Vector3.Dot(oaDir, dir))
            : dir;

        direction.y = 0;
        _rb.velocity = direction.normalized * _speed;
        transform.forward = Vector3.Lerp(transform.forward, direction, _speedRot);

        var animSpeed = Mathf.Lerp(_rb.velocity.magnitude, _animator.GetFloat(Speed), .5f);
        _animator.SetFloat(Speed, animSpeed);
    }

    public void Stop()
    {
        _rb.velocity = Vector3.zero;
        _animator.SetFloat(Speed, 0f);
    }

    public void DoAttack()
    {
        StartCoroutine(AttackCo());
    }

    public void HealUp(IHealth healthSource, float healAmount)
    {
        if (healthSource == null) return;
        Life += healAmount;
    }

    public void GetDamaged(IDamager damager, float damageAmount)
    {
        if (damager == null) return;
        Life -= damageAmount;
    }

    private IEnumerator AttackCo()
    {
        _animator.SetTrigger(Attack);
        _weapon.gameObject.SetActive(true);
        yield return new WaitUntil(() => !_animator.IsInTransition(0) && !_animator.GetBool(IsAttacking));
        _weapon.gameObject.SetActive(false);
        OnAttackComplete?.Invoke();
    }

    private bool CheckForEnemy()
    {
        if (!IsInSight(out var target) || _fsm.Current == _attackState) return false;
        
        if (_attackState.IsAttackOnCooldown) return false;
        
        OnTargetSpotted?.Invoke(target);
        return true;
    }

    private void CheckDead(float healthAmount, float maxHealth)
    {
        if (healthAmount > 0) return;
        StartCoroutine(DieCo());
    }

    private IEnumerator DieCo()
    {
        _animator.SetTrigger(Death);
        yield return new WaitForSeconds(2.5f);
        OnUnitDown?.Invoke(this);
        Destroy(gameObject);
    }

    private bool IsInSight(out Transform target)
    {
        foreach (var possibleTarget in Physics.OverlapSphere(transform.position, _losRange, _entitiesMask))
        {
            target = possibleTarget.transform;
            var dir = target.position - transform.position;

            if (Vector3.Angle(transform.forward, dir) > _losAngle / 2f) continue;

            if (!Physics.Raycast(transform.position, dir.normalized, out var hitInfo, _losRange)) continue;

            if (!hitInfo.transform.TryGetComponent(out IUnit other)) continue;

            if (other.TeamID != TeamID) return true;
        }

        target = null;
        return false;
    }

    #region TestMethods

    [ContextMenu("DecreaseLife")]
    private void DecreaseLife()
    {
        Life -= 25;
    }

    #endregion
}