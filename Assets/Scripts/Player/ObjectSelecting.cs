using HexMapTools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ObjectSelecting : MonoBehaviour
{
    public Grid grid;

    private Selectable selectedPermanent;
    private PermanentCell hoveringPermanent;

    private PermanentCell hitCell;

    private PermanentCell attackMovePotentialCellPosition;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        // Casts the ray and get the first game object hit
        Physics.Raycast(ray, out hit);
        //Debug.Log("This hit at " + hit.point);
        HexCoordinates mouseCoords = grid.getHexCoordinatesFromPosition(hit.point);
        hitCell = grid.cells[mouseCoords];

        if (hitCell)
        {
            //hoving logic
            if (hitCell != hoveringPermanent)
            {
                selectHoveringPermanent(hitCell);
            }

            //attack selecting
            if (selectedPermanent && hitCell.hasPermanent())
            {
                attackCellSelect(mouseCoords, hit);
            }
            
            //permanent logic
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                selectCell(hitCell);
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
        
    }

    PermanentCell attackCellSelect(HexCoordinates mouseCoords,RaycastHit hit)
    {
        //arrow direction
        float xDifference = hitCell.transform.position.x - hit.point.x;
        float zDifference = hitCell.transform.position.z - hit.point.z;
        HexDirection direction;
        if (zDifference > 0.3)
        {
            direction = xDifference > 0 ? HexDirection.SW : HexDirection.SE;
        }
        else if (zDifference < -0.3)
        {
            direction = xDifference > 0 ? HexDirection.NW : HexDirection.NE;
        }
        else
        {
            direction = xDifference > 0 ? HexDirection.W : HexDirection.E;
        }

        HexCoordinates attackMoveCoordinates = HexUtility.GetNeighbour(mouseCoords, direction);
        if (grid.cells[attackMoveCoordinates])
        {
            if (!attackMovePotentialCellPosition)
            {
                attackMovePotentialCellPosition = grid.cells[attackMoveCoordinates];
                attackMovePotentialCellPosition.select();
            }
            else if (attackMovePotentialCellPosition && attackMovePotentialCellPosition != grid.cells[attackMoveCoordinates])
            {
                //when the previous potential attack move cell is equal to the NEW hovering permanent we dont want to deselct the previous attacking cell as its now selected
                if (attackMovePotentialCellPosition == hoveringPermanent) {
                }
                else
                {
                    attackMovePotentialCellPosition.deSelect();
                }

                attackMovePotentialCellPosition = grid.cells[attackMoveCoordinates];
                attackMovePotentialCellPosition.select();
            }
        }
        
        return attackMovePotentialCellPosition;
       //Debug.Log("x: " + xDifference + " z: " + zDifference);
        
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
        if (hoveringPermanent)
        {
            hoveringPermanent.deSelect();
        }

        newPermanent.select();
        hoveringPermanent = newPermanent;
    }

    void deselectHoveringPermanent()
    {
        if (hoveringPermanent)
        {
            hoveringPermanent.deSelect();
            hoveringPermanent = null;
        }
        if (attackMovePotentialCellPosition) {
            attackMovePotentialCellPosition.deSelect();
            attackMovePotentialCellPosition = null;
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
                    deselectPermanent();
                }
            }
            else
            {
                selectedPermanent = cell.getAttachedPermanent().GetComponent<Selectable>();
                selectedPermanent.select();
            }
        }
        else if (selectedPermanent){
            issueCommand = true;
        }

        if (issueCommand)
        {
            selectedPermanent.commandIssuedToCell(cell, attackMovePotentialCellPosition, grid);
        }
        

    }

    void deselectPermanent()
    {
        if (selectedPermanent)
        {
            selectedPermanent.deselect();
            selectedPermanent = null;
        }
    }
}
