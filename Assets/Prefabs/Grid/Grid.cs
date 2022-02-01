using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HexMapTools;
using HexMapToolsExamples;
using Unity.Netcode;

[RequireComponent(typeof(HexGrid))]
public class Grid : NetworkBehaviour
{
    private HexCalculator hexCalculator;
    private HexContainer<PermanentCell> cells;

    private PermanentCell selectedPermanent;
    private HexCoordinates selectedCoordinates;

    private HexPathFinder pathFinder;
    private List<HexCoordinates> foundPath;

    public GameObject myPrefab;

    private void Start()
    {
        HexGrid hexGrid = GetComponent<HexGrid>();

        hexCalculator = hexGrid.HexCalculator;

        cells = new HexContainer<PermanentCell>(hexGrid);
        cells.FillWithChildren();

        pathFinder = new HexPathFinder(pathCost, 1f, 1f, 2000);

        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
        if (IsServer)
        {
            initializeGridPermanentsServerRpc();
        }
    }

    private void Singleton_OnServerStarted()
    {
        if (IsServer)
        {
            initializeGridPermanentsServerRpc();
        }
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Casts the ray and get the first game object hit
        Physics.Raycast(ray, out hit);
        //Debug.Log("This hit at " + hit.point);
        HexCoordinates mouseCoords = hexCalculator.HexFromPosition(hit.point);
        //Debug.Log(mouseCoords);

        /*if (cells[mouseCoords])
        {
            if (!selectedPermanent)
            {
                selectedPermanent = cells[mouseCoords];
                selectedPermanent.select();
            }
            else if (cells[mouseCoords] != selectedPermanent)
            {
                selectedPermanent.deSelect();
                selectedPermanent = cells[mouseCoords];
                selectedPermanent.select();
            }
        }
        else if (selectedPermanent)
        {
            selectedPermanent.deSelect();
            selectedPermanent = null;
        }*/

        if (IsServer && Input.GetKeyDown(KeyCode.S) && cells[mouseCoords] != null)
        {
            if (!cells[mouseCoords].hasPermanent())
            {
                createPermanentServerRpc(mouseCoords);
            }
            else
            {
                NetworkLog.LogInfoServer("Already has permanent!");
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && cells[mouseCoords] != null)
        {
            if (!selectedPermanent)
            {
                selectedPermanent = cells[mouseCoords];
                selectedCoordinates = mouseCoords;
                foreach (PermanentCell c in cells.GetCells())
                {
                    c.deSelect();
                }
                selectedPermanent.select();
            }
            else if (cells[mouseCoords] != selectedPermanent)
            {
                bool yess = pathFinder.FindPath(selectedCoordinates, mouseCoords, out foundPath);
                Debug.Log(yess + " " + foundPath.Count);
                selectedPermanent = null;
                if (yess)
                {
                    foreach (HexCoordinates h in foundPath)
                    {
                        cells[h].select();
                    }
                }
            }
        }


        if (Input.GetKeyDown(KeyCode.Mouse1)){
            foreach (PermanentCell c in cells.GetCells())
            {
                c.deSelect();
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            foreach (PermanentCell c in cells.GetCells())
            {
                c.select();
            }
        }

            /*if (Input.GetKeyDown(KeyCode.Mouse0) && cells[mouseCoords] != null)
            {
                List<HexCoordinates> ring = HexUtility.GetRing(mouseCoords, 2);
                foreach(HexCoordinates r in ring)
                {
                    if (cells[r])
                    {
                        cells[r].select();
                    }
                }
            }*/
        }

    [ServerRpc]
    void createPermanentServerRpc(HexCoordinates cell)
    {
        if (cells[cell] && !cells[cell].hasPermanent())
        {
            /*Vector3 position = hexCalculator.HexToPosition(cell);
            GameObject go = Instantiate(myPrefab, position, Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();*/
            cells[cell].spawnObject(myPrefab);

            //updateCellPermanentClientRpc(cell, go.GetComponent<NetworkObject>());
        }
        else
        {
            Debug.Log("already ahs perm");
        }
    }

    /*[ClientRpc]
    void updateCellPermanentClientRpc(HexCoordinates cell, NetworkObjectReference permanent)
    {
        Debug.Log(cell);

        if (permanent.TryGet(out NetworkObject targetObject))
        {
            Debug.Log("inside");
            cells[cell].attachPermanent(targetObject.GetComponent<Permanent>());
            Debug.Log(cells[cell].hasPermanent());
        }
        else
        {
            // Target not found on server, likely because it already has been destroyed/despawned.
        }
    }*/

    [ServerRpc]
    void initializeGridPermanentsServerRpc()
    {
        foreach (PermanentCell c in cells.GetCells())
        {
            c.spawnStartingObject();
        }
    }

    float pathCost(HexCoordinates a, HexCoordinates b)
    {

        PermanentCell cell = cells[b];

        if (cell == null)
            return float.PositiveInfinity;

        if (cell.hasPermanent())
            return float.PositiveInfinity;

        /*if (cell.Color == CellColor.Blue)
            return blueCost;
        else if (cell.Color == CellColor.Red)
            return redCost;*/

        return 1;
    }

}
