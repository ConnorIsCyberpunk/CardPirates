using System.Collections.Generic;
using UnityEngine;

public class VoteUI : MonoBehaviour
{
    public static VoteUI Instance;

    public GameObject buttonPrefab;
    public Transform buttonParent;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Open(List<PlayerSetup> players)
    {
        gameObject.SetActive(true);

        foreach (Transform t in buttonParent)
            Destroy(t.gameObject);

        foreach (var p in players)
        {
            var b = Instantiate(buttonPrefab, buttonParent);
            b.GetComponentInChildren<UnityEngine.UI.Text>().text = p.myBio.playerName;

            b.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                GameRoundManager.Instance.CaptainEliminates(p);
                gameObject.SetActive(false);
            });
        }
    }
}
