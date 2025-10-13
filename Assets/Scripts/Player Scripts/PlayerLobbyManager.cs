using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerLobbyManager : NetworkBehaviour
{
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Player");
    [ServerRpc] private void SetPlayerName(string playerName) => _playerName.Value = playerName;
    public string PlayerName => _playerName.Value;

    private const string PlayerNamePrefsKey = "PlayerName";

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            // Load saved player name or generate a default one
            string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey, $"Guest_{Random.Range(100, 999)}");
            SetPlayerName(savedName);
        }
    }

    public void SubscribeToNameChange(SyncVar<string>.OnChanged handler)
    {
        _playerName.OnChange += handler;
    }

    public void UnsubscribeFromNameChange(SyncVar<string>.OnChanged handler)
    {
        _playerName.OnChange -= handler;
    }
}
