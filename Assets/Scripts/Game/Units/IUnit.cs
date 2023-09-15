using System;
using UnityEngine;

public interface IUnit
{
    event Action<Transform> OnTargetSpotted;
    event Action<IUnit> OnUnitDown;
    event Action<float, float> OnHealthChanged;
    float Life { get; }
    GameObject GameObject { get; }
    int TeamID { get; set; }
    void Move(Vector3 direction);
    void Stop();
    void DoAttack();
    void HealUp(IHealth healthSource, float healAmount);
    void GetDamaged(IDamager damager, float damageAmount);
}