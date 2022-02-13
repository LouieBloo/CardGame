using HexMapTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellHelper
{
    public static int rotateAmountFromDirection(string orientation)
    {
        switch (orientation)
        {
            case "W":
                return -90;
            case "NW":
                return -45;
            case "NE":
                return 45;
            case "E":
                return 90;
            case "SE":
                return 135;
            case "SW":
                return 225;
        }

        return 0;
    }

    public static HexDirection nextDirectionWhenRotated(HexDirection currentOrientation)
    {
        HexDirection newOrientation = HexDirection.E;
        switch (currentOrientation)
        {
            case HexDirection.E:
                newOrientation = HexDirection.SE;
                break;
            case HexDirection.SE:
                newOrientation = HexDirection.SW;
                break;
            case HexDirection.SW:
                newOrientation = HexDirection.W;
                break;
            case HexDirection.W:
                newOrientation = HexDirection.NW;
                break;
            case HexDirection.NW:
                newOrientation = HexDirection.NE;
                break;
            case HexDirection.NE:
                newOrientation = HexDirection.E;
                break;
        }

        return newOrientation;
    }

    public static HexDirection getOppositeOfDirection(HexDirection input)
    {
        switch (input)
        {
            case HexDirection.E:
                return HexDirection.W;
            case HexDirection.SE:
                return HexDirection.NW;
            case HexDirection.SW:
                return HexDirection.NE;
            case HexDirection.W:
                return HexDirection.E;
            case HexDirection.NW:
                return HexDirection.SE;
            case HexDirection.NE:
                return HexDirection.SW;
        }

        return input;
    }

    public static HexDirection getDirectionFromString(string input)
    {
        HexDirection.TryParse(input, out HexDirection result);
        return result;
    }
}
