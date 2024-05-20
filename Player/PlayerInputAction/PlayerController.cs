using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    private InputManager _inputManager;
    private CharacterController _characterController;
    private CinemachineVirtualCamera _virtualCamera;
    private PlayerStatusController _playerStatusController;
    private Camera Camera;
    private Camera CameraMinimap;

    public Skill_HUD skillHUD;

    public LayerMask DodgeLayers;
    public LayerMask WallLayers;

    public Transform spawnpointbullet;

    [Header("Movement")]
    public Vector3 moveDirection;
    public float dashCooldown = 10f;
    private bool canDash = true;
    private float gravity = -100f;

    private bool cameraLocked = false;
    private bool SpectateOn = false;

    public float _remainingIFrameTime = 0f;

    public void Awake()
    {
        _inputManager = GetComponent<InputManager>();
        _characterController = GetComponent<CharacterController>();
        _playerStatusController = GetComponent<PlayerStatusController>();
        Camera = Camera.main.GetComponent<Camera>();
        CameraMinimap = GameObject.Find("MinimapCam").GetComponent<Camera>();
        skillHUD = GetComponentInChildren<Skill_HUD>(true);
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient && NetworkObject.IsOwner)
        {
            _virtualCamera = GameObject.Find("MainCamera").GetComponent<CinemachineVirtualCamera>();
            _virtualCamera.Follow = transform;

            StartCoroutine(WaitForReCheck());
        }
    }

    public IEnumerator WaitForReCheck()
    {
        while (MiniMapMark.RecheckFinish == false)
        {
            yield return null;
        }
        if (gameObject.tag == "Top")
        {
            int layer = LayerMask.NameToLayer("TeamBottomVisible");
            Camera.cullingMask &= ~(1 << layer);
            CameraMinimap.cullingMask &= ~(1 << layer);
        }
        else if (gameObject.tag == "Bottom")
        {
            int layer = LayerMask.NameToLayer("TeamTopVisible");
            Camera.cullingMask &= ~(1 << layer);
            CameraMinimap.cullingMask &= ~(1 << layer);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        HandleCameraMove();
        HandleFOV();
        HandleCameraState();
        HandleSpectate();

        // Check if the player is dead
        if (_playerStatusController.isDead.Value == false)
        {
            Cursormanager();
            HandleMovement();
            HandleDash();
        }

        CheckFallBelowLevel();
    }

    void HandleSpectate()
    {
        if (_inputManager.SpectateInput)
        {
            _inputManager.SpectateInput = false;
            SpectateToggleServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpectateToggleServerRpc()
    {
        SpectateOn = !SpectateOn;
        var transposer = _virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        int layer = gameObject.tag == "Top" ? LayerMask.NameToLayer("TeamBottomVisible") : LayerMask.NameToLayer("TeamTopVisible");
        if (SpectateOn)
        {
            transposer.m_FollowOffset = (new Vector3(0, 1500, -500));
            Camera.cullingMask |= (1 << layer);
            CameraMinimap.cullingMask |= (1 << layer);
        }
        else
        {
            transposer.m_FollowOffset = (new Vector3(0, 170, -60));
            Camera.cullingMask &= ~(1 << layer);
            CameraMinimap.cullingMask &= ~(1 << layer);
        }
    }

    void CheckFallBelowLevel()
    {
        if (transform.position.y < -1000)
        {
            _characterController.enabled = false;
            transform.position = new Vector3(transform.position.x, 53, transform.position.z);
            _characterController.enabled = true;
        }
    }

    /// <summary>
    /// Handle charater movement
    /// </summary>
    void HandleMovement()
    {
        moveDirection = new Vector3(_inputManager.movementInput.x, 0f, _inputManager.movementInput.y).normalized;

        if (!_characterController.isGrounded)
        {
            moveDirection.y = gravity;
        }

        _characterController.Move(moveDirection * _playerStatusController.MovementSpeed.Value * Time.deltaTime);

        if (IsOwner)
        {
            // Movement log, ***Need to be player name
            GameLogger.Instance.LogMovementServerRpc(_playerStatusController.PlayerData.Value.playerName.ToString(), transform.position);
        }
    }

    /// <summary>
    /// Handles the player's dash ability.
    /// </summary>
    void HandleDash()
    {
        if (canDash && _inputManager.DashButton)
        {
            IFrame(0.5f);
            StartCoroutine(DashCoroutine());

            if (IsOwner)
            {
                // Dash Log, ***Need to be player name
                GameLogger.Instance.LogDashServerRpc(_playerStatusController.PlayerData.Value.playerName.ToString(), transform.position);
            }

            // Disable further dashing and reset cooldown
            canDash = false;
            StartCoroutine(ResetDashCooldown());
        }
    }

    /// <summary>
    /// Handle camera move when cursor touch edge of screen
    /// </summary>
    void HandleCameraMove()
    {
        float mouseX = _inputManager.mouseInput.x;
        float mouseY = _inputManager.mouseInput.y;
        if (cameraLocked) return;
        Vector3 cameraMovement = Vector3.zero;
        if (mouseX <= 0)
        {
            cameraMovement.x -= 1f;
        }
        else if (mouseX >= Screen.width)
        {
            cameraMovement.x += 1f;
        }
        if (mouseY <= 0)
        {
            cameraMovement.z -= 1f;
        }
        else if (mouseY >= Screen.height)
        {
            cameraMovement.z += 1f;
        }
        _virtualCamera.transform.position += cameraMovement;
    }

    /// <summary>
    /// Handle FOV change with moouse scroll
    /// </summary>
    void HandleFOV()
    {
        float scrollInput = _inputManager.ScrollInput;
        if (scrollInput != 0)
        {
            _virtualCamera.m_Lens.FieldOfView = Mathf.Clamp(_virtualCamera.m_Lens.FieldOfView + scrollInput, 45f, 60f);
        }
    }

    /// <summary>
    /// Handle camera locked/unlocked state to player
    /// </summary>
    void HandleCameraState()
    {
        if (_inputManager.CameraLock)
        {
            _inputManager.CameraLock = false;

            // Toggle cameraLocked value
            cameraLocked = !cameraLocked;
            if (cameraLocked)
            {
                _virtualCamera.Follow = transform;
            }
            else
            {
                _virtualCamera.Follow = null;
            }
        }
    }

    /// <summary>
    /// Track player cursor and rotate player to face cursor
    /// </summary>
    private void Cursormanager()
    {
        Ray ray = Camera.ScreenPointToRay(_inputManager.mouseInput);
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float rayLenght;
        if (plane.Raycast(ray, out rayLenght))
        {
            Vector3 pointTolook = ray.GetPoint(rayLenght);
            this.gameObject.transform.LookAt(new Vector3(pointTolook.x, this.gameObject.transform.position.y, pointTolook.z));
            spawnpointbullet.transform.LookAt(new Vector3(pointTolook.x, spawnpointbullet.transform.position.y, pointTolook.z));
        }
    }

    public void IFrame(float waitTime)
    {
        _remainingIFrameTime += waitTime;

        if (_remainingIFrameTime > 0)
        {
            StartCoroutine(IFrameRun());
        }
    }
    /// <summary>
    /// Activates invincibility frames for a specified duration.
    /// </summary>
    /// <param name="waitTime">The duration of invincibility frames in seconds.</param>
    public IEnumerator IFrameRun()
    {
        IFrameServerRpc();

        while (_remainingIFrameTime > 0)
        {
            yield return null;
            _remainingIFrameTime -= Time.deltaTime;
        }

        ResetIFrameServerRpc();
    }

    [ServerRpc]
    private void IFrameServerRpc()
    {
        if (IsServer)
        {
            _characterController.excludeLayers = DodgeLayers;
            _characterController.excludeLayers += WallLayers;
        }
        IFrameClientRpc();
    }

    [ClientRpc]
    private void IFrameClientRpc()
    {
        _characterController.excludeLayers = DodgeLayers;
        _characterController.excludeLayers += WallLayers;
    }

    [ServerRpc]
    private void ResetIFrameServerRpc()
    {
        _characterController.excludeLayers = _characterController.includeLayers;
        ResetIFrameClientRpc();
    }

    [ClientRpc]
    private void ResetIFrameClientRpc()
    {
        _characterController.excludeLayers = _characterController.includeLayers;
    }

    /// <summary>
    /// Executes the player's dash movement for a short duration.
    /// </summary>
    public IEnumerator DashCoroutine()
    {
        float dashDuration = 0.2f;
        Vector3 dashVelocity = moveDirection * 100f;
        float startTime = Time.time;

        // Continue dashing for a short duration
        while (Time.time < startTime + dashDuration)
        {
            _characterController.Move(dashVelocity * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator ResetDashCooldown()
    {
        float startTime = Time.time;
        float elapsedTime = 0f;

        while (elapsedTime < dashCooldown)
        {
            elapsedTime = Time.time - startTime;
            float remainingTime = Mathf.Max(dashCooldown - elapsedTime, 0f);
            float fillAmount = remainingTime / dashCooldown;
            skillHUD.UpdateDashCooldown(fillAmount);
            yield return null;
        }

        canDash = true;
    }

    [ClientRpc]
    public void DashClientRpc(float duration)
    {
        IFrame(0.5f);
        StartCoroutine(DashCoroutine());
    }

    [ClientRpc]
    public void IframeClientRpc(float waitTime)
    {
        IFrame(waitTime);
    }
}