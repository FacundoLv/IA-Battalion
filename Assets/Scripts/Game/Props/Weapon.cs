using UnityEngine;

public class Weapon : MonoBehaviour, IDamager
{
    public float DamageAmount => _damageAmount * DamageModifier;

    [SerializeField] private float _damageAmount;

    private float DamageModifier
    {
        get => _boostEndTime - Time.time > 0 ? _damageModifier : 1;
        set => _damageModifier = value;
    }

    private float _damageModifier = 1f;
    private float _boostEndTime;

    private IUnit _owner;
    private float _lastHitTime;
    private const float HitCooldown = .3f;

    private void Awake()
    {
        _owner = GetComponentInParent<IUnit>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out IUnit unit)) return;

        if (unit.TeamID == _owner.TeamID) return;

        if (Time.time - _lastHitTime < HitCooldown) return;

        _lastHitTime = Time.time;
        DealDamage(unit);
    }

    public void DealDamage(IUnit unit)
    {
        unit.GetDamaged(this, DamageAmount);
    }

    public void BoostDamage(IUnit source, BoostData boostData)
    {
        if (source != _owner) return;
        DamageModifier = boostData.Modifier;
        _boostEndTime = Time.time + boostData.Duration;
    }
}
