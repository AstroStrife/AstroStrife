using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New AbilityShipData", menuName = "Ability System/Ultimate/Menhit")]
public class DeployNegativeWave : Ability
{
    public float DamageMultiplier;
    public float detectionRadius;
    public override void Activate(GameObject user, int SkillLevel)
    {
        GameObject WaveSignal = Instantiate(prefab, new Vector3(user.transform.position.x, prefab.transform.position.y, user.transform.position.z), user.transform.rotation);
        WaveSignal.GetComponent<NetworkObject>().Spawn();

        Collider[] colliders = Physics.OverlapSphere(user.transform.position, detectionRadius);

        foreach (Collider collider in colliders)
        {
            PlayerStatusController playerStatusController = collider.GetComponent<PlayerStatusController>();
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (playerStatusController != null && collider.tag != user.tag)
            {
                if (damageable != null)
                {
                    if (collider.GetComponentInChildren<BlinkEffect>() != null)
                    {
                        collider.GetComponentInChildren<BlinkEffect>().Blink();
                    }
                    float effectiveDamage = Mathf.Max(0, playerStatusController.EP.Value * (DamageMultiplier + SkillLevel));
                    damageable.TakeDamage(effectiveDamage, user.GetComponent<NetworkObject>().NetworkObjectId, user.name);

                    user.GetComponent<PlayerScore>().IncreaseTotalPlayerDamage(effectiveDamage);

                    playerStatusController.EP.Value = 0;
                }
            }
        }
    }
}
