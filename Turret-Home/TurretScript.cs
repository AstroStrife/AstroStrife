using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurretScript : NetworkBehaviour, IDamageable
{
    private Transform currentTarget;
    private string TeamEnemy;

    private PoolManager poolManager;

    [Header("MinionData")]
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HP = new NetworkVariable<float>(0);
    public NetworkVariable<float> Defense = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackDam = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackSpeedPerSec = new NetworkVariable<float>(0);
    public NetworkVariable<float> attackRange = new NetworkVariable<float>(0);
    public NetworkVariable<int> MoneyValue = new NetworkVariable<int>(0);
    private float attackTimer = 1f;

    private HealthBar _healthbar;

    public override void OnNetworkSpawn()
    {
        poolManager = PoolManager.Instance;
        MaxHP.Value = 5000f;
        HP.Value = MaxHP.Value;
        Defense.Value = 50f;
        AttackDam.Value = 300;
        AttackSpeedPerSec.Value = 2f;
        attackRange.Value = 100f;
        MoneyValue.Value = 1000;

        TeamEnemy = gameObject.CompareTag("Top") ? "Bottom" : "Top";

        AssignToPoolServerRpc();
        _healthbar = GetComponentInChildren<HealthBar>();
        _healthbar.SetMaxSlider(MaxHP.Value);
        _healthbar.UpdateSlider(HP.Value);
    }

    private void Update()
    {
        if (IsServer)
        {
            TurretServerRpc();

            if (GameManager.Instance.GameEnd.Value == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }

    }
    [ServerRpc]
    public void TurretServerRpc()
    {
        if (currentTarget == null)
        {
            FindNearestTargetServerRpc(TeamEnemy, attackRange.Value);
        }
        else
        {
            if (currentTarget.GetComponent<PlayerStatusController>() == null)
            {
                Defense.Value = 0f;

                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
                if (distanceToTarget <= attackRange.Value && currentTarget.gameObject.activeSelf == true)
                {
                    if (attackTimer >= AttackSpeedPerSec.Value)
                    {
                        // If the target is already within attack range, attack it
                        AttackTargetServerRpc(AttackDam.Value, 0);
                        attackTimer = 0;
                    }
                    attackTimer += Time.deltaTime;
                }
                else
                {
                    Defense.Value = 50f;
                    currentTarget = null;
                    attackTimer = 1f;
                }
            }
            else
            {
                float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

                bool isOnBridge = false;
                bool isTargetDead = false;
                if (currentTarget.TryGetComponent(out PlayerStatusController player))
                {
                    isOnBridge = player.OnBridge.Value;
                    isTargetDead = player.isDead.Value;
                }
                if (distanceToTarget <= attackRange.Value && isOnBridge && !isTargetDead)
                {
                    if (attackTimer >= AttackSpeedPerSec.Value)
                    {
                        // If the target is already within attack range, attack it
                        AttackTargetServerRpc(AttackDam.Value, this.NetworkObjectId);
                        attackTimer = 0;
                    }
                    attackTimer += Time.deltaTime;
                }
                else
                {
                    Defense.Value = 50f;
                    currentTarget = null;
                    attackTimer = 1f;
                }
            }
        }
    }


    /// <summary>
    /// Find nearest enemy and return it
    /// </summary>
    /// <param name="Team"> Which team to find </param>
    /// <param name="CallingTransform"> Call self tranform </param>
    /// <param name="DetectRange"> Range that minion will start to follow </param>
    /// <param name="currentTarget"> aimed target </param>
    /// <returns> nearestTarget </returns>
    /// <returns> nearestTarget </returns>
    [ServerRpc]
    public void FindNearestTargetServerRpc(string Team, float AttackRange)
    {
        Queue<GameObject> enemies = poolManager.poolTeam[Team];

        float minDistanceSquared = AttackRange * AttackRange; // Square of DetectRange

        foreach (GameObject enemy in enemies)
        {
            // Check if the object's name contains "bullet" or not on bridge and skip it if it does
            if (enemy.name.ToLower().Contains("bullet"))
            {
                continue;
            }

            if (enemy.activeSelf != true)
            {
                continue;
            }

            float distanceToCreepSquared = (transform.position - enemy.transform.position).sqrMagnitude;
            if (distanceToCreepSquared <= minDistanceSquared && enemy.gameObject.activeSelf)
            {
                currentTarget = enemy.transform;
                minDistanceSquared = distanceToCreepSquared;
                // Exit the loop as soon as a first target is found
                break;
            }
        }
    }

    [ServerRpc]
    public void AttackTargetServerRpc(float Damage, ulong attackerID)
    {
        if (currentTarget.GetComponent<NetworkObject>() != null && currentTarget.GetComponent<NetworkObject>().IsSpawned)
        {
            poolManager.RequestInLaneBulletFromPoolServerRpc("InLane", "Bullet", transform.position, transform.rotation * Quaternion.Euler(0f, -90f, 90f), currentTarget.GetComponent<NetworkObject>());
            IDamageable damageable = currentTarget.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float effectiveDamage = Mathf.Max(0, Damage - damageable.GetDefense());
                damageable.TakeDamage(effectiveDamage, attackerID, this.gameObject.name);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamServerRpc(float Damage, ulong attackerID, string attackerName)
    {
        List<GameObject> turretList = gameObject.CompareTag("Top") ? TurretSpawn.TopTurrets : TurretSpawn.BottomTurrets;
        int turretIndex = turretList.IndexOf(gameObject);

        // Check if this is not the first turret AND the previous turret is still active.
        if (turretIndex > 0 && turretList[turretIndex - 1].activeSelf)
        {
            return; // Don't take damage if previous turret is still active.
        }
        else
        {
            if (IsOwner)
            {
                // Get attack log
                GameLogger.Instance.LogActionServerRpc(attackerName, " Attack ", this.gameObject.name + " : " + Damage + "Damage");
            }
            TakeDamClientRpc(Damage);
            HP.Value = Mathf.Max(0, HP.Value - Damage);
        }

        if (HP.Value <= 0)
        {
            GameManager.Instance.HandleKill(attackerID, MoneyValue.Value, 50, "", this.gameObject.transform);
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
    }

    [ClientRpc]
    public void TakeDamClientRpc(float Damage)
    {
        _healthbar.UpdateSlider(HP.Value - Damage);
    }

    [ServerRpc]
    public void PushToPoolServerRpc()
    {
        gameObject.GetComponent<NetworkObject>().Despawn(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignToPoolServerRpc()
    {
        if (!poolManager.poolTeam.ContainsKey(this.gameObject.tag))
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            objectPool.Enqueue(this.gameObject);
            poolManager.poolTeam.Add(this.gameObject.tag, objectPool);
        }
        else
        {
            poolManager.poolTeam[this.gameObject.tag].Enqueue(this.gameObject);
        }
    }


    // IDamageable Part
    public void TakeDamage(float amount, ulong attackerID, string attackerName)
    {
        TakeDamServerRpc(amount, attackerID, attackerName);
    }

    public float GetDefense()
    {
        return Defense.Value;
    }
}