using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject skillItemPrefab;
    [SerializeField] private Transform skillListParent;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Detail Panel")]
    [SerializeField] private TMP_Text skillNameText;
    [SerializeField] private TMP_Text skillDescriptionText;
    [SerializeField] private Image skillIcon;

    private int selectedSkillIndex = -1;
    private List<ScriptableObject> currentSkills = new();

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1 : 0;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    public void UpdateUI(Dictionary<ScriptableObject, int> skillCounts)
    {
        // Clear UI
        foreach (Transform child in skillListParent)
            Destroy(child.gameObject);

        currentSkills.Clear();

        int index = 0;
        foreach (var kvp in skillCounts)
        {
            ScriptableObject skill = kvp.Key;
            int count = kvp.Value;

            currentSkills.Add(skill);

            GameObject obj = Instantiate(skillItemPrefab, skillListParent);
            obj.SetActive(true);

            // Setup icon and texts
            SkillsUIItem uiItem = obj.GetComponent<SkillsUIItem>();
            if (uiItem != null)
                uiItem.Initialize(this, skill, count, index);

            index++;
        }

        // Auto-select first
        if (currentSkills.Count > 0)
        {
            selectedSkillIndex = 0;
            UpdateDetails();
            UpdateSelection();
        }
        else
        {
            selectedSkillIndex = -1;
            ClearDetails();
        }
    }

    public void SelectSkill(int index)
    {
        selectedSkillIndex = index;
        UpdateSelection();
        UpdateDetails();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < skillListParent.childCount; i++)
        {
            Image img = skillListParent.GetChild(i).GetComponent<Image>();
            if (img != null)
                img.color = i == selectedSkillIndex ? Color.yellow : Color.white;
        }
    }

    private void UpdateDetails()
    {
        if (selectedSkillIndex < 0 || selectedSkillIndex >= currentSkills.Count)
        {
            ClearDetails();
            return;
        }

        ScriptableObject skillSO = currentSkills[selectedSkillIndex];

        // Pull readable data  
        string name = skillSO.name;
        string description = "No description.";

        Sprite icon = null;

        // Try reading from CollectableSOBase if applicable
        if (skillSO is CollectableSOBase col)
        {
            name = col.ItemName;
            description = col.ItemDescription;
            icon = col.ItemIcon;
        }

        // Apply
        if (skillNameText != null) skillNameText.text = name;
        if (skillDescriptionText != null) skillDescriptionText.text = description;
        
        if (skillIcon != null)
        {
            skillIcon.sprite = icon;
            skillIcon.enabled = icon != null;
        }
    }

    private void ClearDetails()
    {
        if (skillNameText) skillNameText.text = "";
        if (skillDescriptionText) skillDescriptionText.text = "";
        if (skillIcon) skillIcon.enabled = false;
    }
}
