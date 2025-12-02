using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TraitDatabase", menuName = "GameData/Trait Database")]
public class TraitDatabase : ScriptableObject
{
    public List<string> traits = new List<string>();
}
