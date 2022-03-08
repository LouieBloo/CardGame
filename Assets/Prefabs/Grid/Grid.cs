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
    public HexContainer<PermanentCell> cells;

    private PermanentCell selectedPermanent;
    private HexCoordinates selectedCoordinates;

    private HexPathFinder pathFinder;

    private void Start()
    {
        HexGrid hexGrid = GetComponent<HexGrid>();

        hexCalculator = hexGrid.HexCalculator;

        cells = new HexContainer<PermanentCell>(hexGrid);
        cells.FillWithChildren();

        //set all permanent cells hex positions for easy reference
        foreach (PermanentCell c in cells.GetCells())
        {
            c.setHexCoordinates(hexCalculator.HexFromPosition(c.transform.position));
        }

        pathFinder = new HexPathFinder(pathCost, 1f, 1f, 2000);

        NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
    }

    private void Singleton_OnServerStarted()
    {
        /*if (IsServer)
        {
            initializeGridPermanentsServerRpc();
        }*/
    }

    private void Update()
    {
        /*Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Casts the ray and get the first game object hit
        Physics.Raycast(ray, out hit);
        //Debug.Log("This hit at " + hit.point);
        HexCoordinates mouseCoords = hexCalculator.HexFromPosition(hit.point);*/
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

        
        /*
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
        }*/


        /*if (Input.GetKeyDown(KeyCode.Mouse1)){
            foreach (PermanentCell c in cells.GetCells())
            {
                c.deHover();
            }
        }*/

        

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

    private bool isCellOktoSpawn(HexCoordinates cellCoordinates)
    {
        if (cells[cellCoordinates] && !cells[cellCoordinates].hasPermanent())
        {
            return true;
        }

        return false;
    }

    public void createCreatureOnCell(HexCoordinates[] spawnCells, ulong ownerId,string creatureName)
    {
        //verify there is nothing else on these cells
        bool okToSpawn = true;
        foreach (HexCoordinates h in spawnCells)
        {
            if (!isCellOktoSpawn(h))
            {
                Debug.Log("Cant spawn object, already a permanent");
                okToSpawn = false;
            }
        }

        if (okToSpawn)
        {
            //transform our hex coordinates into vector 3 for the permanent
            Vector3[] creatureCellPositions = new Vector3[spawnCells.Length];
            for(int x = 0; x < spawnCells.Length; x++)
            {
                creatureCellPositions[x] = getPositionFromHexCoordinates(spawnCells[x]);
            }
            NetworkObjectReference spawnedCreatureReference = cells[spawnCells[0]].spawnCreature(Quaternion.identity, ownerId, creatureName, creatureCellPositions);
            //tell all the other cells that will be occupied that they are occupied
            for(int x = 1; x < spawnCells.Length; x++)
            {
                cells[spawnCells[x]].attachPermanent(spawnedCreatureReference);
            }
        }
    }

    //we assume all security testing was done before this call
    public void permanentMovedToNewCell(NetworkObjectReference permanent, Vector3 endPosition, string finalOrientation, List<PermanentCell> extraSpaces)
    {
        HexCoordinates endCell = hexCalculator.HexFromPosition(endPosition);

        if (permanent.TryGet(out NetworkObject targetObject))
        {
            //first we unattach the cells that this permanent occupies
            Permanent targetPermanent = targetObject.GetComponent<Permanent>();
            foreach(Vector3 cellVector in targetPermanent.getCellsOccupied())
            {
                HexCoordinates cellCoords = hexCalculator.HexFromPosition(cellVector);
                cells[cellCoords].unattachPermanent();
            }

            //we must now tell our new cells that they have a permanent and tell the permanent all the positions it occupies
            int totalSpacesTaken = 1;
            if(extraSpaces != null && extraSpaces.Count > 0)
            {
                totalSpacesTaken += extraSpaces.Count;
            }
            Vector3[] positionsOfSpaces = new Vector3[totalSpacesTaken];
            //main cell is a special case
            cells[endCell].attachPermanent(permanent);
            positionsOfSpaces[0] = hexCalculator.HexToPosition(endCell);
            for(int x = 1; x < positionsOfSpaces.Length; x++)
            {
                cells[extraSpaces[x - 1].getHexCoordinates()].attachPermanent(permanent);
                positionsOfSpaces[x] = extraSpaces[x - 1].transform.position;
            }
            targetPermanent.setOccupiedCellsServerRpc(positionsOfSpaces);
        }
        else
        {
            Debug.Log("HOW DID THIS HAPPEN?");
        }
    }

    public void creatureDiedOnCell(NetworkObjectReference reference)
    {
        GlobalVars.gv.turnManager.removeObjectFromTurnOrder(reference);
    }

    public List<HexCoordinates> findPath(HexCoordinates start, HexCoordinates end)
    {
        List<HexCoordinates> foundPath;
        bool didFindPath = pathFinder.FindPath(start, end, out foundPath);
        //Debug.Log(didFindPath + " " + foundPath.Count);
        if (didFindPath)
        {
            return foundPath;
        }

        return null;
    }

    public List<Vector3> findPathVector3(HexCoordinates start, HexCoordinates end)
    {
        List<HexCoordinates> foundPath = findPath(start, end);
        if (foundPath != null)
        {
            List<Vector3> returnObj = new List<Vector3>();
            foreach(HexCoordinates h in foundPath)
            {
                returnObj.Add(hexCalculator.HexToPosition(h));
            }
            return returnObj;
        }

        return null;
    }

    public HexCoordinates getHexCoordinatesFromPosition(Vector3 position)
    {
        return hexCalculator.HexFromPosition(position);
    }

    public Vector3 getPositionFromHexCoordinates(HexCoordinates coordinates)
    {
        return hexCalculator.HexToPosition(coordinates);
    }

    public PermanentCell getPermanentCellAtPosition(Vector3 position)
    {
        return cells[getHexCoordinatesFromPosition(position)];
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
