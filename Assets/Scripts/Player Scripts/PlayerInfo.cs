using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    public PlayerManager playerManager;

    public readonly SyncVar<string> playerName = new SyncVar<string>("Player");
    [ServerRpc] private void SetPlayerName(string playerName) => this.playerName.Value = playerName;

    private const string PlayerNamePrefsKey = "PlayerName";

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
    }

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
}
