using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Gameplay Settings")]
    public int maxPlayers = 8;
    public float roundTime = 120f;

    [Header("Audio")]
    [Range(0f, 1f)] public float masterVolume = 0.8f;

    [Header("Network")]
    public string lobbySceneName = "Lobby";
}
