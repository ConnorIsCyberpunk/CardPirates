using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectWithPreviews : MonoBehaviourPunCallbacks
{
    [Header("Data")]
    public SkinDatabase database;

    [Header("UI")]
    public RawImage[] previewSlots;
    public Button[] selectButtons;

    [Header("Spawn Settings")]
    public string playerPrefabName = "Player";
    public Transform specificSpawnPoint; 

    [Header("Preview Render")]
    public int renderSize = 256;
    public Color backgroundColor = new Color(0, 0, 0, 0);
    public string[] previewLayers = new string[] { "UI3D_0", "UI3D_1", "UI3D_2" };

    Transform previewRoot;
    List<Camera> cams = new List<Camera>();
    List<RenderTexture> rts = new List<RenderTexture>();

    public override void OnEnable()
    {
        base.OnEnable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Awake()
    {
        SetupPreviews();
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

    void SetupPreviews()
    {
        previewRoot = new GameObject("PreviewRoot").transform;
        previewRoot.position = new Vector3(9999, 9999, 9999);

        for (int i = 0; i < previewSlots.Length; i++)
        {
            var slot = previewSlots[i];
            var prefab = database != null ? database.Get(i) : null;
            
            if (slot == null || prefab == null) continue;

            // Assign unique layer for each preview camera
            string layerName = "";
            if (i < previewLayers.Length) layerName = previewLayers[i];
            else layerName = previewLayers[previewLayers.Length - 1];

            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0) continue;

            var model = Instantiate(prefab, previewRoot);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
            SetLayerRecursively(model, layer);

            var camGO = new GameObject("PreviewCam_" + i);
            var cam = camGO.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            cam.cullingMask = 1 << layer;
            cams.Add(cam);

            var rt = new RenderTexture(renderSize, renderSize, 16, RenderTextureFormat.ARGB32);
            rts.Add(rt);
            cam.targetTexture = rt;
            slot.texture = rt;

            FitCameraToObject(cam, model);
        }
    }

    void Select(int skinIndex)
    {
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (specificSpawnPoint != null)
        {
            pos = specificSpawnPoint.position;
            rot = specificSpawnPoint.rotation;
        }
        else
        {
            Debug.LogError("Spawn Point not assigned, using zero");
        }

        PhotonNetwork.Instantiate(playerPrefabName, pos, rot, 0, new object[] { skinIndex });

        if (PhotonNetwork.IsMasterClient)
        {
            if (RoleManager.Instance != null)
            {
                RoleManager.Instance.DistributeRoles();
            }
        }

        gameObject.SetActive(false);
        Destroy(gameObject, 0.5f);
    }

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