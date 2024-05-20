public interface IDamageable
{
    void TakeDamage(float amount, ulong attackerID, string attackerName);
    float GetDefense();
}