using UnityEngine;

public class PlayerSkinController : MonoBehaviour
{
    [SerializeField] Transform modelRoot;
    [SerializeField] SkinDatabase database;

    GameObject current;

    public void ApplySkin(int index)
    {
        if (!database || !modelRoot) return;

        if (current) Destroy(current);
        
        var prefab = database.Get(index);
        if (!prefab) return;

        current = Instantiate(prefab, modelRoot);
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.identity;
        current.transform.localScale = Vector3.one;
    }
}