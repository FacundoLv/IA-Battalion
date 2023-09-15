using System.Linq;
using Core;
using UnityEngine;

internal class FleeState : BaseState
{
    private readonly IUnit _unit;
    private HealthSpot _healthSpot;

    public FleeState(IUnit unit)
    {
        _unit = unit;
    }

    public override void Awake()
    {
        _healthSpot = FindClosestHealthSpot();
    }

    public override void Execute()
    {
        var dir = _healthSpot.transform.position - _unit.GameObject.transform.position;
        _unit.Move(dir.normalized);
    }

    public override void Sleep()
    {
        _unit.Stop();
    }

    private HealthSpot FindClosestHealthSpot()
    {
        return GameObject.FindObjectsOfType<HealthSpot>()
            .Where(n => n.TeamID == _unit.TeamID)
            .OrderBy(n => (n.transform.position - _unit.GameObject.transform.position).sqrMagnitude)
            .FirstOrDefault();
    }
}