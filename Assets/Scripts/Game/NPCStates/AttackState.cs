using System;
using Core;
using UnityEngine;

internal class AttackState : BaseState
{
    public event Action OnTargetGone;
    
    private Transform _target;
    private bool _isAttacking;
    private float _lastAttack = Time.time;

    private readonly IUnit _unit;
    private readonly float _attackDistance;
    private readonly float _attackCooldown;

    public bool IsAttackOnCooldown => Time.time - _lastAttack < _attackCooldown;

    public AttackState(IUnit unit, float attackDistance, float attackCooldown)
    {
        unit.OnTargetSpotted += AssignTarget;
        _unit = unit;
        _attackDistance = attackDistance;
        _attackCooldown = attackCooldown;
    }

    public override void Awake()
    {
    }

    public override void Execute()
    {
        if (_isAttacking) return;

        if (_target == null)
        {
            OnTargetGone?.Invoke();
            return;
        }

        var targetDir = _target.position - _unit.GameObject.transform.position;
        if (targetDir.sqrMagnitude > _attackDistance * _attackDistance)
        {
            _unit.Move(targetDir.normalized);
            return;
        }

        _isAttacking = true;
        _unit.Stop();
        _unit.DoAttack();
    }

    public override void Sleep()
    {
        _isAttacking = false;
        _lastAttack = Time.time;
    }

    private void AssignTarget(Transform target) => _target = target;
}