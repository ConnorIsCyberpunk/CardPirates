using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectWithPreviews : MonoBehaviourPunCallbacks
{
    [Header("Data")]
    public SkinDatabase database;

    [Header("UI")]
    public RawImage[] previewSlots;   // size 3
    public Button[] selectButtons;    // size 3

    [Header("Spawn")]
    public string playerPrefabName = "Player";   // must be in Resources/
    public Vector3 spawnPos = Vector3.zero;

    [Header("Preview Render")]
    public int renderSize = 256;
    public Color backgroundColor = new Color(0, 0, 0, 0);
    [Tooltip("One unique layer per preview slot (create in Tags & Layers).")]
    public string[] previewLayers = new[] { "UI3D_0", "UI3D_1", "UI3D_2" };

    // internals
    Transform previewRoot;
    readonly List<Camera> cams = new();
    readonly List<RenderTexture> rts = new();

    public override void OnEnable()
    {
        base.OnEnable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake()
    {
        SetupPreviews();

        // hook buttons
        for (int i = 0; i < selectButtons.Length; i++)
        {
            int idx = i;
            if (selectButtons[i] != null)
                selectButtons[i].onClick.AddListener(() => Select(idx));
        }
    }

    public override void OnJoinedRoom()
    {
        gameObject.SetActive(true);
    }

    void OnDestroy()
    {
        foreach (var c in cams) if (c) Destroy(c.gameObject);
        foreach (var rt in rts) if (rt) rt.Release();
        if (previewRoot) Destroy(previewRoot.gameObject);
    }

    void SetupPreviews()
    {
        previewRoot = new GameObject("PreviewRoot").transform;
        previewRoot.gameObject.hideFlags = HideFlags.HideAndDontSave;
        previewRoot.position = new Vector3(9999, 9999, 9999);

        for (int i = 0; i < previewSlots.Length; i++)
        {
            var slot = previewSlots[i];
            var prefab = database != null ? database.Get(i) : null;
            if (slot == null || prefab == null) continue;

            string layerName = (i < previewLayers.Length) ? previewLayers[i] : previewLayers[^1];
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"CharacterSelectWithPreviews: Layer '{layerName}' not found. Create it in Project Settings > Tags and Layers.");
                continue;
            }

            var model = Instantiate(prefab, previewRoot);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            SetLayerRecursively(model, layer);

            var camGO = new GameObject($"PreviewCam_{i}");
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.cullingMask = 1 << layer;
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane = 50f;
            cam.fieldOfView = 25f;
            cams.Add(cam);

            var rt = new RenderTexture(renderSize, renderSize, 16, RenderTextureFormat.ARGB32);
            rt.name = $"RT_Char_{i}";
            rt.Create();
            rts.Add(rt);
            cam.targetTexture = rt;
            slot.texture = rt;

            FitCameraToObject(cam, model);
        }
    }

    void Select(int skinIndex)
    {
        // Use spawn point if found
        Vector3 pos = Vector3.zero;
        CharacterSpawner spawner = FindObjectOfType<CharacterSpawner>();
        if (spawner != null && spawner.spawnPoint != null)
        {
            pos = spawner.spawnPoint.position;
        }
        else if (spawner != null)
        {
            pos = spawner.transform.position;
        }
        else
        {
            Debug.LogWarning("[CharacterSelectWithPreviews] No spawner found, defaulting to (0,0,0)");
        }

        PhotonNetwork.Instantiate(playerPrefabName, pos, Quaternion.identity, 0, new object[] { skinIndex });
        gameObject.SetActive(false);
        OnDestroy();
    }

    // --- Helpers that were missing ---
    static void SetLayerRecursively(GameObject go, int layer)
    {
        foreach (var t in go.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    static void FitCameraToObject(Camera cam, GameObject obj)
    {
        var bounds = new Bounds(obj.transform.position, Vector3.zero);
        foreach (var r in obj.GetComponentsInChildren<Renderer>())
            bounds.Encapsulate(r.bounds);

        Vector3 center = bounds.center;
        float radius = bounds.extents.magnitude;

        cam.transform.position = center + Vector3.back;
        cam.transform.LookAt(center, Vector3.up);

        float dist = radius / Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        cam.transform.position = center - cam.transform.forward * dist;
    }
}
