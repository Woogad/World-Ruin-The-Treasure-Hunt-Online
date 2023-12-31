using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerSelectReady : NetworkBehaviour
{
    public static PlayerSelectReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;
    public event EventHandler OnAllPlayerReady;

    private event Action _readyClientRpcName;

    private AudioSource audioSource;
    private NetworkList<PlayerReady> _playerReadyList;



    private void Awake()
    {
        Instance = this;
        _playerReadyList = new NetworkList<PlayerReady>();
        _playerReadyList.OnListChanged += PlayerReadyListOnlistChanged;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        _readyClientRpcName = this.SetPlayerReadyClientRpc;

        if (!IsServer) return;
        _playerReadyList.Add(new PlayerReady
        {
            ClientID = NetworkManager.ServerClientId,
            IsReady = false
        });
        NetworkManager.Singleton.OnClientConnectedCallback += NetworkManagerOnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManagerOnDisConnectCallback;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        NetworkManager.Singleton.OnClientConnectedCallback -= NetworkManagerOnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManagerOnDisConnectCallback;
    }

    private void PlayerReadyListOnlistChanged(NetworkListEvent<PlayerReady> changeEvent)
    {
        SetPlayerReadyClientRpc();

        Invoke(_readyClientRpcName.Method.Name, 0.1f);
    }

    private void NetworkManagerOnDisConnectCallback(ulong clientID)
    {
        RemovePlayerReadyNetworkList(clientID);

        if (IsHost)
        {
            SetPlayerUnReady();
        }
    }

    private void NetworkManagerOnClientConnectedCallback(ulong ClientID)
    {
        _playerReadyList.Add(new PlayerReady
        {
            ClientID = ClientID,
            IsReady = false
        });
    }

    public void SetPlayerReady()
    {
        SetPlayerReadyServerRpc();
    }
    public void SetPlayerUnReady()
    {
        SetPlayerUnReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        foreach (var player in _playerReadyList)
        {
            if (player.ClientID == serverRpcParams.Receive.SenderClientId)
            {
                PlayerReady playerReady = new PlayerReady
                {
                    ClientID = serverRpcParams.Receive.SenderClientId,
                    IsReady = true
                };
                int readyPlayerIndex = _playerReadyList.IndexOf(player);
                _playerReadyList[readyPlayerIndex] = playerReady;
                break;
            }
        }
        bool allClientReady = true;

        foreach (var player in _playerReadyList)
        {
            if (!player.IsReady)
            {
                allClientReady = false;
                break;
            }
        }
        if (allClientReady)
        {
            OnAllPlayerReady?.Invoke(this, EventArgs.Empty);
            PlayStartSoundClientRpc();
            StartCoroutine(StartGameCountdown(2f));
        }
    }

    private IEnumerator StartGameCountdown(float duration)
    {
        float startTimer = 0;
        while (startTimer < duration)
        {
            startTimer += Time.deltaTime;
            yield return null;
        }
        LobbyManager.Instance.DeleteLobby();
        Loader.LoadNetwork(Loader.Scene.GameScene);
        yield break;
    }

    [ClientRpc]
    private void PlayStartSoundClientRpc()
    {
        audioSource.Play();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerUnReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        foreach (var player in _playerReadyList)
        {
            if (player.ClientID == serverRpcParams.Receive.SenderClientId)
            {
                PlayerReady playerReady = new PlayerReady
                {
                    ClientID = serverRpcParams.Receive.SenderClientId,
                    IsReady = false
                };
                int readyPlayerIndex = _playerReadyList.IndexOf(player);
                _playerReadyList[readyPlayerIndex] = playerReady;
                break;
            }
        }
    }


    [ClientRpc]
    private void SetPlayerReadyClientRpc()
    {
        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientID)
    {
        foreach (var player in _playerReadyList)
        {
            if (player.ClientID == clientID)
            {
                return player.IsReady;
            }
        }
        return false;
    }

    public void RemovePlayerReadyNetworkList(ulong clientID)
    {
        Debug.Log("Remove PlayerID From ReadyList : " + clientID);
        foreach (var player in _playerReadyList)
        {
            if (player.ClientID == clientID)
            {
                int disconnectPlayerIndex = _playerReadyList.IndexOf(player);
                _playerReadyList.RemoveAt(disconnectPlayerIndex);
                break;
            }
        }
    }

}
