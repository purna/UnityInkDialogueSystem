
using UnityEngine;

namespace Core.Game
{
    public class PlayerSkillTreeManager : MonoBehaviour
    {
        public static PlayerSkillTreeManager Instance { get; private set; }

        [SerializeField] private SkillsTreeContainer skillTreeContainer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (skillTreeContainer != null)
            {
                SkillTreeUnlockManager.Instance?.SetSkillTreeContainer(skillTreeContainer);
            }
        }

        public bool HasSkillUnlocked(string skillName)
        {
            if (skillTreeContainer == null)
                return false;

            Skill skill = skillTreeContainer.GetSkillByName(skillName);
            return skill != null && skill.IsUnlocked;
        }

        public Skill GetSkill(string skillName)
        {
            return skillTreeContainer?.GetSkillByName(skillName);
        }

        public bool TryUnlockSkill(Skill skill)
        {
            if (skill == null)
                return false;

            // Delegate to SkillTreeManager for proper skill unlocking logic
            if (SkillTreeManager.Instance != null)
            {
                return SkillTreeManager.Instance.TryUnlockSkill(skill);
            }

            // Fallback: direct unlock without manager (not recommended)
            Debug.LogWarning("PlayerSkillTreeManager: SkillTreeManager.Instance is null. Consider setting up SkillTreeManager for proper skill management.");
            return false;
        }
    }
}