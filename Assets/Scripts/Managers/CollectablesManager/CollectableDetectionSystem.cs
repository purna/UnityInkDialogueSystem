using UnityEngine;
using System.Collections.Generic;
using Core.Game;
public class CollectableDetectionSystem : MonoBehaviour
{
    public static CollectableDetectionSystem Instance { get; private set; }

    [SerializeField] private GameObject detectionIndicatorPrefab;
    [SerializeField] private float updateInterval = 0.5f;

    private bool detectionEnabled = false;
    private float detectionRadius = 10f;
    private bool showOnMinimap = false;
    private Transform playerTransform;
    
    private List<GameObject> activeIndicators = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Find player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        if (detectionEnabled)
        {
            InvokeRepeating(nameof(UpdateDetection), 0f, updateInterval);
        }
    }

    public void EnableDetection(float radius, bool onMinimap)
    {
        detectionEnabled = true;
        detectionRadius = radius;
        showOnMinimap = onMinimap;
        
        if (!IsInvoking(nameof(UpdateDetection)))
        {
            InvokeRepeating(nameof(UpdateDetection), 0f, updateInterval);
        }
    }

    public void DisableDetection()
    {
        detectionEnabled = false;
        CancelInvoke(nameof(UpdateDetection));
        ClearIndicators();
    }

    private void UpdateDetection()
    {
        if (!detectionEnabled || playerTransform == null)
            return;

        ClearIndicators();

        // Find all collectables within range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(playerTransform.position, detectionRadius);
        
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<Collectable>() != null)
            {
                CreateIndicator(collider.transform.position);
            }
        }
    }

    private void CreateIndicator(Vector3 position)
    {
        if (detectionIndicatorPrefab != null)
        {
            GameObject indicator = Instantiate(detectionIndicatorPrefab, position, Quaternion.identity);
            activeIndicators.Add(indicator);
        }
    }

    private void ClearIndicators()
    {
        foreach (var indicator in activeIndicators)
        {
            if (indicator != null)
                Destroy(indicator);
        }
        activeIndicators.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (detectionEnabled && playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, detectionRadius);
        }
    }
}
