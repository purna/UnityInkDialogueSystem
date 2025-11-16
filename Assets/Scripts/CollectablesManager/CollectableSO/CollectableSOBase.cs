using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectableSOBase : ScriptableObject
{
    [Header("UI")]
    public string ItemName;
    public Sprite ItemIcon;
    [TextArea(4, 8)]
    public string ItemDescription;

    [Header("Collection Effects")]
    public Color CollectColor;
    public float CollectionFlashTime = 0.5f;
    public AudioClip CollectionClip;

    public float CollectionDuration = 1f;

    protected PlayerEffects _playerEffects;

    public abstract void Collect(GameObject objectThatCollected);

    public void GetReference(GameObject objectThatCollected)
    {
        _playerEffects = FinderHelper.GetComponentOnObject<PlayerEffects>(objectThatCollected);
    }
}
