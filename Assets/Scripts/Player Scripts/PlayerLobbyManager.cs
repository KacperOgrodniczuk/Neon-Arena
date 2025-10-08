using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerLobbyManager : NetworkBehaviour
{
    private SyncVar<string> _playerName = new SyncVar<string>("Player");
    
    public string PlayerName => _playerName.Value;

    //syncvar playerready


}
