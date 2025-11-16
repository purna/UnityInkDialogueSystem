using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(CollectableTriggerHandler))]

public class Collectable : MonoBehaviour
{
    [SerializeField] private CollectableSOBase _collectable;

    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    public void Collect(GameObject objectThatCollected)
    {
        _collectable.Collect(objectThatCollected);
    }

       public void Use(GameObject objectThatCollected)
    {
        _collectable.Collect(objectThatCollected);
    }



}
