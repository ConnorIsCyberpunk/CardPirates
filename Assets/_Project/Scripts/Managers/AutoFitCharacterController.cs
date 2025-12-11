using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(CharacterController))]
public class AutoFitCharacterController : MonoBehaviour
{
    public float heightPadding = 0.02f;
    public float radiusPadding = 0.02f;
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

        var rends = GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0) return;

        // Calculate bounds of all children
        var world = new Bounds(rends[0].bounds.center, Vector3.zero);
        for (int i = 0; i < rends.Length; i++)
            world.Encapsulate(rends[i].bounds);

        Vector3 localMin = transform.InverseTransformPoint(world.min);
        Vector3 localMax = transform.InverseTransformPoint(world.max);
        Vector3 localSize = localMax - localMin;

        float height = Mathf.Max(0.1f, localSize.y + heightPadding);
        float radius = Mathf.Max(0.05f, Mathf.Max(Mathf.Abs(localSize.x), Mathf.Abs(localSize.z)) * 0.5f + radiusPadding);
        float centerY = localMin.y + height * 0.5f;

        cc.height = height;
        cc.radius = radius;
        cc.center = new Vector3(0f, centerY, 0f);

        cc.stepOffset = height * 0.2f;
        cc.skinWidth = 0.08f;
        cc.minMoveDistance = 0.001f;
    }
}