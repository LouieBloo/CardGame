using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PermanentCell : NetworkBehaviour
{
    private Permanent attachedPermanent;
    private SpriteRenderer spriteRenderer;

    public GameObject objectToSpawnOnStartup;
    public Quaternion objectToSpawnRotation;

    NetworkVariable<ulong> attachedPermanentId = new NetworkVariable<ulong>();
    NetworkBehaviour attachedNetworkBehavior;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnDrawGizmos()
    {
        if (objectToSpawnOnStartup)
        {
            drawString(objectToSpawnOnStartup.name, transform.position, Color.black, new Vector2(0, 3));
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public GameObject spawnStartingObject()
    {
        if (!objectToSpawnOnStartup) { return null; }
        GameObject go = Instantiate(objectToSpawnOnStartup, GetComponent<Transform>().position, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        attachPermanent(go.GetComponent<NetworkObject>().NetworkObjectId);
        return go;
    }

    public GameObject spawnObject(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, transform.position, Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();
        attachPermanent(go.GetComponent<NetworkObject>().NetworkObjectId);
        return go;
    }

    private void attachPermanent(ulong permanentId)
    {
        attachedPermanentId.Value = permanentId;
    }

    public bool hasPermanent()
    {
        return attachedPermanentId.Value > 0;
    }

    public void select()
    {
        spriteRenderer.color = Color.black;
    }

    public void deSelect()
    {
        spriteRenderer.color = Color.white;
    }



    static public void drawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
    {
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPosition = view.camera.WorldToScreenPoint(worldPosition);
        if (screenPosition.y < 0 || screenPosition.y > view.camera.pixelHeight || screenPosition.x < 0 || screenPosition.x > view.camera.pixelWidth || screenPosition.z < 0)
            return;
        UnityEditor.Handles.BeginGUI();
        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = (int)textSize,
            normal = new GUIStyleState() { textColor = textColor }
        };
        Vector2 size = style.CalcSize(new GUIContent(text));
        var alignedPosition =
            ((Vector2)screenPosition +
            size * ((anchor + Vector2.left + Vector2.up) / 2f)) * (Vector2.right + Vector2.down) +
            Vector2.up * view.camera.pixelHeight;
        GUI.Label(new Rect(alignedPosition, size), text, style);
        UnityEditor.Handles.EndGUI();
    }
}
