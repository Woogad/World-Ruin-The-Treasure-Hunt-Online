using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnToggleWeaponModeAction;
    public event EventHandler OnShootWeaponAction;
    public event EventHandler OnReloadAction;
    public event EventHandler<OnViewScoreBoardHoldActionArgs> OnViewScoreBoardHoldAction;
    public class OnViewScoreBoardHoldActionArgs : EventArgs
    {
        public bool IsHoldViewScoreBoardAction;
    }
    public event EventHandler<OnEscActionArgs> OnEscAction;
    public class OnEscActionArgs : EventArgs
    {
        public bool IsEscMenuOpen;
    }
    public event EventHandler<OnShootWeaponActionArgs> OnShootWeaponHoldAction;
    public class OnShootWeaponActionArgs : EventArgs
    {
        public bool IsHoldShootAction;
    }

    private PlayerInputAction _playerInputAction;
    private bool _isEscMenuOpen = false;

    private void Awake()
    {
        Instance = this;
        _playerInputAction = new PlayerInputAction();
        _playerInputAction.Enable();

        //*left mouse event
        _playerInputAction.Player.ShootWeapon.started += ShootWeaponStart;
        _playerInputAction.Player.ShootWeapon.performed += ShootWeaponPerformed;
        _playerInputAction.Player.ShootWeapon.canceled += ShootWeaponCanceled;

        _playerInputAction.Player.Interact.performed += InteractPerformed;
        _playerInputAction.Player.ReloadWeapon.performed += ReloadWeaponPerformed;
        _playerInputAction.Player.ToggleWeaponMode.performed += ToggleWeaponModePerformed;
        _playerInputAction.Player.Esc.performed += EscPerformed;
        _playerInputAction.Player.ViewScoreBoard.performed += ViewScoreBoardPerformed;
        _playerInputAction.Player.ViewScoreBoard.canceled += ViewScoreBoardCanceled;
    }

    private void OnDestroy()
    {
        _playerInputAction.Player.ShootWeapon.started -= ShootWeaponStart;
        _playerInputAction.Player.ShootWeapon.performed -= ShootWeaponPerformed;
        _playerInputAction.Player.ShootWeapon.canceled -= ShootWeaponCanceled;

        _playerInputAction.Player.Interact.performed -= InteractPerformed;
        _playerInputAction.Player.ReloadWeapon.performed -= ReloadWeaponPerformed;
        _playerInputAction.Player.ToggleWeaponMode.performed -= ToggleWeaponModePerformed;
        _playerInputAction.Player.Esc.performed -= EscPerformed;
        _playerInputAction.Player.ViewScoreBoard.performed -= ViewScoreBoardPerformed;
        _playerInputAction.Player.ViewScoreBoard.canceled -= ViewScoreBoardCanceled;
        _playerInputAction.Dispose();
    }

    private void ViewScoreBoardPerformed(InputAction.CallbackContext context)
    {
        bool isHoldViewScoreBoardAction = true;
        OnViewScoreBoardHoldAction?.Invoke(this, new OnViewScoreBoardHoldActionArgs
        {
            IsHoldViewScoreBoardAction = isHoldViewScoreBoardAction
        });
    }
    private void ViewScoreBoardCanceled(InputAction.CallbackContext context)
    {
        bool isHoldViewScoreBoardAction = false;
        OnViewScoreBoardHoldAction?.Invoke(this, new OnViewScoreBoardHoldActionArgs
        {
            IsHoldViewScoreBoardAction = isHoldViewScoreBoardAction
        });
    }

    private void EscPerformed(InputAction.CallbackContext context)
    {
        _isEscMenuOpen = !_isEscMenuOpen;
        OnEscAction?.Invoke(this, new OnEscActionArgs
        {
            IsEscMenuOpen = _isEscMenuOpen
        });
    }

    private void ShootWeaponStart(InputAction.CallbackContext obj)
    {
        if (!GameManager.Instance.IsGamePlaying() || _isEscMenuOpen) return;
        if (!Player.LocalInstance.IsAlive()) return;
        OnShootWeaponAction(this, EventArgs.Empty);
    }

    private void ShootWeaponPerformed(InputAction.CallbackContext obj)
    {
        if (!GameManager.Instance.IsGamePlaying() || _isEscMenuOpen) return;
        if (!Player.LocalInstance.IsAlive()) return;
        bool isHoldShootAction = true;
        OnShootWeaponHoldAction?.Invoke(this, new OnShootWeaponActionArgs
        {
            IsHoldShootAction = isHoldShootAction
        });
    }

    private void ShootWeaponCanceled(InputAction.CallbackContext obj)
    {
        bool isHoldShootAction = false;
        OnShootWeaponHoldAction?.Invoke(this, new OnShootWeaponActionArgs
        {
            IsHoldShootAction = isHoldShootAction
        });
    }

    private void ToggleWeaponModePerformed(InputAction.CallbackContext obj)
    {
        if (!GameManager.Instance.IsGamePlaying() || _isEscMenuOpen) return;
        if (!Player.LocalInstance.IsAlive()) return;
        OnToggleWeaponModeAction?.Invoke(this, EventArgs.Empty);
    }

    private void ReloadWeaponPerformed(InputAction.CallbackContext obj)
    {
        if (!GameManager.Instance.IsGamePlaying() || _isEscMenuOpen) return;
        if (!Player.LocalInstance.IsAlive()) return;
        OnReloadAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractPerformed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (GameManager.Instance.IsGameWaitingPlayer())
        {
            OnInteractAction?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            if (!Player.LocalInstance.IsAlive()) return;
            if (_isEscMenuOpen) return;
            OnInteractAction?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector2 GetMovementVectorNormalized()
    {
        if (!GameManager.Instance.IsGamePlaying() || _isEscMenuOpen) return new Vector2(0, 0);
        if (!Player.LocalInstance.IsAlive()) return new Vector2(0, 0);
        Vector2 inputVector = _playerInputAction.Player.Move.ReadValue<Vector2>();

        inputVector = inputVector.normalized;

        return inputVector;
    }
}
