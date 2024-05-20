using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : NetworkBehaviour
{
    private Animator _animator;
    public Vector2 movementInput;
    public Vector3 mouseInput;
    public float ScrollInput;
    public bool CameraLock;
    public bool DashButton;
    public bool DefaultAttackInput;
    public bool CheatInput;
    public bool SpectateInput;
    public float ItemUseInput;
    public bool QSkillUseInput;
    public bool ShiftSkillUseInput;
    public bool ESkillUseInput;
    public bool RSkillUseInput;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    public void OnMovement(InputValue inputValue)
    {
        movementInput = inputValue.Get<Vector2>();
        _animator.SetTrigger("walk");
    }

    public void OnMouseDetection(InputValue inputValue)
    {
        mouseInput = inputValue.Get<Vector2>();
    }

    public void OnZoom(InputValue inputValue)
    {
        ScrollInput = inputValue.Get<float>() * 0.01f;
    }

    public void OnCamLock(InputValue inputValue)
    {
        CameraLock = inputValue.isPressed;
    }

    public void OnDash(InputValue inputValue)
    {
        DashButton = inputValue.isPressed;
    }

    public void OnDefaultAttack(InputValue inputValue)
    {
        DefaultAttackInput = inputValue.isPressed;
    }

    public void OnCheat(InputValue inputValue)
    {
        CheatInput = inputValue.isPressed;
    }
    public void OnSpectate(InputValue inputValue)
    {
        SpectateInput = inputValue.isPressed;
    }

    public void OnItemUse(InputValue inputValue)
    {
        ItemUseInput = inputValue.Get<float>();
    }

    public void OnSkillUseQ(InputValue inputValue)
    {
        QSkillUseInput = inputValue.isPressed;
    }
    public void OnSkillUseShift(InputValue inputValue)
    {
        ShiftSkillUseInput = inputValue.isPressed;
    }
    public void OnSkillUseE(InputValue inputValue)
    {
        ESkillUseInput = inputValue.isPressed;
    }
    public void OnSkillUseR(InputValue inputValue)
    {
        RSkillUseInput = inputValue.isPressed;
    }
}