using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LayerMaskHelper : MonoBehaviour
{
    /// <summary>
    /// Returns true if the gameObject's layer is contained within the layerMask's layers.
    /// </summary>
    /// <param name="gameObject">The GameObject we are comparing against the LayerMask.</param>
    /// <param name="layerMask">The LayerMask we are checking if the GameObject is within.</param>
    /// <returns>bool</returns>
    public static bool ObjIsInLayerMask(GameObject gameObject, LayerMask layerMask)
    {
        if ((layerMask.value & (1 << gameObject.layer)) > 0)
        {
            return true;
        }

        return false;
    }
    /// <summary>
    /// Returns a LayerMask with the specified layers.
    /// <param name="layers"></param>
    /// <returns>LayerMask</returns> 
    /// </summary>

    public static LayerMask CreateLayerMask(params int[] layers)
    {
        LayerMask layerMask = 0;
        foreach (int layer in layers)
        {
            layerMask |= (1 << layer);
        }

        return layerMask;
    }
}
