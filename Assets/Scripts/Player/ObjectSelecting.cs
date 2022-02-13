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
    private HexDirection selectedPermanentTargetOrientation;
    private HexDirection attackOrientation;
    private PermanentCell hoveringPermanent;

    private PermanentCell hitCell;

    private PermanentCell attackMovePotentialCellPosition;

    public Texture2D westAttackTexture;
    public Texture2D northWestAttackTexture;
    public Texture2D northEastAttackTexture;
    public Texture2D eastAttackTexture;
    public Texture2D southEastAttackTexture;
    public Texture2D southWestAttackTexture;

    public Texture2D westMoveTexture;
    public Texture2D northWestMoveTexture;
    public Texture2D northEastMoveTexture;
    public Texture2D eastMoveTexture;
    public Texture2D southEastMoveTexture;
    public Texture2D southWestMoveTexture;

    private Texture2D currentTexture;

    private Dictionary<HexDirection, Texture2D> mouseTextureDirectionMapping = new Dictionary<HexDirection, Texture2D>();
    private Dictionary<HexDirection, Texture2D> mouseTextureMoveDirectionMapping = new Dictionary<HexDirection, Texture2D>();

    // Start is called before the first frame update
    void Start()
    {
        mouseTextureDirectionMapping.Add(HexDirection.W, westAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NW, northWestAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NE, northEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.E, eastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SE, southEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SW, southWestAttackTexture);

        mouseTextureMoveDirectionMapping.Add(HexDirection.W, westMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.NW, northWestMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.NE, northEastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.E, eastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.SE, southEastMoveTexture);
        mouseTextureMoveDirectionMapping.Add(HexDirection.SW, southWestMoveTexture);
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

            if (Input.GetKeyDown(KeyCode.R))
            {
                rotateTargetOrientation();
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

        //set the texture of the mouse to the right direction of attack arrow
        if(!currentTexture || currentTexture != mouseTextureDirectionMapping[direction])
        {
            currentTexture = mouseTextureDirectionMapping[direction];
            Cursor.SetCursor(currentTexture, new Vector2(64,64), CursorMode.Auto);
            attackOrientation = direction;
        }
        
        //check if the neighbor in this direction exists, and do stuff on it sometimes
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
        /*if (hoveringPermanent)
        {
            hoveringPermanent.deSelect();
            resetMouseTexture();
        }*/
        deselectHoveringPermanent();

        newPermanent.select();
        hoveringPermanent = newPermanent;

        if(selectedPermanent && selectedPermanent.CreatureMovement)
        {
            selectedPermanentTargetOrientation = selectedPermanent.CreatureMovement.getOrientation();
            setMouseTexture(mouseTextureMoveDirectionMapping[selectedPermanentTargetOrientation], new Vector2(64, 64));
        }
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
        resetMouseTexture();
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
        if (selectedPermanent && selectedPermanent.CreatureMovement)
        {
            selectedPermanentTargetOrientation = CellHelper.nextDirectionWhenRotated(selectedPermanentTargetOrientation);
            setMouseTexture(mouseTextureMoveDirectionMapping[selectedPermanentTargetOrientation], new Vector2(64, 64));
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
            selectedPermanent.commandIssuedToCell(cell, attackMovePotentialCellPosition, grid, attackMovePotentialCellPosition == null ? selectedPermanentTargetOrientation : CellHelper.getOppositeOfDirection(attackOrientation));
        }
    }

    void deselectPermanent()
    {
        if (selectedPermanent)
        {
            selectedPermanent.deselect();
            selectedPermanent = null;
        }

        resetMouseTexture();
    }
}
