using UnityEngine;

public class UpgradeUpgradesManager : MonoBehaviour
{
    private PlayerUpgrades playerUpgrades;

    private void Start()
    {
        playerUpgrades = FindObjectOfType<PlayerUpgrades>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            playerUpgrades.UnlockBomb();
            Debug.Log("Bomb upgrade unlocked!");
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            playerUpgrades.UnlockInvisibility();
            Debug.Log("Invisibility upgrade unlocked!");
        }
    }
}