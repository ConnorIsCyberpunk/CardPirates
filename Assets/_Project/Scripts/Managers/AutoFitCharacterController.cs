using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CharacterController))]
public class AutoFitCharacterController : MonoBehaviour
{
    [Tooltip("Small extra added to capsule height so it never clips the head.")]
    public float heightPadding = 0.02f;

    [Tooltip("Small extra added to capsule radius so it never clips shoulders.")]
    public float radiusPadding = 0.02f;

    [Tooltip("Re-apply continuously in edit mode.")]
    public bool continuousInEditMode = true;

    CharacterController cc;

    void Awake() { cc = GetComponent<CharacterController>(); }
    void Reset() { FitNow(); }
    void OnValidate() { FitNow(); }

    void Update()
    {
        if (!Application.isPlaying && continuousInEditMode)
            FitNow();
    }

    public void FitNow()
    {
        if (!cc) cc = GetComponent<CharacterController>();

        // Collect visible renderers under this character
        var rends = GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return;

        // World bounds of all renderers
        var world = new Bounds(rends[0].bounds.center, Vector3.zero);
        for (int i = 0; i < rends.Length; i++)
            world.Encapsulate(rends[i].bounds);

        // Convert important points to this Transform's local space
        Vector3 localMin = transform.InverseTransformPoint(world.min);
        Vector3 localMax = transform.InverseTransformPoint(world.max);
        Vector3 localSize = localMax - localMin;

        // Feet at localMin.y, head at localMax.y
        float height = Mathf.Max(0.1f, localSize.y + heightPadding);
        float radius = Mathf.Max(0.05f,
            Mathf.Max(Mathf.Abs(localSize.x), Mathf.Abs(localSize.z)) * 0.5f + radiusPadding);

        // Center.y so the bottom of the capsule sits at the feet (localMin.y)
        float centerY = localMin.y + height * 0.5f;

        cc.height = height;
        cc.radius = radius;
        cc.center = new Vector3(0f, centerY, 0f);

        // sensible defaults
        cc.stepOffset = Mathf.Clamp(height * 0.2f, 0.2f, 0.6f);
        cc.skinWidth = 0.08f;
        cc.minMoveDistance = 0.001f;
    }
}
