using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Skins/Skin Database", fileName = "SkinDatabase")]
public class SkinDatabase : ScriptableObject
{
    public GameObject[] skinPrefabs;   // 0..N (Character, Character2, Character3)
    public GameObject Get(int index)
    {
        if (skinPrefabs == null || skinPrefabs.Length == 0) return null;
        index = Mathf.Clamp(index, 0, skinPrefabs.Length - 1);
        return skinPrefabs[index];
    }
}
