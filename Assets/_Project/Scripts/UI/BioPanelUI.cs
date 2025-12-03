using UnityEngine;
using UnityEngine.UI;

public class BioPanelUI : MonoBehaviour
{
    public static BioPanelUI Instance;

    [Header("UI")]
    public Text nameText;
    public Text traitsText;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // hidden until player spawns
    }

    public void ShowBio(PlayerBio bio)
    {
        if (bio == null)
            return;

        nameText.text = bio.playerName;

        traitsText.text = "";
        foreach (string t in bio.traits)
        {
            traitsText.text += "• " + t + "\n";
        }

        gameObject.SetActive(true);
    }
}
