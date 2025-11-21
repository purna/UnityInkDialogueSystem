using UnityEngine;
using TMPro;
using Core.Game;

public class SkillGatedChest : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private Skill requiredSkill;
    [SerializeField] private string requiredSkillName;

    [Header("Rewards")]
    [SerializeField] private GameObject[] collectableRewards;
    [SerializeField] private int skillPointReward = 5;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private float promptDisplayTime = 2f;

    private bool hasBeenOpened = false;

    private void Start()
    {
        UpdateVisual();
        
        // Hide prompt at start
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenOpened || !other.CompareTag("Player"))
            return;

        if (CanOpen())
        {
            OpenChest();
        }
        else
        {
            ShowLockedMessage();
        }
    }

    private bool CanOpen()
    {
        if (requiredSkill != null)
        {
            return requiredSkill.IsUnlocked;
        }

        if (!string.IsNullOrEmpty(requiredSkillName) && SkillTreeManager.Instance != null)
        {
            var skill = SkillTreeManager.Instance.SkillTreeContainer?.GetSkillByName(requiredSkillName);
            return skill != null && skill.IsUnlocked;
        }

        return true;
    }

    private void OpenChest()
    {
        hasBeenOpened = true;

        // Spawn collectable rewards
        foreach (var reward in collectableRewards)
        {
            Instantiate(reward, transform.position + Vector3.up, Quaternion.identity);
        }

        // Grant skill points
        if (skillPointReward > 0 && SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.AddSkillPoints(skillPointReward);
        }

        UpdateVisual();
        Debug.Log("Chest opened!");
    }

    private void UpdateVisual()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = hasBeenOpened ? unlockedSprite : lockedSprite;
        }
    }

    private void ShowLockedMessage()
    {
        string skillName = requiredSkill != null ? requiredSkill.SkillName : requiredSkillName;
        string message = $"This chest requires '{skillName}' skill to open!";

        Debug.Log(message);
        
        // Show UI message to player
        if (promptObject != null && promptText != null)
        {
            promptObject.SetActive(true);
            promptText.text = message;
            
            // Auto-hide after delay
            Invoke(nameof(HidePrompt), promptDisplayTime);
        }
    }

    private void HidePrompt()
    {
        if (promptObject != null)
        {
            promptObject.SetActive(false);
        }
    }
}