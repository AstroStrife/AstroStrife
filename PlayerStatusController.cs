using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatusController : NetworkBehaviour, IDamageable
{
    [Header("UnitData")]
    public UnitData ShipData;

    [Header("PlayerData")]
    public NetworkVariable<PlayerData> PlayerData = new NetworkVariable<PlayerData>();
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HPRegen = new NetworkVariable<float>(0);
    public NetworkVariable<float> Defense = new NetworkVariable<float>(0);
    public NetworkVariable<float> MovementSpeed = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackDam = new NetworkVariable<float>(0);
    public NetworkVariable<float> AttackSpeedPerSec = new NetworkVariable<float>(0);
    public NetworkVariable<float> CooldownHaste = new NetworkVariable<float>(0);

    public NetworkVariable<float> MaxEP = new NetworkVariable<float>(0);
    public NetworkVariable<float> EP = new NetworkVariable<float>(0);
    public NetworkVariable<float> EPRegen = new NetworkVariable<float>(0);
    public NetworkVariable<bool> OnBridge = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> OnCheat = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> OnBase = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> topPlayerTag = new NetworkVariable<bool>(false);

    public NetworkVariable<int> MoneyValue = new NetworkVariable<int>(0);
    public NetworkVariable<int> EXPValue = new NetworkVariable<int>(0);

    public NetworkVariable<int> Money = new NetworkVariable<int>(0);

    public NetworkVariable<float> GrowthMaxHealthPoint = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthHealthPointRegeneration = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthDefense = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthAttackDamage = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthEnergyPoint = new NetworkVariable<float>(0);
    public NetworkVariable<float> GrowthEnergyPointRegeneration = new NetworkVariable<float>(0);

    // Upgrade Stat
    public NetworkVariable<int> StatUpgradable = new NetworkVariable<int>(0);
    public float attackTimer = 0;

    private PoolManager poolManager;
    private GameManager gameManager;
    private Animator _animator;
    private GameStateManager gameStateManager;
    public BushScript currentBush;
    private InputManager _inputManager;
    private CharacterController _characterController;
    private PlayerScore _playerScore;
    private HealthBar _healthbar;

    public Transform respawnPoint;
    private Renderer[] _renderers;

    private float UpdateTimer = 5f;
    private float timeSinceLastUpdate = 0f;

    private float hpRegenTimer = 0f;
    private float epRegenTimer = 0f;


    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            MaxHP.Value = ShipData.MaxHealthPoint;
            HP.Value = MaxHP.Value;
            HPRegen.Value = ShipData.HealthPointRegeneration;
            Defense.Value = ShipData.Defense;
            MovementSpeed.Value = ShipData.MovementSpeed;
            AttackDam.Value = ShipData.AttackDamage;
            AttackSpeedPerSec.Value = ShipData.AttackSpeedPerSec;
            CooldownHaste.Value = ShipData.CooldownHaste;

            MaxEP.Value = ShipData.MaxEnergyPoint;
            EP.Value = MaxEP.Value;
            EPRegen.Value = ShipData.EnergyPointRegeneration;
            MoneyValue.Value = ShipData.MoneyValue;
            EXPValue.Value = ShipData.EXPValue;
            GrowthMaxHealthPoint.Value = ShipData.GrowthMaxHealthPoint;
            GrowthHealthPointRegeneration.Value = ShipData.GrowthHealthPointRegeneration;
            GrowthDefense.Value = ShipData.GrowthDefense;
            GrowthAttackDamage.Value = ShipData.GrowthAttackDamage;
            GrowthEnergyPoint.Value = ShipData.GrowthEnergyPoint;
            GrowthEnergyPointRegeneration.Value = ShipData.GrowthEnergyPointRegeneration;
            StatUpgradable.Value = 100;
            _inputManager = GetComponent<InputManager>();
        }
        poolManager = GameObject.Find("PoolManager").GetComponent<PoolManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        gameStateManager = GameObject.Find("GameManager").GetComponent<GameStateManager>();
        _animator = GetComponentInChildren<Animator>();
        _characterController = GetComponent<CharacterController>();
        _playerScore = GetComponent<PlayerScore>();
        _healthbar = GetComponentInChildren<HealthBar>();
        _renderers = GetComponentsInChildren<Renderer>();
        _healthbar.SetMaxSlider(MaxHP.Value);
        _healthbar.UpdateSlider(HP.Value);
    }

    public void Start()
    {
        poolManager = PoolManager.Instance;

        if (topPlayerTag.Value == true)
        {
            gameObject.tag = "Top";
        }
        else
        {
            gameObject.tag = "Bottom";
        }

        StartCoroutine(ReCheck());
        _characterController.enabled = true;
    }

    public IEnumerator ReCheck()
    {
        yield return new WaitForSeconds(2);

        if (gameManager.spawnPoint[this.PlayerData.Value.noOfSpawnPoint].transform.position != this.gameObject.transform.position)
        {
            this.gameObject.transform.position = gameManager.spawnPoint[this.PlayerData.Value.noOfSpawnPoint].transform.position;
        }
        if (PlayerData.Value.playerTeam.ToString() != this.gameObject.tag)
        {
            this.gameObject.tag = PlayerData.Value.playerTeam.ToString();
        }
        if (gameObject.tag == "Top")
        {
            respawnPoint = GameObject.Find("TopSpawnPoint").transform;

        }
        else
        {
            respawnPoint = GameObject.Find("BottomSpawnPoint").transform;
        }
        if (IsServer)
        {
            AssignToPoolServerRpc();
        }
        yield return new WaitForSeconds(1);
        if (IsOwner)
        {
            MiniMapMark.localPlayerTag = gameObject.tag;
            MiniMapMark.RecheckFinish = true;
            MiniMapMarkHill.localPlayerTag = gameManager.tag;
        }

        this.gameObject.GetComponent<AbilityController>().SetDriverData(this.PlayerData.Value.playerDriver.ToString());
        gameStateManager.AddPlayerReady();
    }

    public void SetPlayerData(PlayerData _playerData)
    {
        PlayerData.Value = _playerData;
        _playerScore.SetUsername(_playerData.playerName.ToString());
        _playerScore.SetEmail(_playerData.playerEmail.ToString());
        _playerScore.SetTeam(_playerData.playerTeam.ToString());
        _playerScore.SetUserDriver(_playerData.playerDriver.ToString());
        _playerScore.SetUserShip(_playerData.playerShip.ToString());
    }

    public void SetTeam(bool _topteam)
    {
        topPlayerTag.Value = _topteam;
    }

    public void Update()
    {
        if (IsServer)
        {
            HPandEP_Regen();
            HandleHealAtBase();
        }
        if (!IsOwner) return;
        HandleCheat();
    }

    public void HPandEP_Regen()
    {
        if (isDead.Value != true)
        {
            hpRegenTimer += Time.deltaTime;
            if (hpRegenTimer >= 1f)
            {
                HealServerRpc(HPRegen.Value);
                hpRegenTimer = 0f;
            }

            epRegenTimer += Time.deltaTime;
            if (epRegenTimer >= 1f)
            {
                RegenEPServerRpc(EPRegen.Value);
                epRegenTimer = 0f;
            }
        }
    }

    void HandleCheat()
    {
        if (_inputManager.CheatInput)
        {
            Debug.Log("cheat");
            _inputManager.CheatInput = false;
            CheatToggleServerRpc();
        }
    }

    public void HandleHeal(float amount)
    {
        if (IsServer)
        {
            HealServerRpc(amount);
        }
    }

    // Heal and regenerate energy point
    [ServerRpc(RequireOwnership = false)]
    public void HealServerRpc(float amount)
    {
        HP.Value = Mathf.Min(HP.Value + amount, MaxHP.Value);
        HealClientRpc();
    }
    [ClientRpc]
    private void HealClientRpc()
    {
        _healthbar.UpdateSlider(HP.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RegenEPServerRpc(float amount)
    {
        EP.Value = Mathf.Min(EP.Value + amount, MaxEP.Value);
    }

    void HandleHealAtBase()
    {
        if (OnBase.Value == true)
        {
            timeSinceLastUpdate += Time.deltaTime;

            if (timeSinceLastUpdate >= UpdateTimer)
            {
                HandleHeal(MaxHP.Value / 10);
                timeSinceLastUpdate = 0f;
            }
        }
        else
        {
            timeSinceLastUpdate = 0f;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void CheatToggleServerRpc()
    {
        Money.Value += 1000;
        GetComponent<PlayerLevelController>().Experience.Value += 1000;
        // Toggle cameraLocked value
        OnCheat.Value = !OnCheat.Value;
        if (OnCheat.Value)
        {
            MaxHP.Value *= 100;
            HP.Value *= 100;
            Defense.Value *= 100;
            MovementSpeed.Value *= 10;
            AttackDam.Value *= 10000;
            _healthbar.SetMaxSlider(MaxHP.Value);
            _healthbar.UpdateSlider(HP.Value);
        }
        else
        {
            MaxHP.Value /= 100;
            HP.Value /= 100;
            Defense.Value /= 100;
            MovementSpeed.Value /= 10;
            AttackDam.Value /= 10000;
            _healthbar.SetMaxSlider(MaxHP.Value);
            _healthbar.UpdateSlider(HP.Value);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamServerRpc(float Damage, ulong attackerID, string attackerName)
    {
        if (IsOwner)
        {
            // Get attack log
            GameLogger.Instance.LogActionServerRpc(attackerName, " Attack ", PlayerData.Value.playerName.ToString() + " : " + Damage + "Damage");
        }
        TakeDamClientRpc(Damage);
        HP.Value = Mathf.Max(0, HP.Value - Damage);
        //_playerScore
        _playerScore.IncreaseTotalDamageReceived(Damage);
        if (HP.Value <= 0)
        {
            GameManager.Instance.HandleKill(attackerID, MoneyValue.Value, EXPValue.Value, "Player", this.gameObject.transform);
            _playerScore.IncreaseDeaths();
            _animator.SetBool("dead", true);
            isDead.Value = true;
            DeadClientRpc();
        }
    }

    [ClientRpc]
    public void TakeDamClientRpc(float Damage)
    {
        _healthbar.UpdateSlider(HP.Value - Damage);
    }

    [ClientRpc]
    private void DeadClientRpc()
    {
        activeBuffs.Clear();
        activeDeBuffs.Clear();
        SetRendererVisibilityClientRpc(false);
        StartCoroutine(Respawn());
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignToPoolServerRpc()
    {
        if (!IsServer) return;
        poolManager.poolTeam[this.gameObject.tag].Enqueue(gameObject);
        poolManager.poolPlayer["Player"].Enqueue(gameObject);


        if (this.tag == "Top")
        {
            GameManager.Instance.PlayerTop.Add(this.NetworkObjectId);
        }
        else
        {
            GameManager.Instance.PlayerBottom.Add(this.NetworkObjectId);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bush"))
        {
            HandleBushTriggerEnter(other.gameObject);
        }
        if (IsServer)
        {
            if (other.CompareTag("Bridge"))
            {
                OnBridge.Value = true;
            }
            else if (other.name.ToLower().Contains("spawnpoint"))
            {
                HandleBaseTriggerEnter(other);
            }
            else if (other.CompareTag("Rune"))
            {
                HandleRuneTriggerEnter(other);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bush"))
        {
            HandleBushTriggerExit();
        }
        if (IsServer)
        {
            if (other.CompareTag("Bridge"))
            {
                OnBridge.Value = false;
            }
            else if (other.name.ToLower().Contains("spawnpoint"))
            {
                HandleBaseTriggerExit(other);
            }
        }
    }

    private void HandleBaseTriggerEnter(Collider Base)
    {
        if (this.tag == Base.tag)
        {
            OnBase.Value = true;

            if (IsOwner)
            {
                // Respawn log
                GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Enter Base: ", Base.tag);
            }
        }
    }

    private void HandleBaseTriggerExit(Collider Base)
    {
        OnBase.Value = false;

        if (IsOwner)
        {
            // Respawn log
            GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Exit Base: ", Base.tag);
        }
    }

    private void HandleBushTriggerEnter(GameObject bushObject)
    {
        currentBush = bushObject.GetComponent<BushScript>();
        if (currentBush != null)
        {
            currentBush.PlayerEntered(gameObject, gameObject.tag);
            if (IsOwner)
            {
                // Respawn log
                GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Enter Bush: ", currentBush.name);
            }
        }
    }

    private void HandleBushTriggerExit()
    {
        if (currentBush != null)
        {
            currentBush.PlayerExited(gameObject, gameObject.tag);
            if (IsOwner)
            {
                // Respawn log
                GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Exit Bush: ", currentBush.name);
            }
        }
        currentBush = null;
    }

    private void HandleRuneTriggerEnter(Collider runeCollider)
    {
        Rune rune = runeCollider.GetComponent<Rune>();
        if (rune != null)
        {
            if (rune.runeType == RuneType.DoubleDamage)
            {
                ApplyBuff(2, 0, 0, 0, 0, 15f, "RuneDoebleDamage");
            }
            else if (rune.runeType == RuneType.Haste)
            {
                ApplyBuff(0, 0, 2, 0, 0, 15f, "RuneHaste");
            }

            Destroy(runeCollider.gameObject);
            runeCollider.GetComponent<NetworkObject>().Despawn();
        }
        if (IsOwner)
        {
            // Respawn log
            GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Pick rune: ", rune.runeType.ToString());
        }
    }

    public void ChangeVisibility(string visibility)
    {
        if (visibility == "Top")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamTopVisible"));
        }
        else if (visibility == "Bottom")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("TeamBottomVisible"));
        }
        else if (visibility == "Default")
        {
            ChangeLayerOfObjectAndChildren(gameObject, LayerMask.NameToLayer("Default"));
        }
    }

    private void ChangeLayerOfObjectAndChildren(GameObject obj, int layer)
    {
        if (obj.layer != layer)
        {
            // Set the new layer
            obj.layer = layer;

            // Recursively change the layer for children
            foreach (Transform child in obj.transform)
            {
                ChangeLayerOfObjectAndChildren(child.gameObject, layer);
            }
        }
    }

    [ClientRpc]
    private void SetRendererVisibilityClientRpc(bool isVisible)
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = isVisible;
        }
    }

    public void BackToBase()
    {
        _characterController.enabled = false;
        this.gameObject.transform.position = respawnPoint.position;
        _characterController.enabled = true;
    }

    // Call this method to award money to the player
    [ServerRpc(RequireOwnership = false)]
    public void AwardMoneyServerRpc(int amount)
    {
        if (IsServer)
        {
            _playerScore.IncreaseTotalGold(amount);
            Money.Value += amount;
        }
    }

    // Call this method to deduct money from the player
    [ServerRpc(RequireOwnership = false)]
    public void DeductMoneyServerRpc(int amount)
    {
        if (IsServer && Money.Value >= amount)
        {
            Money.Value -= amount;
        }
    }

    private IEnumerator Respawn()
    {
        _characterController.enabled = false;

        // Wait for the respawn time
        yield return new WaitForSeconds(10f);
        _animator.SetBool("dead", false);
        BackToBase();

        _characterController.enabled = true;
        if (IsServer)
        {
            HP.Value = MaxHP.Value;
            EP.Value = MaxEP.Value;
        }
        _healthbar.UpdateSlider(MaxHP.Value - 1);
        isDead.Value = false;
        SetRendererVisibilityClientRpc(true);
        if (IsOwner)
        {
            // Respawn log
            GameLogger.Instance.LogActionServerRpc(PlayerData.Value.playerName.ToString(), " Respawn ", "at base " + this.gameObject.tag);
        }
    }

    public void GetStunt(float duration)
    {
        if (IsServer)
        {
            StartCoroutine(StuntCoroutine(duration));
        }
    }
    private IEnumerator StuntCoroutine(float duration)
    {
        isDead.Value = true;
        yield return new WaitForSeconds(duration);
        isDead.Value = false;
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
        public float DefenseMultiplier;
        public float SpeedMultiplier;
        public float AttackSpeedMultiplier;
        public float coolDownHasteMultiplie;
    }
    private class Debuff
    {
        public string Identifier;
        public float Duration;
        public float DamageReduction;
        public float DefenseReduction;
        public float SpeedReduction;
        public float AttackSpeedReduction;
        public float coolDownHasteReduction;
    }

    private float originalAttackDamage;
    private float originalDefense;
    private float originalMovementSpeed;
    private float originalAttackSpeed;
    private float originalCooldownHaste;

    // Buff Part
    public void ApplyBuff(float damageMultiplier, float defenseMultiplier, float speedMultiplier, float attackSpeedMultiplier, float coolDownHasteMultiplie, float duration, string Identifier)
    {
        var newBuff = new Buff()
        {
            Identifier = Identifier,
            Duration = duration,
            DamageMultiplier = damageMultiplier,
            DefenseMultiplier = defenseMultiplier,
            SpeedMultiplier = speedMultiplier,
            AttackSpeedMultiplier = attackSpeedMultiplier,
            coolDownHasteMultiplie = coolDownHasteMultiplie
        };

        activeBuffs.Add(newBuff);

        if (!isBuffActive && !isDeBuffActive)
        {
            originalAttackDamage = AttackDam.Value;
            originalDefense = Defense.Value;
            originalMovementSpeed = MovementSpeed.Value;
            originalAttackSpeed = AttackSpeedPerSec.Value;
            originalCooldownHaste = CooldownHaste.Value;
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
    public void ApplyDebuff(float damageReduction, float defenseReduction, float speedReduction, float attackSpeedReduction, float coolDownHasteMultiplie, float duration, string Identifier)
    {
        var newDebuff = new Debuff()
        {
            Identifier = Identifier,
            Duration = duration,
            DamageReduction = damageReduction,
            DefenseReduction = defenseReduction,
            SpeedReduction = speedReduction,
            AttackSpeedReduction = attackSpeedReduction,
            coolDownHasteReduction = coolDownHasteMultiplie
        };

        activeDeBuffs.Add(newDebuff);
        if (!isBuffActive && !isDeBuffActive)
        {
            originalAttackDamage = AttackDam.Value;
            originalDefense = Defense.Value;
            originalMovementSpeed = MovementSpeed.Value;
            originalAttackSpeed = AttackSpeedPerSec.Value;
            originalCooldownHaste = CooldownHaste.Value;
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
            float netDefenseChange = 0f;
            float netMovementSpeedChange = 0f;
            float netAttackSpeedChange = 0f;
            float netCooldownHasteChange = 0f;

            // Calculate debuff effects as negative changes
            foreach (var debuff in activeDeBuffs)
            {
                netAttackDamageChange -= debuff.DamageReduction;
                netDefenseChange -= debuff.DefenseReduction;
                netMovementSpeedChange -= debuff.SpeedReduction;
                netAttackSpeedChange -= debuff.AttackSpeedReduction;
                netCooldownHasteChange -= debuff.coolDownHasteReduction;
            }

            // Calculate buff effects as positive changes
            foreach (var buff in activeBuffs)
            {
                netAttackDamageChange += buff.DamageMultiplier;
                netDefenseChange += buff.DefenseMultiplier;
                netMovementSpeedChange += buff.SpeedMultiplier;
                netAttackSpeedChange += buff.AttackSpeedMultiplier;
                netCooldownHasteChange += buff.coolDownHasteMultiplie;
            }

            // Apply the net changes to the original stats
            AttackDam.Value = ApplyStatChange(originalAttackDamage, netAttackDamageChange);
            Defense.Value = ApplyStatChange(originalDefense, netDefenseChange);
            MovementSpeed.Value = ApplyStatChange(originalMovementSpeed, netMovementSpeedChange);
            AttackSpeedPerSec.Value = ApplyStatChange(originalAttackSpeed, netAttackSpeedChange);
            CooldownHaste.Value = ApplyStatChange(originalCooldownHaste, netCooldownHasteChange);
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
        Defense.Value = originalDefense;
        MovementSpeed.Value = originalMovementSpeed;
        AttackSpeedPerSec.Value = originalAttackSpeed;
        CooldownHaste.Value = originalCooldownHaste;
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

    [ServerRpc(RequireOwnership = false)]
    public void CheckBuyStatServerRpc(string stat)
    {
        if (Money.Value >= 100 && StatUpgradable.Value > 0)
        {
            Money.Value -= 100;
            StatUpgradable.Value -= 1;
            switch (stat)
            {
                case "MaxHP":
                    MaxHP.Value += 50;
                    break;
                case "MaxEP":
                    MaxEP.Value += 25;
                    break;
                case "Defense":
                    Defense.Value += 2;
                    break;
                case "AttackDam":
                    AttackDam.Value += 5;
                    break;
                case "MovementSpeed":
                    MovementSpeed.Value += 1;
                    break;
                case "CooldownHaste":
                    CooldownHaste.Value += 1;
                    break;
            }
        }
    }
}