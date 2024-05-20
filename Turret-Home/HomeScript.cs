using Unity.Netcode;
using UnityEngine;

public class HomeScript : NetworkBehaviour, IDamageable
{
    private string homeTag;

    private PoolManager poolManager;
    private HealthBar _healthbar;

    [Header("MinionData")]
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>(0);
    public NetworkVariable<float> HP = new NetworkVariable<float>(0);
    public NetworkVariable<float> Defense = new NetworkVariable<float>(0);

    public override void OnNetworkSpawn()
    {
        poolManager = PoolManager.Instance;
        MaxHP.Value = 8000f;
        HP.Value = MaxHP.Value;
        Defense.Value = 0f;
        homeTag = gameObject.CompareTag("Top") ? "Top" : "Bottom";
        if (IsServer)
        {
            if (homeTag == "Top")
            {
                GameManager.Instance.TopBase = this.gameObject;
            }
            else
            {
                GameManager.Instance.BottomBase = this.gameObject;
            }
        }
        _healthbar = GetComponentInChildren<HealthBar>();
        _healthbar.SetMaxSlider(MaxHP.Value);
        _healthbar.UpdateSlider(HP.Value);
    }

    public void Start()
    {
        AssignToPoolServerRpc();
    }
    private void Update() {
        if(IsServer){
             if(GameManager.Instance.GameEnd.Value == true){
                gameObject.GetComponent<NetworkObject>().Despawn();
            }
        }
       
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamServerRpc(float Damage, ulong attackerID, string attackerName)
    {
        if (TurretSpawn.AreAllTurretsDown(homeTag))
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
                gameObject.SetActive(false);
                gameObject.GetComponent<NetworkObject>().Despawn(false);
            }
        }
    }

    [ClientRpc]
    public void TakeDamClientRpc(float Damage)
    {
        _healthbar.UpdateSlider(HP.Value - Damage);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AssignToPoolServerRpc()
    {
        poolManager.poolTeam[this.gameObject.tag].Enqueue(gameObject);
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