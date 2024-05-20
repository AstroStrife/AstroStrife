using Unity.Netcode;
using UnityEngine;

public class PlayerAttackController : NetworkBehaviour
{
    [Header("Bullet")]
    public Transform spawnpointbullet;

    private InputManager _inputManager;
    public PlayerStatusController _playerStatusController;
    private PoolManager poolManager;
    private Animator _animator;
    private float _attackCooldown;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _animator = GetComponentInChildren<Animator>();
            _inputManager = GetComponent<InputManager>();
            _playerStatusController = GetComponent<PlayerStatusController>();
            poolManager = PoolManager.Instance;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!_playerStatusController.isDead.Value)
        {
            HandleShooting();
        }
    }

    private void HandleShooting()
    {
        _attackCooldown -= Time.deltaTime;
        if (_inputManager.DefaultAttackInput && _attackCooldown <= 0f)
        {
            _attackCooldown = 1f / _playerStatusController.AttackSpeedPerSec.Value; // Cooldown based on attack speed
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        _animator.SetTrigger("attack");
        Vector3 direction = spawnpointbullet.forward;
        ShootServerRpc(spawnpointbullet.position, spawnpointbullet.rotation, direction, _playerStatusController.AttackDam.Value);

        // Log the attack action; logging can be made conditional to reduce overhead
        if (IsOwner)
        {
            GameLogger.Instance.LogActionServerRpc(_playerStatusController.PlayerData.Value.playerName.ToString(), " Fire ", gameObject.tag + "_Bullet");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc(Vector3 position, Quaternion rotation, Vector3 direction, float Damage)
    {
        poolManager.RequestBulletFromPoolServerRpc(this.gameObject.tag, "Bullet", position, rotation * Quaternion.Euler(0f, -90f, 90f), direction, Damage, this.NetworkObjectId, this.gameObject.name);
    }
}