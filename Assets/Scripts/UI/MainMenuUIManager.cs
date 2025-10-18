using FishNet;
using UnityEngine;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Canvas")]
    public Canvas mainMenuCanvas;

    private const string PlayerNamePrefsKey = "PlayerName";

    private void Awake()
    {
        mainMenuCanvas.worldCamera = CameraManager.Instance.cameraObject;
    }

    public void HostButton()
    {
        ConnectionManager.Instance.StartHost();
    }

    public void JoinButton()
    {
        ConnectionManager.Instance.StartClient();
    }

    public void SetIpAddress(string text)
    {
        InstanceFinder.NetworkManager.TransportManager.Transport.SetClientAddress(text);
    }

    // TODO: Load currently saved player name to show in the input field (In case a player has set one previously.)

    public void SetPlayerName(string name)
    {
        string nameToSave = name;

        // Simple validation: use a default if the field is empty
        if (string.IsNullOrEmpty(nameToSave))
        {
            nameToSave = $"Guest_{Random.Range(100, 999)}";
        }

        PlayerPrefs.SetString(PlayerNamePrefsKey, nameToSave);
        PlayerPrefs.Save();
        Debug.Log($"Player name set to: {nameToSave} and saved to PlayerPrefs");
    }
}
