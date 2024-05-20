using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BottomBoss : NetworkBehaviour, IDamageable
{
    private Animator _animator;
    [SerializeField] private GameObject minion_dead_prefab;
    private Transform currentTarget = null;

    public UnitData BossBottom;
    private PoolManager poolManager;

    [Header("MinionData")]
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HP = new NetworkVariable<float>(0);
    public NetworkVariable<float> Defense = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackDam = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackSpeedPerSec = new NetworkVariable<float>(0);
    public NetworkVariable<float> attackRange = new NetworkVariable<float>(0);
    public NetworkVariable<int> MoneyValue = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXPValue = new NetworkVariable<int>(0);

    public NetworkVariable<float> GrowthMaxHealthPoint = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthDefense = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthAttackDamage = new NetworkVariable<float>(0);
    public float attackTimer = 0;

    private HealthBar _healthbar;
    public override void OnNetworkSpawn()
    {
        poolManager = PoolManager.Instance;
        _animator = GetComponentInChildren<Animator>();
        SetBossData(BossBottom);
        _healthbar = GetComponentInChildren<HealthBar>();
        _healthbar.SetMaxSlider(MaxHP.Value);
        _healthbar.UpdateSlider(HP.Value);
    }

    private void SetBossData(UnitData bossData)
    {
        MaxHP.Value = bossData.MaxHealthPoint;
        HP.Value = MaxHP.Value;
        Defense.Value = bossData.Defense;
        AttackDam.Value = bossData.AttackDamage;
        AttackSpeedPerSec.Value = bossData.AttackSpeedPerSec;
        attackRange.Value = bossData.attackRange;
        MoneyValue.Value = bossData.MoneyValue;
        EXPValue.Value = bossData.EXPValue;
        GrowthMaxHealthPoint.Value = bossData.GrowthMaxHealthPoint;
        GrowthDefense.Value = bossData.GrowthDefense;
        GrowthAttackDamage.Value = bossData.GrowthAttackDamage;
    }

    private void Update()
    {
        if (IsServer)
        {
            BottomBossServerRpc();

            if (GameManager.Instance.GameEnd.Value == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }


    }
    [ServerRpc]
    public void BottomBossServerRpc()
    {
        if (currentTarget == null)
        {
            FindNearestTargetServerRpc("Player", attackRange.Value);
        }
        else
        {
            Defense.Value = 0f;

            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);
            if (distanceToTarget <= attackRange.Value && currentTarget.gameObject.activeSelf == true)
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
                currentTarget = null;
                attackTimer = 1f;
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
    public void FindNearestTargetServerRpc(string Team, float DetectRange)
    {
        Queue<GameObject> enemies = poolManager.poolPlayer[Team];

        float minDistanceSquared = DetectRange * DetectRange; // Square of DetectRange

        foreach (GameObject enemy in enemies)
        {
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
            _animator.SetTrigger("Attack");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamServerRpc(float Damage, ulong attackerID, string attackerName)
    {
        if (IsOwner)
        {
            // Get attack log
            GameLogger.Instance.LogActionServerRpc(attackerName, " Attack ", this.gameObject.name + " : " + Damage + "Damage");
        }
        TakeDamClientRpc(Damage);
        HP.Value = Mathf.Max(0, HP.Value - Damage);
        if (HP.Value <= 0)
        {
            GameObject DeadPrefab = Instantiate(minion_dead_prefab, this.gameObject.transform.position, this.gameObject.transform.rotation);
            DeadPrefab.GetComponent<NetworkObject>().Spawn();
            GameManager.Instance.HandleKill(attackerID, MoneyValue.Value, EXPValue.Value, "Boss", this.gameObject.transform);
            GameManager.Instance.GiveBossBotBuffTeam(attackerID);
            HP.Value = MaxHP.Value;
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
        poolManager.poolDictionary[gameObject.tag + "_" + "BossBottom"].Enqueue(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(false);
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