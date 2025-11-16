using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableTriggerHandler : MonoBehaviour
{
    // Layer 3 is my player
    [SerializeField] private LayerMask _whoCanCollect = LayerMaskHelper.CreateLayerMask(3);

    private Collectable _collectable;

    private void Awake()
    {
        _collectable = GetComponent<Collectable>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
         //Debug.Log("Triggered: " + collision.gameObject.name);
         //Debug.Log("Player layer :" + collision.gameObject.layer);
         //Debug.Log("Layer: " + _whoCanCollect.value);
        
        
        if (LayerMaskHelper.ObjIsInLayerMask(collision.gameObject, _whoCanCollect) || collision.gameObject.layer == 3)
        {
            
            _collectable.Collect(collision.gameObject);

            Debug.Log("Destroyed" + collision.gameObject);
            Destroy(gameObject);
        }
    }
}
