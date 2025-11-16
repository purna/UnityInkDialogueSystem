using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private GameObject shieldObject;
       [SerializeField] private Transform shieldSpawnPosition;
    [SerializeField] private float timeForShield = 2f;
    [SerializeField] private  PlayerUpgrades playerUpgrades;

    [SerializeField] public bool IsActive = false;

    public bool ShouldBeProtecting { get; private set; } = false;

    


    private void Update()
    {

        if (playerUpgrades.ShieldUpgradeUnlocked == true)
        {
             // Check if the E key was pressed this frame
            if  (Keyboard.current.eKey.wasPressedThisFrame)
            {
               
               playerUpgrades.UnlockShield();
               SpawnShield();
               playerUpgrades.LockShield();

            }
       
        }
    }

        private void SpawnShield()
    {
        // Instantiate the bomb prefab at the spawn position
        if (shieldObject != null && shieldSpawnPosition != null)
        {

            StartCoroutine(ProtectPlayer());
            
        }
        else
        {
            Debug.LogWarning("Shield prefab or spawn position is not assigned.");
        }
    }

 public IEnumerator ProtectPlayer()
{
    GameObject spawnedShieldInstance = Instantiate(shieldObject, shieldSpawnPosition.position, shieldSpawnPosition.rotation);

    ShouldBeProtecting = true;

    float _elapsedTime = 0f;
    while (_elapsedTime < timeForShield)
    {
        _elapsedTime += Time.fixedDeltaTime;
        yield return new WaitForFixedUpdate();
    }

    ShouldBeProtecting = false;

    if (spawnedShieldInstance != null)
    {
        Destroy(spawnedShieldInstance); // Destroy the instance, not the prefab
    }

    yield return null;
}


    public void SetShieldObject(GameObject collectedObject)
    {
        shieldObject = collectedObject;
    }
}
