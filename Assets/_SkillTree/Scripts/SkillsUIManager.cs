using System.Collections.Generic;
using UnityEngine;
using Core.Game;

public class SkillsUIManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private SkillsUI skillsUI;

    [Header("Collected Skills (Inspector Preview)")]
    [SerializeField] private List<Skill> collectedSkills = new();


    private void HandleSkillsUpdate(List<Skill> items)
    {
        collectedSkills.Clear();

        // Filter only skills
        foreach (var item in items)
        {
            if (item is Skill skill)
                collectedSkills.Add(skill);
        }

        // Build count dictionary
        if (skillsUI != null)
        {
            var skillCounts = new Dictionary<ScriptableObject, int>();

            foreach (var skill in collectedSkills)
            {
                if (skillCounts.ContainsKey(skill))
                    skillCounts[skill]++;
                else
                    skillCounts[skill] = 1;
            }

            skillsUI.UpdateUI(skillCounts);
        }
    }
}
