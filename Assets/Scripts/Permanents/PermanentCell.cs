using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PermanentCell : Selectable
{
    private SpriteRenderer spriteRenderer;

    public GameObject objectToSpawnOnStartup;
    public Quaternion objectSpawnRotation;

    public GameObject creaturePrefab;

    private HexCoordinates hexCoordinates;

    //NetworkVariable<ulong> attachedPermanentId = new NetworkVariable<ulong>();
    NetworkVariable<NetworkObjectReference> attachedNetworkObject = new NetworkVariable<NetworkObjectReference>();

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        type = SelectableType.HexCell;
    }

    void OnDrawGizmos()
    {
        if (objectToSpawnOnStartup)
        {
            drawString(objectToSpawnOnStartup.name, transform.position, Color.black, new Vector2(0, 3));
        }
    }


    public void setHexCoordinates(HexCoordinates coordinates)
    {
        this.hexCoordinates = coordinates;
    }

    public HexCoordinates getHexCoordinates()
    {
        return this.hexCoordinates;
    }

    public GameObject spawnStartingObject()
    {
        return null;
        if (!objectToSpawnOnStartup) { return null; }
        //return spawnCreature(objectToSpawnOnStartup, objectSpawnRotation,0,"PPPP");
    }

    public NetworkObjectReference spawnCreature(Quaternion rotation, ulong ownerId,string creatureName, Vector3[] spawnCells)
    {
        //instantiate the object and set ownership
        GameObject go = Instantiate(creaturePrefab, transform.position, rotation);
        go.GetComponent<NetworkObject>().Spawn();
        if (ownerId > 0)
        {
            go.GetComponent<NetworkObject>().ChangeOwnership(ownerId);
        }

        //tell the creature what kind they are and orientation
        go.GetComponent<Creature>().setSpawnParameters(creatureName,HexDirection.E);

        //tell creature what cells they occupy
        go.GetComponent<Permanent>().setOccupiedCellsServerRpc(spawnCells);
        
        //attach to ourself
        attachPermanent(go.GetComponent<NetworkObject>());
        return go.GetComponent<NetworkObject>();
    }

    public void attachPermanent(NetworkObjectReference networkObject)
    {
        attachedNetworkObject.Value = networkObject;
    }

    public void unattachPermanent()
    {
        attachedNetworkObject.Value = new NetworkObjectReference();
    }

    public Permanent getAttachedPermanent()
    {
        if (attachedNetworkObject.Value.TryGet(out NetworkObject targetObject))
        {
            return targetObject.GetComponent<Permanent>();
        }

        return null;
    }

    public bool hasPermanent()
    {
        if (attachedNetworkObject.Value.TryGet(out NetworkObject targetObject))
        {
            return true;
        }
        return false;
        //return attachedNetworkObject && attachedNetworkObject.Value != null;
    }

    public void hover()
    {
        spriteRenderer.color = Color.black;
    }

    public void deHover()
    {
        spriteRenderer.color = Color.white;
    }


    static public void drawString(string text, Vector3 worldPosition, Color textColor, Vector2 anchor, float textSize = 15f)
    {
       /* var view = UnityEditor.SceneView.currentDrawingSceneView;
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
        UnityEditor.Handles.EndGUI();*/
    }
}
