using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="SkinDatabase", menuName="Settings/Skin Database")]
public class SkinDatabase : ScriptableObject
{
    public List<GameObject> skinPrefabs = new List<GameObject>();

    public GameObject Get(int index)
    {
        if (skinPrefabs == null || skinPrefabs.Count == 0) return null;
        
        // Safety check index
        if (index < 0 || index >= skinPrefabs.Count) index = 0;
        
        return skinPrefabs[index];
    }
}