public interface IDamager
{
    float DamageAmount { get; }
    void DealDamage(IUnit unit);
}
