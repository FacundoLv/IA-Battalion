using UnityEngine;

public class HealthSpot : MonoBehaviour, IHealth
{
    [field: SerializeField] public int TeamID { get; private set; }
    [field: SerializeField] public int HealAmount { get; private set; }

    private void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent(out IUnit unit)) return;

        if (unit.TeamID != TeamID) return;

        unit.HealUp(this, HealAmount * Time.deltaTime);
    }
}