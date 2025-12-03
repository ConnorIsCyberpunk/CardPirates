using UnityEngine;

public class PlayerBioGenerator : MonoBehaviour
{
    public static PlayerBioGenerator Instance;

    [Header("Data")]
    public TraitDatabase traitDatabase;
    public int traitsPerPlayer = 3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public PlayerBio GenerateBio(string playerName)
    {
        PlayerBio bio = new PlayerBio();
        bio.playerName = playerName;

        if (traitDatabase == null || traitDatabase.traits.Count == 0)
        {
            Debug.LogError("TraitDatabase is empty!");
            return bio;
        }

        for (int i = 0; i < traitsPerPlayer; i++)
        {
            int index = Random.Range(0, traitDatabase.traits.Count);
            bio.traits.Add(traitDatabase.traits[index]);
        }

        return bio;
    }
}
