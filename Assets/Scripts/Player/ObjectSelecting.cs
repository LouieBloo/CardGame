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
            if(hitCell != hoveringPermanent)
            {
                selectHoveringPermanent(hitCell);
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

    public bool isEmptyCellTargeted()
    {
        Debug.Log("cell: " + hitCell);
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
            selectedPermanent.commandIssuedToCell(cell, grid);
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
