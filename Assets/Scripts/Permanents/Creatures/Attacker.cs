using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using static Selectable;

public class Attacker : NetworkBehaviour
{
    private GameObject projectilePrefab;

    private NetworkVariable<int> range = new NetworkVariable<int>();
    private NetworkVariable<FixedString64Bytes> rangeType = new NetworkVariable<FixedString64Bytes>();

    private CreatureMovement creatureMovement;

    public Texture2D westAttackTexture;
    public Texture2D northWestAttackTexture;
    public Texture2D northEastAttackTexture;
    public Texture2D eastAttackTexture;
    public Texture2D southEastAttackTexture;
    public Texture2D southWestAttackTexture;
    private Dictionary<HexDirection, Texture2D> mouseTextureDirectionMapping = new Dictionary<HexDirection, Texture2D>();

    public Texture2D rangedAttackTexture;

    private Grid grid;

    public enum RangeType
    {
        Melee,
        Ranged
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!grid)
        {
            grid = GameObject.FindGameObjectsWithTag("Grid")[0].GetComponent<Grid>();
        }

        creatureMovement = GetComponent<CreatureMovement>();

        mouseTextureDirectionMapping.Add(HexDirection.W, westAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NW, northWestAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.NE, northEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.E, eastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SE, southEastAttackTexture);
        mouseTextureDirectionMapping.Add(HexDirection.SW, southWestAttackTexture);
    }

    public void setup(CreatureStats stats)
    {
        if (IsServer)
        {
            this.range.Value = stats.baseRange;
            this.rangeType.Value = stats.attackType.ToString();
        }

        projectilePrefab = stats.projectilePrefab;
    }

    [ServerRpc]
    public void commandIssuedToCellServerRpc(Vector3 target, Vector3[] extraHoveringCells, HexDirection orientation, HexDirection mouseOrientation)
    {
        //transform our vector3 to permanent cells
        PermanentCell targetCell = grid.cells[grid.getHexCoordinatesFromPosition(target)];
        List<PermanentCell> extraCells = new List<PermanentCell>();
        if(extraHoveringCells != null)
        {
            foreach(Vector3 v in extraHoveringCells)
            {
                extraCells.Add(grid.cells[grid.getHexCoordinatesFromPosition(v)]);
            }
        }

        if (!targetCell) { Debug.Log("Invalid attack, cant find cell"); return; }

        if (targetCell.hasPermanent())
        {
            //if(targetActionCell.getAttachedPermanent().GetComponent<NetworkObject>().OwnerClientId == OwnerClientId) { Debug.Log("Invalid attack, target is same team as us!"); return; }

            if (getAttackType() == RangeType.Melee && extraCells.Count > 0)
            {
                creatureMovement.moveToCellAndAttack(targetCell, extraCells, orientation, mouseOrientation,targetReadyForAttack);
            }
            else if (getAttackType() == RangeType.Ranged && isValidTarget(targetCell.getHexCoordinates(),targetCell.OwnerClientId))
            {
                targetReadyForAttack(targetCell);
            }
        }
        else
        {
            creatureMovement.moveToCell(targetCell, extraCells, orientation);
        }
    }

    public void targetReadyForAttack(PermanentCell target)
    {
        switch (getAttackType())
        {
            case RangeType.Melee:
                attackTargetMelee(target);
                break;
            case RangeType.Ranged:
                attackTargetRange(target);
                break;
        }
    }

    public bool isValidTarget(HexCoordinates coords,ulong ownerId)
    {
        if(ownerId == OwnerClientId)
        {
            //Debug.Log("INVALID TARGET AS ITS OUR TEAM");
        }

        int distanceToTarget = HexUtility.Distance(creatureMovement.currentCoordinates(), coords);
        if(distanceToTarget <= range.Value || getAttackType() == RangeType.Melee)
        {
            return true;
        }
        else
        {
            //Debug.Log("INVALID TARGET OUT OF RANGE");
        }
        
        return false;
    }

    public OnHoverOverSelectableResponse mouseAttackHover(HexDirection mouseOrientation)
    {
        if(getAttackType() == RangeType.Melee)
        {
            if (creatureMovement.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Point)
            {
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, 1, CellHelper.getOppositeOfDirection(mouseOrientation)));
            }
            else if (creatureMovement.getHexSpaceType() == CreatureStats.CreatureHexSpaces.Line)
            {
                //note the +1 to distance since we start counting from the target, not the move position
                return new OnHoverOverSelectableResponse(mouseTextureDirectionMapping[mouseOrientation], new SelectableHexArea(SelectableHexAreaType.Line, creatureMovement.hexSpaceDistance.Value + 1, CellHelper.getOppositeOfDirection(mouseOrientation)));
            }
        }else if(getAttackType() == RangeType.Ranged)
        {
            return new OnHoverOverSelectableResponse(rangedAttackTexture, new SelectableHexArea(SelectableHexAreaType.Point,0,HexDirection.NONE));
        }

        return null;
    }

    private void attackTargetMelee(PermanentCell target)
    {
        damageTarget(target);
    }

    public void attackTargetRange(PermanentCell target)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        projectile.GetComponent<TargetProjectile>().UpdateTarget(target.getAttachedPermanent().transform, Vector3.zero, damageTarget, target);
        spawnProjectileClientRpc(target.getAttachedPermanent().GetComponent<NetworkObject>());
    }

    [ClientRpc]
    public void spawnProjectileClientRpc(NetworkObjectReference target)
    {
        if (!IsServer)
        {
            if(projectilePrefab == null)
            {
                projectilePrefab = GetComponent<Creature>().getCreatureObject().GetComponent<CreatureStats>().projectilePrefab;
            }

            if (target.TryGet(out NetworkObject targetObject))
            {
                GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                projectile.GetComponent<TargetProjectile>().UpdateTarget(targetObject.transform, Vector3.zero);
            }
        }
    }

    void damageTarget(PermanentCell target)
    {
        target.getAttachedPermanent().permanentAttacked(GetComponent<NetworkObject>(), Permanent.Type.Creature);
    }

    public RangeType getAttackType()
    {
        RangeType.TryParse(rangeType.Value.ToString(), out RangeType result);
        return result;
    }
}
