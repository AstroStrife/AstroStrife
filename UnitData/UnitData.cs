using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Data", menuName = "Unit Data")]
public class UnitData : ScriptableObject
{
    public float MaxHealthPoint;
    public float HealthPointRegeneration;
    public float Defense;
    public float MovementSpeed;
    public float AttackDamage;
    public float AttackSpeedPerSec;
    public float CooldownHaste;
    public float MaxEnergyPoint;
    public float EnergyPointRegeneration;

    public float attackRange;
    public float DetectRange;

    public int MoneyValue;
    public int EXPValue;

    public float GrowthMaxHealthPoint;
    public float GrowthHealthPointRegeneration;
    public float GrowthDefense;
    public float GrowthAttackDamage;
    public float GrowthEnergyPoint;
    public float GrowthEnergyPointRegeneration;
}
