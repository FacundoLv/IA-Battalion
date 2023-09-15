using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;

public class SeekState : BaseState
{
    private readonly IUnit _unit;
    private List<GroundUnit> _enemyFlock;
    private BossUnit _enemyBoss;

    public SeekState(IUnit unit)
    {
        _unit = unit;
    }

    public override void Awake()
    {
        _enemyFlock = GetEnemyFlock();
        foreach (var enemy in _enemyFlock) 
            enemy.OnUnitDown += RemoveFromTargets;
        _enemyBoss ??= GetEnemyBoss();
    }

    public override void Execute()
    {
        var target = GetTarget();
        var direction = target - _unit.GameObject.transform.position;
        _unit.Move(direction.normalized);
    }

    public override void Sleep()
    {
        foreach (var enemy in _enemyFlock) 
            enemy.OnUnitDown -= RemoveFromTargets;
        _enemyFlock = null;

        _unit.Stop();
    }

    private Vector3 GetTarget()
    {
        if (_enemyFlock.Count <= 0 && _enemyBoss) return _enemyBoss.transform.position;

        var target = Vector3.zero;
        foreach (var enemy in _enemyFlock)
            target += enemy.transform.position;
        target /= _enemyFlock.Count;
        return target;
    }

    private List<GroundUnit> GetEnemyFlock()
    {
        return GameObject
            .FindObjectsOfType<GroundUnit>()
            .Where(n => n.TeamID != _unit.TeamID)
            .ToList();
    }

    private void RemoveFromTargets(IUnit unit)
    {
        _enemyFlock.Remove(unit as GroundUnit);
        unit.OnUnitDown -= RemoveFromTargets;
    }

    private BossUnit GetEnemyBoss()
    {
        return GameObject
            .FindObjectsOfType<BossUnit>()
            .FirstOrDefault(n => n.TeamID != _unit.TeamID);
    }
}
