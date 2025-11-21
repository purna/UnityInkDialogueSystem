using UnityEngine;
using Core.Game;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(CollectableTriggerHandler))]
public class EnhancedCollectable : MonoBehaviour
{
    [SerializeField] private CollectableSOBase _collectable;
    
    [Header("Skill Requirements")]
    [SerializeField] private bool requiresSkill = false;
    [SerializeField] private Skill requiredSkill;
    [SerializeField] private string requiredSkillName; // Alternative to reference
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color lockedColor = Color.gray;
    private Color originalColor;

    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            UpdateVisuals();
        }
    }

    public bool CanCollect()
    {
        if (!requiresSkill)
            return true;

        // Check by reference
        if (requiredSkill != null)
        {
            return requiredSkill.IsUnlocked;
        }

        // Check by name using SkillTreeManager
        if (!string.IsNullOrEmpty(requiredSkillName) && SkillTreeManager.Instance != null)
        {
            var skill = SkillTreeManager.Instance.SkillTreeContainer?.GetSkillByName(requiredSkillName);
            return skill != null && skill.IsUnlocked;
        }

        return true;
    }

    public void Collect(GameObject objectThatCollected)
    {
        if (CanCollect())
        {
            _collectable.Collect(objectThatCollected);
        }
        else
        {
            ShowCannotCollectMessage();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        if (requiresSkill && !CanCollect())
        {
            spriteRenderer.color = lockedColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void ShowCannotCollectMessage()
    {
        string skillName = requiredSkill != null ? requiredSkill.SkillName : requiredSkillName;
        Debug.Log($"Cannot collect this item. Requires skill: {skillName}");
        // Show UI message to player
    }
}
