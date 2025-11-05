using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="SkinDatabase", menuName="Settings/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public List<GameObject> skinPrefabs = new();

    public GameObject Get(int index)
    {
        if (skinPrefabs == null || skinPrefabs.Count == 0) return null;
        index = Mathf.Clamp(index, 0, skinPrefabs.Count - 1);
        return skinPrefabs[index];
    }
}
