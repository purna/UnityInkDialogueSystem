
// ===== 4. SKILL TREE UNLOCK MANAGER =====
using UnityEngine;
using System.Collections.Generic;

namespace Core.Game
{
    public class SkillTreeUnlockManager : MonoBehaviour
    {
        public static SkillTreeUnlockManager Instance { get; private set; }

        [Header("Skill Tree Container")]
        [SerializeField] private SkillsTreeContainer skillTreeContainer;

        private HashSet<string> unlockedBranches = new HashSet<string>();

        public event System.Action<SkillsTreeGroup> OnSkillBranchUnlocked;

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
        }

        public void SetSkillTreeContainer(SkillsTreeContainer container)
        {
            skillTreeContainer = container;
        }

        public bool UnlockSkillBranch(SkillsTreeGroup group)
        {
            if (group == null) return false;

            string groupName = group.GroupName;
            if (unlockedBranches.Contains(groupName))
            {
                Debug.Log($"Skill branch '{groupName}' is already unlocked.");
                return false;
            }

            unlockedBranches.Add(groupName);
            OnSkillBranchUnlocked?.Invoke(group);
            Debug.Log($"Unlocked skill branch: {groupName}");
            return true;
        }

        public bool UnlockSkillBranchByName(string branchName)
        {
            if (skillTreeContainer == null)
            {
                Debug.LogError("SkillTreeContainer is not set!");
                return false;
            }

            if (unlockedBranches.Contains(branchName))
            {
                Debug.Log($"Skill branch '{branchName}' is already unlocked.");
                return false;
            }

            // Find the group in the container
            foreach (var group in skillTreeContainer.Groups.Keys)
            {
                if (group.GroupName == branchName)
                {
                    return UnlockSkillBranch(group);
                }
            }

            Debug.LogWarning($"Skill branch '{branchName}' not found in container.");
            return false;
        }

        public bool IsSkillBranchUnlocked(string branchName)
        {
            return unlockedBranches.Contains(branchName);
        }

        public bool IsSkillBranchUnlocked(SkillsTreeGroup group)
        {
            return group != null && unlockedBranches.Contains(group.GroupName);
        }

        public List<string> GetUnlockedBranches()
        {
            return new List<string>(unlockedBranches);
        }
    }
}
