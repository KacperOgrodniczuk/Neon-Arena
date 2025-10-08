using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerLobbyManager : NetworkBehaviour
{
    private readonly SyncVar<string> _playerName = new SyncVar<string>("Player");
    [ServerRpc] private void SetPlayerName(string playerName) => _playerName.Value = playerName;
    public string PlayerName => _playerName.Value;

    private const string PlayerNamePrefsKey = "PlayerName";

    //syncvar playerready

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (IsOwner)
        {
            // Load saved player name or generate a default one
            string savedName = PlayerPrefs.GetString(PlayerNamePrefsKey, $"Guest_{Random.Range(100, 999)}");
            SetPlayerName(savedName);
            Debug.Log($"Player name set to: {savedName} as read from PlayerPrefs");
        }
    }
}
