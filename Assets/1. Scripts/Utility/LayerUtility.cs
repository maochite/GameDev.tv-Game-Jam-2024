using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum LayerEnum
{
    Ground = 1 << 0,
    Destructible = 1 << 1,
    Indestructible = 1 << 2,
    Player = 1 << 3,
    Enemy = 1 << 4,
    LineOfSight = 1 << 5,
}

[System.Flags]
public enum HitLayer
{
    Ground = 1 << 0,
    Destructible = 1 << 1,
    Indestructible = 1 << 2,
    Player = 1 << 3,
    Enemy = 1 << 4,
}

[System.Flags]
public enum CollisionLayer
{
    Ground = 1 << 0,
    Destructible = 1 << 1,
    Indestructible = 1 << 2,
    Player = 1 << 3,
    Enemy = 1 << 4,
}

public static class LayerUtility
{

    private static readonly Dictionary<LayerEnum, LayerMask> maskDict = new();
    private static readonly Dictionary<LayerEnum, int> layerDict = new();

    static LayerUtility()
    {
        foreach (LayerEnum layer in Enum.GetValues(typeof(LayerEnum)))
        {
            maskDict[layer] = LayerMask.GetMask(layer.ToString());
            layerDict[layer] = LayerMask.NameToLayer(layer.ToString());
        }
    }

    public static LayerMask LayerMaskByLayerEnumType(LayerEnum colliderTypeFlags)
    {
        LayerMask mask = 0;

        foreach (LayerEnum layer in System.Enum.GetValues(typeof(LayerEnum)))
        {
            if (colliderTypeFlags.HasFlag(layer))
            {
                mask |= maskDict[layer];
            }
        }

        return mask;
    }

    //Think this works? May need to double check
    public static T SetAllBits<T>() where T : Enum
    {
        int newValue = 0;

        foreach (T flag in Enum.GetValues(typeof(T)))
        {
            int flagValue = Convert.ToInt32(flag);
            newValue |= flagValue;
        }

        return (T)Enum.ToObject(typeof(T), newValue);
    }


    public static int LayerByLayerEnumType(LayerEnum layerEnum)
    {
        if (layerDict.ContainsKey(layerEnum))
        {
            return layerDict[layerEnum];
        }

        Debug.LogWarning("Invalid layer enum value: " + layerEnum);
        return 0;
    }

    public static LayerMask LayerToLayerMask(int layer)
    {
        return 1 << layer;
    }

    public static LayerMask CombineMasks(LayerMask mask1, LayerMask mask2)
    {
        return mask1 | mask2;
    }

    public static void RemoveMaskFromMask(ref LayerMask mask, LayerMask layerToRemove)
    {
        mask &= ~(1 << layerToRemove);
    }

    public static bool CheckMaskOverlap(LayerMask mask1, LayerMask mask2)
    {
        return (mask1 & mask2) != 0;
    }

}
