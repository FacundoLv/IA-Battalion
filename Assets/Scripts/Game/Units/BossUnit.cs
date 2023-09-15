using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Utils;
using UnityEngine;

public class BossUnit : MonoBehaviour, IUnit
{
    public event Action<Transform> OnTargetSpotted;
    public event Action<IUnit> OnUnitDown;
    public event Action<float, float> OnHealthChanged;
    public event Action OnAttackComplete;
    public event Action OnPoweredUp;
    public event Action<int, int> OnMinionCountUpdated;

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
    [field: SerializeField] public int TeamID { get; set; }

    public int CurrentMinionCount
    {
        get => _currentMinionCount;
        set
        {
            _currentMinionCount = value;
            OnMinionCountUpdated?.Invoke(_currentMinionCount, _maxMinions);
        }
    }

    private float _life;
    private bool _canBeDamaged;
    [SerializeField] private float _maxLife;

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
    
    private float _lastPowerUpTime;
    private float _powerUpDuration;
    [SerializeField] private float _powerUpCoolDown = 10f;

    private Rigidbody _rb;
    private Animator _animator;
    private Weapon _weapon;

    private List<GroundUnit> _minions = new List<GroundUnit>();

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int PowerUp = Animator.StringToHash("PowerUp");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
    private static readonly int Death = Animator.StringToHash("Death");

    private SeekState _seekState;
    private AttackState _attackState;
    private PowerUpState _powerUpState;
    private FSM _fsm;

    private QuestionNode _canPowerUp;
    private ActionNode _powerUp;
    private QuestionNode _isEnemyInSight;
    private ActionNode _seekEnemies;
    private ActionNode _attackEnemy;
    private int _maxMinions;
    private int _currentMinionCount;

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
        _seekState = new SeekState(this);
        _attackState = new AttackState(this, _attackDistance, _attackCooldown);
        _powerUpState = new PowerUpState(this);

        _seekState.AddTransition(_attackState);
        _seekState.AddTransition(_powerUpState);
        
        _attackState.AddTransition(_seekState);
        _attackState.AddTransition(_powerUpState);

        _powerUpState.AddTransition(_seekState);
        _powerUpState.AddTransition(_attackState);
        
        _fsm = new FSM();

        // Set desition tree
        _attackEnemy = new ActionNode(() => _fsm.Transition(_attackState));
        _seekEnemies = new ActionNode(() => _fsm.Transition(_seekState));
        _isEnemyInSight = new QuestionNode(CheckForEnemy, _attackEnemy, _seekEnemies);
        _powerUp = new ActionNode(() => _fsm.Transition(_powerUpState));
        _canPowerUp = new QuestionNode(CanPowerUp, _powerUp, _isEnemyInSight);

        // Subscribe to events
        OnHealthChanged += EvaluateActions;
        OnHealthChanged += CheckDead;
        OnPoweredUp += EvaluateActions;
        OnAttackComplete += EvaluateActions;
        _attackState.OnTargetGone += EvaluateActions;
        OnMinionCountUpdated += CheckIfMortal;
    }

    private void Update()
    {
        if (CheckForEnemy()) EvaluateActions();
    }

    private void LateUpdate()
    {
        _fsm.OnUpdate();
    }

    public void Init()
    {
        _fsm.SetInit(_seekState);
    }

    public void AddMinion(GroundUnit minion)
    {
        _minions.Add(minion); 
        minion.OnUnitDown += OnMinionDown;
        _maxMinions = _minions.Count;
        CurrentMinionCount = _maxMinions;
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

        if (!_canBeDamaged) return;
        
        Life -= damageAmount;
    }

    public void BoostDamage(BoostData boostData)
    {
        StartCoroutine(BoostDamageCo(boostData));
    }

    private IEnumerator BoostDamageCo(BoostData boostData)
    {
        _weapon.BoostDamage(this, boostData);
        _lastPowerUpTime = Time.time;
        _powerUpDuration = boostData.Duration;
        _animator.SetTrigger(PowerUp);
        yield return new WaitForSeconds(3f);
        OnPoweredUp?.Invoke();
    }

    private IEnumerator AttackCo()
    {
        _animator.SetTrigger(Attack);
        _weapon.gameObject.SetActive(true);
        yield return new WaitUntil(() => !_animator.IsInTransition(0) && !_animator.GetBool(IsAttacking));
        _weapon.gameObject.SetActive(false);
        OnAttackComplete?.Invoke();
    }

    private void EvaluateActions()
    {
        _canPowerUp.Execute();
    }

    private void EvaluateActions(float healthAmount, float maxHealth)
    {
        if (healthAmount <= 0) return;
        EvaluateActions();
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
        yield return new WaitForSeconds(4.15f);
        OnUnitDown?.Invoke(this);
        Destroy(gameObject);
    }

    private bool CanPowerUp() => _minions.Count <= 0 && Time.time - _lastPowerUpTime  > _powerUpDuration + _powerUpCoolDown;

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

    private void CheckIfMortal(int minionCount, int maxMinions)
    {
        if (minionCount <= 0) _canBeDamaged = true;
    }

    private void OnMinionDown(IUnit minion)
    {
        _minions.Remove(minion as GroundUnit);
        CurrentMinionCount = _minions.Count;
    }
}
