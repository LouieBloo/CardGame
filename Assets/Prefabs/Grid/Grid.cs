using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HexMapTools;
using HexMapToolsExamples;

[RequireComponent(typeof(HexGrid))]
public class Grid : MonoBehaviour
{
    private HexCalculator hexCalculator;
    private HexContainer<Permanent> cells;

    private Permanent selectedPermanent;
    private HexCoordinates selectedCoordinates;

    private HexPathFinder pathFinder;
    private List<HexCoordinates> foundPath;

    private void Start()
    {
        HexGrid hexGrid = GetComponent<HexGrid>();

        hexCalculator = hexGrid.HexCalculator;

        cells = new HexContainer<Permanent>(hexGrid);
        cells.FillWithChildren();

        pathFinder = new HexPathFinder(pathCost, 1f, 1f, 2000);
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
            }else if(cells[mouseCoords] != selectedPermanent)
            {
                selectedPermanent.deSelect();
                selectedPermanent = cells[mouseCoords];
                selectedPermanent.select();
            }
        }else if (selectedPermanent)
        {
            selectedPermanent.deSelect();
            selectedPermanent = null;
        }*/

        if (Input.GetKeyDown(KeyCode.Mouse0) && cells[mouseCoords] != null)
        {
            if (!selectedPermanent)
            {
                selectedPermanent = cells[mouseCoords];
                selectedCoordinates = mouseCoords;
                selectedPermanent.select();
            }
            else if (cells[mouseCoords] != selectedPermanent)
            {
                bool yess = pathFinder.FindPath(selectedCoordinates, mouseCoords, out foundPath);
                Debug.Log(yess + " " + foundPath.Count);
                selectedPermanent = null;
                if (yess) {
                    foreach(HexCoordinates h in foundPath)
                    {
                        cells[h].select();
                    }
                }
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

    float pathCost(HexCoordinates a, HexCoordinates b)
    {

        Permanent cell = cells[b];

        if (cell == null)
            return float.PositiveInfinity;

        /*if (cell.Color == CellColor.Blue)
            return blueCost;
        else if (cell.Color == CellColor.Red)
            return redCost;*/

        return 1;
    }

}
