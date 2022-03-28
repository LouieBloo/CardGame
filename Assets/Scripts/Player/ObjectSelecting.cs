using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelecting : MonoBehaviour
{
    public Grid grid;
    public CameraTracker cameraTracker;

    private Selectable selectedPermanent;
    private Selectable savedSelectedPermanent;
    private HexDirection selectedPermanentTargetOrientation = HexDirection.NONE;
    private HexDirection mouseOrientation;
    private PermanentCell hoveringPermanent;
    private List<PermanentCell> extraHoveringPermanents = new List<PermanentCell>();
    private List<PermanentCell> selectedHalfHoverPermanents = new List<PermanentCell>();
    private PermanentCell altClickHoveringPermanent;

    private PermanentCell hitCell;

    private Texture2D currentTexture;

    private Action<PermanentCell,List<PermanentCell> ,HexDirection , HexDirection > findTargetCallback;

    Coroutine pollingRoutine;

    // Start is called before the first frame update
    void Start()
    {
        startPolling();
    }

    public void startPolling()
    {
        if(pollingRoutine == null)
        {
            pollingRoutine = StartCoroutine(pollingForInput());
        }
    }

    public void stopPolling()
    {
        if(pollingRoutine != null)
        {
            StopCoroutine(pollingRoutine);
            pollingRoutine = null;
        }

        deselectPermanent();
    }

    // Update is called once per frame
    IEnumerator pollingForInput()
    {
        //yield first so our grid is loaded
        yield return null;
        while (true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            // Casts the ray and get the first game object hit
            Physics.Raycast(ray, out hit);

            //dont do our logic if we are hovering over UI elements
            if(EventSystem.current.currentSelectedGameObject != null)
            {
                //Debug.Log("Pause!");
            }
            else
            {
                HexCoordinates mouseCoords = grid.getHexCoordinatesFromPosition(hit.point);
                hitCell = grid.cells[mouseCoords];

                if (hitCell)
                {
                    //permanent logic
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        selectCell(hitCell);
                    }
                    else
                    {
                        //hovering logic
                        if (hitCell != hoveringPermanent)
                        {
                            selectHoveringPermanent(hitCell);
                        }

                        handleAdditionalHovering(mouseCoords, hit);
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0))
                    {
                        deselectPermanent();
                    }
                    deselectHoveringPermanent();
                }

                //right click wipes everything
                if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    if (hoveringPermanent != null && hoveringPermanent.hasPermanent())
                    {
                        altClickHoveringPermanentSelected();
                    }
                    else
                    {
                        deselectPermanent();
                        deselectHoveringPermanent();
                    }
                }
                else if (Input.GetKeyUp(KeyCode.Mouse1))
                {
                    releaseAltClickHoveringPermanent();
                }
            }
            
            
           

            yield return null;
        }
    }

    public void findTarget(Action<PermanentCell,List<PermanentCell> ,HexDirection , HexDirection > callback, Selectable selectable)
    {
        findTargetCallback = callback;

        if (selectedPermanent)
        {
            savedSelectedPermanent = selectedPermanent;
            deselectPermanent();
            deselectHoveringPermanent();
        }

        selectedPermanent = selectable;
        selectedPermanent.select();

        startPolling();
    }

    void handleAdditionalHovering(HexCoordinates mouseCoords,RaycastHit hit)
    {
        //arrow direction
        float xDifference = hitCell.transform.position.x - hit.point.x;
        float zDifference = hitCell.transform.position.z - hit.point.z;
        if (zDifference > 0.3)
        {
            mouseOrientation = xDifference > 0 ? HexDirection.SW : HexDirection.SE;
        }
        else if (zDifference < -0.3)
        {
            mouseOrientation = xDifference > 0 ? HexDirection.NW : HexDirection.NE;
        }
        else
        {
            mouseOrientation = xDifference > 0 ? HexDirection.W : HexDirection.E;
        }


        if (selectedPermanent)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                rotateTargetOrientation();
            }

            selectedPermanentTargetOrientation = selectedPermanentTargetOrientation != HexDirection.NONE ? selectedPermanentTargetOrientation : selectedPermanent.getOrientation();
            Selectable targetSelectable = hoveringPermanent.hasPermanent() ? hoveringPermanent.getAttachedPermanent().GetComponent<Selectable>() : hoveringPermanent;
            Selectable.OnHoverOverSelectableResponse selectedPermanentHoverResponse = selectedPermanent.onMouseHoverEnter(hoveringPermanent,targetSelectable, selectedPermanentTargetOrientation, mouseOrientation);
            if (selectedPermanentHoverResponse != null)
            {
                //set mouse texture for movement orientation
                setMouseTexture(selectedPermanentHoverResponse.texture, new Vector2(64, 64));

                //select any extra cells such as big creatures with 2 spaces
                selectHoveringPermanentExtra(selectedPermanentHoverResponse.selectableArea);
            }

        }
    }

    public bool isEmptyCellTargeted()
    {
        return getTargetedCell() && !hitCell.hasPermanent();
    }

    public PermanentCell getTargetedCell()
    {
        return hitCell;
    }

    void selectHoveringPermanent(PermanentCell newPermanent)
    {
        deselectHoveringPermanent();

        newPermanent.hover();
        hoveringPermanent = newPermanent;
    }

    void deselectHoveringPermanent()
    {
        if (hoveringPermanent)
        {
            hoveringPermanent.deHover();
            hoveringPermanent = null;
        }
        deselectselectHoveringPermanentExtra();
        resetMouseTexture();
    }

    void altClickHoveringPermanentSelected()
    {
        releaseAltClickHoveringPermanent();
        altClickHoveringPermanent = hoveringPermanent;
        altClickHoveringPermanent.getAttachedPermanent().GetComponent<Selectable>().onAltClick(Input.mousePosition);
    }

    void releaseAltClickHoveringPermanent()
    {
        if (altClickHoveringPermanent != null && altClickHoveringPermanent.hasPermanent())
        {
            altClickHoveringPermanent.getAttachedPermanent().GetComponent<Selectable>().onAltClickRelease();
        }

        altClickHoveringPermanent = null;
    }

    //target being whatever cell we will be adding this extra too. when attacking its the move location, not the attacking target
    void selectHoveringPermanentExtra(Selectable.SelectableHexArea area)
    {
        deselectselectHoveringPermanentExtra();

        if(area.type == Selectable.SelectableHexAreaType.Line)
        {
            Vector3 targetPosition = hoveringPermanent.transform.position;
            for(int x = 0; x < area.distance; x++)
            {
                HexCoordinates neighbor = HexUtility.GetNeighbour(grid.getHexCoordinatesFromPosition(targetPosition), CellHelper.getOppositeOfDirection(area.orientation));
                if (grid.cells[neighbor])
                {
                    extraHoveringPermanents.Add(grid.cells[neighbor]);
                    grid.cells[neighbor].hover();
                    targetPosition = grid.cells[neighbor].transform.position;
                }
            }
        }
    }

    void deselectselectHoveringPermanentExtra()
    {
        if (extraHoveringPermanents.Count > 0)
        {
            foreach(PermanentCell p in extraHoveringPermanents)
            {
                p.deHover();
            }
            extraHoveringPermanents.Clear();
        }
    }

    void resetMouseTexture()
    {
        currentTexture = null;
        Cursor.SetCursor(currentTexture, Vector2.zero, CursorMode.Auto);
    }

    void setMouseTexture(Texture2D texture, Vector2 offset)
    {
        currentTexture = texture;
        Cursor.SetCursor(currentTexture, offset, CursorMode.Auto);
    }

    void rotateTargetOrientation()
    {
        selectedPermanentTargetOrientation = CellHelper.nextDirectionWhenRotated(selectedPermanentTargetOrientation);
    }

    //when its a creatures turn we want to auto select it
    public void selectNextInTurnOrder(Transform objectToSelect)
    {
        if (selectedPermanent)
        {
            deselectPermanent();
        }
        else if(grid.cells[grid.getHexCoordinatesFromPosition(objectToSelect.position)].getAttachedPermanent())
        {
            /*Debug.Log(objectToSelect.position);
            Debug.Log(grid.cells[grid.getHexCoordinatesFromPosition(objectToSelect.position)]);
            Debug.Log(grid.cells[grid.getHexCoordinatesFromPosition(objectToSelect.position)].getAttachedPermanent());*/
            selectedPermanent = grid.cells[grid.getHexCoordinatesFromPosition(objectToSelect.position)].getAttachedPermanent().GetComponent<Selectable>();
            selectHalfHoverPermanents(selectedPermanent.select());
        }
    }

    void selectCell(PermanentCell cell)
    {
        bool issueCommand = false;
        //if the cell we hit has something attached
        if (cell.hasPermanent())
        {
            if (selectedPermanent)
            {
                //we have a permanent selected and we selected an
                if (selectedPermanent.gameObject != cell.getAttachedPermanent().gameObject)
                {
                    issueCommand = true;
                }
                else
                {
                    //we clicked on ourselves, deselect
                    cameraTracker.trackTarget(selectedPermanent.transform,true);
                    //deselectPermanent();
                }
            }
            else if(cell.hasSelectablePermanent())
            {
                selectedPermanent = cell.getAttachedPermanent().GetComponent<Selectable>();
                selectHalfHoverPermanents(selectedPermanent.select());
            }
        }
        else if (selectedPermanent){
            issueCommand = true;
        }

        if (issueCommand)
        {
            if(findTargetCallback != null)
            {
                findTargetCallback(cell, extraHoveringPermanents, selectedPermanentTargetOrientation, mouseOrientation);
            }
            else
            {
                selectedPermanent.commandIssuedToCell(cell, extraHoveringPermanents, selectedPermanentTargetOrientation, mouseOrientation, commandSuccessFullReset);
            }
        }
    }

    void deselectPermanent()
    {
        findTargetCallback = null;

        if (selectedPermanent)
        {
            selectedPermanent.deselect();
            selectedPermanent = null;
        }

        selectedPermanentTargetOrientation = HexDirection.NONE;
        resetMouseTexture();
        resetSelectedHalfHoverPermanents();
        //cameraTracker.stopTargeting();
    }

    void selectHalfHoverPermanents(Selectable.SelectableHexArea area)
    {
        resetSelectedHalfHoverPermanents();

        if (selectedPermanent != null && area != null)
        {
            for (int x = 0; x < area.distance; x++)
            {
                List<HexCoordinates> ringHexs = HexUtility.GetRing(grid.getHexCoordinatesFromPosition(selectedPermanent.transform.position), x+1);

                foreach(HexCoordinates hex in ringHexs)
                {
                    if (grid.cells[hex])
                    {
                        List<Vector3> path = grid.findPathVector3(grid.getHexCoordinatesFromPosition(selectedPermanent.transform.position), hex);
                        if (path != null && path.Count <= area.distance)
                        {
                            grid.cells[hex].halfSelect();
                            selectedHalfHoverPermanents.Add(grid.cells[hex]);
                        }
                    }
                }
            }
        }
    }

    void resetSelectedHalfHoverPermanents()
    {
        foreach(PermanentCell c in selectedHalfHoverPermanents)
        {
            c.deHalfSelect();
        }
        selectedHalfHoverPermanents.Clear();
    }

    public void commandSuccessFullReset()
    {
        deselectPermanent();
    }
}
