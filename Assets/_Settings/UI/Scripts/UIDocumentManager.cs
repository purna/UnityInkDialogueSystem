using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Manages visibility and navigation between UI Documents
/// </summary>
public class UIDocumentManager : MonoBehaviour
{
    public static UIDocumentManager Instance { get; private set; }

    [Header("UI Documents")]
    [SerializeField] private List<UIDocumentEntry> _uiDocuments = new List<UIDocumentEntry>();

    [Header("Settings")]
    [SerializeField] private bool _hideAllOnStart = true;

    private Stack<UIDocument> _navigationStack = new Stack<UIDocument>();
    private UIDocument _currentDocument;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (_hideAllOnStart)
        {
            HideAllDocuments();
        }

        // Show any documents marked as visible on start
        foreach (var entry in _uiDocuments)
        {
            if (entry.visibleOnStart && entry.uiDocument != null)
            {
                ShowDocument(entry.uiDocument, false);
            }
        }
    }

    /// <summary>
    /// Show a UI Document and optionally hide the current one
    /// </summary>
    public void ShowDocument(UIDocument documentToShow, bool addToHistory = true)
    {
        if (documentToShow == null) return;

        // Add current document to history before switching
        if (addToHistory && _currentDocument != null && _currentDocument != documentToShow)
        {
            _navigationStack.Push(_currentDocument);
        }

        // Hide current document
        if (_currentDocument != null && _currentDocument != documentToShow)
        {
            HideDocument(_currentDocument);
        }

        // Show new document
        documentToShow.gameObject.SetActive(true);
        var root = documentToShow.rootVisualElement;
        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
        }

        _currentDocument = documentToShow;
    }

    /// <summary>
    /// Hide a specific UI Document
    /// </summary>
    public void HideDocument(UIDocument document)
    {
        if (document == null) return;

        var root = document.rootVisualElement;
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
        document.gameObject.SetActive(false);
    }

    /// <summary>
    /// Go back to the previous UI Document in history
    /// </summary>
    public void ShowPreviousDocument()
    {
        if (_navigationStack.Count > 0)
        {
            var previousDocument = _navigationStack.Pop();
            
            // Hide current without adding to history
            if (_currentDocument != null)
            {
                HideDocument(_currentDocument);
            }

            // Show previous
            _currentDocument = previousDocument;
            _currentDocument.gameObject.SetActive(true);
            var root = _currentDocument.rootVisualElement;
            if (root != null)
            {
                root.style.display = DisplayStyle.Flex;
            }
        }
        else
        {
            Debug.LogWarning("No previous UI Document in navigation history");
        }
    }

    /// <summary>
    /// Clear the navigation history
    /// </summary>
    public void ClearHistory()
    {
        _navigationStack.Clear();
    }

    /// <summary>
    /// Hide all registered UI Documents
    /// </summary>
    public void HideAllDocuments()
    {
        foreach (var entry in _uiDocuments)
        {
            if (entry.uiDocument != null)
            {
                HideDocument(entry.uiDocument);
            }
        }
    }

    /// <summary>
    /// Check if there's a previous document to go back to
    /// </summary>
    public bool CanGoBack()
    {
        return _navigationStack.Count > 0;
    }

    [System.Serializable]
    public class UIDocumentEntry
    {
        public string name;
        public UIDocument uiDocument;
        public bool visibleOnStart = false;
    }
}