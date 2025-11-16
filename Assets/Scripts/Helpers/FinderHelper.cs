using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// *SUMMARY
/// The FinderHelper class is responsible for finding components on objects in the scene.
/// The class includes functionality to find components on a specific object, its parent, or its children.
/// The class is typically used to find components on objects when the object is not directly accessible.
/// </summary> <summary>
/// 
/// </summary>

public class FinderHelper : MonoBehaviour
{
   /// <summary>
   /// *Finds a component of type T on the specified GameObject, its parent, or its children.
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="gameObject"></param>
   /// <returns></returns> <summary>
   /// </summary>
   public static T GetComponentOnObject<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        component = gameObject.GetComponentInParent<T>();
        if (component != null)
        {
            return component;
        }

        component = gameObject.GetComponentInChildren<T>();
        if (component != null)
        {
            return component;
        }

        return null;
    }
}
