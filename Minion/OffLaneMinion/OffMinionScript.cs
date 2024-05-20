using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class OffMinionScripts : NetworkBehaviour, IDamageable
{
    private Animator _animator;
    [SerializeField] private GameObject minion_dead_prefab;
    public Transform spawnpointbullet;
    private NavMeshAgent navMeshAgent;
    private Transform currentTarget = null;

    public UnitData BigOffMinionData;
    public UnitData SmallOffMinionData;
    private PoolManager poolManager;
    private HealthBar _healthbar;

    private Vector3 spawnPosition;

    [Header("MinionData")]
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HP = new NetworkVariable<float>(0);
    public NetworkVariable<float> Defense = new NetworkVariable<float>(0);
    public NetworkVariable<float> MovementSpeed = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackDam = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackSpeedPerSec = new NetworkVariable<float>(0);
    public NetworkVariable<float> attackRange = new NetworkVariable<float>(0);
    public NetworkVariable<float> DetectRange = new NetworkVariable<float>(0);
    public NetworkVariable<int> MoneyValue = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXPValue = new NetworkVariable<int>(0);

    public NetworkVariable<float> GrowthMaxHealthPoint = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthHealthPointRegeneration = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthDefense = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthAttackDamage = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthEnergyPoint = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthEnergyPointRegeneration = new NetworkVariable<float>(0);
    public float attackTimer = 0;
    private string Type;
    public override void OnNetworkSpawn()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        poolManager = PoolManager.Instance;

        if (gameObject.name.Contains("Big"))
        {
            if (IsServer)
            {
                SetMinionData(BigOffMinionData);
            }
            Type = "BigOffLaneMinion";
        }
        else
        {
            if (IsServer)
            {
                SetMinionData(SmallOffMinionData);
            }
            Type = "SmallOffLaneMinion";
        }

        navMeshAgent.speed = MovementSpeed.Value;
        spawnPosition = transform.position;
        _healthbar = GetComponentInChildren<HealthBar>();
        _healthbar.SetMaxSlider(MaxHP.Value);
        _healthbar.UpdateSlider(HP.Value);
        UpdateLevelClientRpc();
    }

    private void SetMinionData(UnitData data)
    {
        GrowthMaxHealthPoint.Value = data.GrowthMaxHealthPoint;
        GrowthDefense.Value = data.GrowthDefense;
        GrowthAttackDamage.Value = data.GrowthAttackDamage;
        MaxHP.Value = data.MaxHealthPoint + (GrowthMaxHealthPoint.Value * (GameManager.Instance.MinionLevel.Value - 1));
        HP.Value = MaxHP.Value;
        Defense.Value = data.Defense + (GrowthDefense.Value * (GameManager.Instance.MinionLevel.Value - 1));
        MovementSpeed.Value = data.MovementSpeed;
        AttackDam.Value = data.AttackDamage + (GrowthAttackDamage.Value * (GameManager.Instance.MinionLevel.Value - 1));
        AttackSpeedPerSec.Value = data.AttackSpeedPerSec;
        attackRange.Value = data.attackRange;
        DetectRange.Value = data.DetectRange;
        MoneyValue.Value = data.MoneyValue;
        EXPValue.Value = data.EXPValue;
    }

    private void Update()
    {
        if (IsServer)
        {
            MinionServerRpc();

            if (GameManager.Instance.GameEnd.Value == true)
            {
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }

    }
    [ServerRpc]
    public void MinionServerRpc()
    {
        if (currentTarget == null)
        {
            HP.Value = MaxHP.Value;
            _healthbar.UpdateSlider(HP.Value);
            FindNearestTargetServerRpc("Player", DetectRange.Value);
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
            if (distanceToTarget <= DetectRange.Value && isOnBridge && !isTargetDead)
            {
                // If the target is within detect range, move towards the target until it's within attack range
                if (distanceToTarget >= attackRange.Value)
                {
                    navMeshAgent.SetDestination(currentTarget.position);
                    _animator.SetTrigger("Walk");
                    navMeshAgent.isStopped = false;
                }
                else
                {
                    navMeshAgent.isStopped = true;
                    if (attackTimer >= AttackSpeedPerSec.Value)
                    {
                        AttackTargetServerRpc(AttackDam.Value, this.NetworkObjectId);
                        attackTimer = 0;
                    }
                    attackTimer += Time.deltaTime;
                }
            }
            else
            {
                currentTarget = null;
                attackTimer = 0;
                navMeshAgent.SetDestination(spawnPosition);
                _animator.SetTrigger("Walk");
                navMeshAgent.isStopped = false;
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
            if (enemy.TryGetComponent(out PlayerStatusController player))
            {
                if (!player.OnBridge.Value)
                {
                    continue;
                }
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
            poolManager.RequestInLaneBulletFromPoolServerRpc("InLane", "Bullet", spawnpointbullet.position, spawnpointbullet.rotation * Quaternion.Euler(0f, -90f, 90f), currentTarget.GetComponent<NetworkObject>());
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
            GameManager.Instance.HandleKill(attackerID, MoneyValue.Value, EXPValue.Value, "OffMinion", this.gameObject.transform);
            HP.Value = MaxHP.Value;
            activeBuffs.Clear();
            activeDeBuffs.Clear();
            gameObject.SetActive(false);
            PushToPoolServerRpc();
        }
    }

    [ClientRpc]
    public void TakeDamClientRpc(float Damage)
    {
        _healthbar.UpdateSlider(HP.Value - Damage);
    }

    [ClientRpc]
    private void UpdateLevelClientRpc()
    {
        _healthbar.UpdateLevel(GameManager.Instance.MinionLevel.Value);
    }

    [ServerRpc]
    public void PushToPoolServerRpc()
    {
        poolManager.poolDictionary[gameObject.tag + "_" + Type].Enqueue(gameObject);
        gameObject.GetComponent<NetworkObject>().Despawn(false);
    }

    // Buff & Debuff variable part
    private List<Buff> activeBuffs = new List<Buff>();
    private bool isBuffActive = false;

    private List<Debuff> activeDeBuffs = new List<Debuff>();
    private bool isDeBuffActive = false;
    private class Buff
    {
        public string Identifier;
        public float Duration;
        public float DamageMultiplier;
        public float SpeedMultiplier;
        public float AttackSpeedMultiplier;
    }
    private class Debuff
    {
        public string Identifier;
        public float Duration;
        public float DamageReduction;
        public float SpeedReduction;
        public float AttackSpeedReduction;
    }

    private float originalAttackDamage;
    private float originalMovementSpeed;
    private float originalAttackSpeed;

    // Buff Part
    public void ApplyBuff(float damageMultiplier, float speedMultiplier, float attackSpeedMultiplier, float duration, string Identifier)
    {
        var newBuff = new Buff()
        {
            Identifier = Identifier,
            Duration = duration,
            DamageMultiplier = damageMultiplier,
            SpeedMultiplier = speedMultiplier,
            AttackSpeedMultiplier = attackSpeedMultiplier
        };

        activeBuffs.Add(newBuff);

        if (!isBuffActive && !isDeBuffActive)
        {
            originalAttackDamage = AttackDam.Value;
            originalMovementSpeed = MovementSpeed.Value;
            originalAttackSpeed = AttackSpeedPerSec.Value;
        }

        if (!isBuffActive)
        {
            isBuffActive = true;
            StartCoroutine(BuffCoroutine());
        }

        UpdateStats();
    }
    private IEnumerator BuffCoroutine()
    {
        while (activeBuffs.Count > 0)
        {
            // Wait for the shortest duration buff to expire
            float shortestDuration = activeBuffs.Min(buff => buff.Duration);
            yield return new WaitForSeconds(shortestDuration);

            // Reduce duration and remove expired buffs
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                activeBuffs[i].Duration -= shortestDuration;
                if (activeBuffs[i].Duration <= 0)
                {
                    activeBuffs.RemoveAt(i);
                }
            }

            UpdateStats(); // Update stats after removing expired buffs
        }

        isBuffActive = false;

        if (!isDeBuffActive)
        {
            RestoreOriginalStats();
        }
    }

    // De-Buff Part
    public void ApplyDebuff(float damageReduction, float speedReduction, float attackSpeedReduction, float duration, string Identifier)
    {
        var newDebuff = new Debuff()
        {
            Identifier = Identifier,
            Duration = duration,
            DamageReduction = damageReduction,
            SpeedReduction = speedReduction,
            AttackSpeedReduction = attackSpeedReduction
        };

        activeDeBuffs.Add(newDebuff);

        if (!isBuffActive && !isDeBuffActive)
        {
            originalAttackDamage = AttackDam.Value;
            originalMovementSpeed = MovementSpeed.Value;
            originalAttackSpeed = AttackSpeedPerSec.Value;
        }

        if (!isDeBuffActive)
        {
            isDeBuffActive = true;
            StartCoroutine(DebuffCoroutine());
        }

        UpdateStats();
    }
    private IEnumerator DebuffCoroutine()
    {
        while (activeDeBuffs.Count > 0)
        {
            float shortestDuration = activeDeBuffs.Min(debuff => debuff.Duration);
            yield return new WaitForSeconds(shortestDuration);

            activeDeBuffs.RemoveAll(debuff => (debuff.Duration -= shortestDuration) <= 0);

            UpdateStats();
        }

        isDeBuffActive = false;

        if (!isBuffActive)
        {
            RestoreOriginalStats();
        }
    }

    // Calculate Buff & DeBuff Part
    private void UpdateStats()
    {
        if (IsServer)
        {
            // Initialize net change percentages
            float netAttackDamageChange = 0f;
            float netMovementSpeedChange = 0f;
            float netAttackSpeedChange = 0f;

            // Calculate debuff effects as negative changes
            foreach (var debuff in activeDeBuffs)
            {
                netAttackDamageChange -= debuff.DamageReduction;
                netMovementSpeedChange -= debuff.SpeedReduction;
                netAttackSpeedChange += debuff.AttackSpeedReduction;
            }

            // Calculate buff effects as positive changes
            foreach (var buff in activeBuffs)
            {
                netAttackDamageChange += buff.DamageMultiplier;
                netMovementSpeedChange += buff.SpeedMultiplier;
                netAttackSpeedChange -= buff.AttackSpeedMultiplier;
            }

            // Apply the net changes to the original stats
            AttackDam.Value = ApplyStatChange(originalAttackDamage, netAttackDamageChange);
            MovementSpeed.Value = ApplyStatChange(originalMovementSpeed, netMovementSpeedChange);
            AttackSpeedPerSec.Value = ApplyStatChange(originalAttackSpeed, netAttackSpeedChange);
        }
    }
    private float ApplyStatChange(float originalValue, float netChange)
    {
        if (netChange > 0)
        {
            return originalValue * netChange;
        }
        else
        {
            return Math.Max(0, originalValue * (1 + netChange));
        }
    }
    private void RestoreOriginalStats()
    {
        AttackDam.Value = originalAttackDamage;
        MovementSpeed.Value = originalMovementSpeed;
        AttackSpeedPerSec.Value = originalAttackSpeed;
    }
    public void RemoveSelectedDebuff(string identifier)
    {
        for (int i = activeDeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeDeBuffs[i].Identifier == identifier)
            {
                activeDeBuffs.RemoveAt(i);
                break;
            }
        }
        UpdateStats();
    }
    public void RemoveSelectedBuff(string identifier)
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (activeBuffs[i].Identifier == identifier)
            {
                activeBuffs.RemoveAt(i);
                break;
            }
        }
        UpdateStats();
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