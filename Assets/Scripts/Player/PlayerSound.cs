using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSound : NetworkBehaviour
{
    private Player _player;
    private float _footsetpTimer;
    private float _footsetpTimerMax = .4f;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    private void Start()
    {
        _player.OnRelaod += PlayerOnReload;
        _player.OnShoot += PlayerOnShoot;
        _player.OnDead += PlayerOnDead;
        _player.OnKillScore += PlayerOnKillScore;
        _player.OnGoldCoinChanged += PlayerOnGoldCoinChanged;
    }

    private void PlayerOnGoldCoinChanged(object sender, EventArgs e)
    {
        float volume = 1f;
        SoundManager.Instance.PlayPickGoldCoinSound(_player.transform.position, volume);
    }

    private void PlayerOnKillScore(object sender, EventArgs e)
    {
        float volume = 1f;
        SoundManager.Instance.PlayKillScoreSound(_player.transform.position, volume);
    }

    private void PlayerOnDead(object sender, EventArgs e)
    {
        float volume = 1f;
        SoundManager.Instance.PlayPlayerDeadSound(_player.transform.position, volume);
    }

    private void PlayerOnShoot(object sender, EventArgs e)
    {
        if (IsHost)
        {
            PlayerOnShootClientRpc();
        }
        else
        {

            PlayerOnShootServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerOnShootServerRpc()
    {
        PlayerOnShootClientRpc();
    }

    [ClientRpc]
    private void PlayerOnShootClientRpc()
    {
        float volume = 0.6f;
        if (_player.GetGunObject().getCurrentAmmo() != 0)
        {
            SoundManager.Instance.PlayGunShootSound(_player.transform.position, volume);
        }
        else
        {
            SoundManager.Instance.PlayEmptyShoot(_player.transform.position, volume);
        }
    }

    private void PlayerOnReload(object sender, EventArgs e)
    {
        if (IsHost)
        {
            PlayerOnReloadClientRpc();
        }
        else
        {

            PlayerOnReloadServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerOnReloadServerRpc()
    {
        PlayerOnReloadClientRpc();
    }
    [ClientRpc]
    private void PlayerOnReloadClientRpc()
    {
        float volume = 0.4f;
        SoundManager.Instance.PlayReloadSound(_player.transform.position, volume);
    }


    [ServerRpc(RequireOwnership = false)]
    private void PlayerOnWalkingServerRpc()
    {
        PlayerOnWalkingClientRpc();
    }

    [ClientRpc]
    private void PlayerOnWalkingClientRpc()
    {
        float volume = .5f;
        SoundManager.Instance.PlayFootstepSound(_player.transform.position, volume);
    }

    private void FixedUpdate()
    {
        _footsetpTimer -= Time.deltaTime;
        if (_footsetpTimer < 0f)
        {
            _footsetpTimer = _footsetpTimerMax;
            if (_player.IsWalking())
            {
                if (IsHost)
                {
                    PlayerOnWalkingClientRpc();
                }
                else
                {
                    PlayerOnWalkingServerRpc();
                }
            }
        }

    }
}
