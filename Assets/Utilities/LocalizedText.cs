using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour {
    [SerializeField] private string _table;
    [SerializeField] private bool _isLogging;
    protected TextMeshProUGUI _text;

    protected string _key;
    protected readonly Dictionary<string, string> _arguments = new();

    public string Table => _table;

    protected void Start() {
        UpdateText();
        LocalizationSettings.SelectedLocaleChanged += delegate { UpdateText(); };
    }

    public virtual void SetText(string text) {
        _key = text;
        UpdateText();
    }

    public void SetColor(Color color) {
        CheckText();
        _text.color = color;
    }

    public virtual void UpdateText() {
        CheckText();
        _text.text = LocalizationSettings.StringDatabase.GetLocalizedString(_table, _key, arguments: _arguments);
    }

    protected void CheckText() {
        if (_text != null) return;

        _text = GetComponent<TextMeshProUGUI>();
        if (string.IsNullOrEmpty(_key))
            _key = _text.text;
    }

    public void AddArguments(string name, string variable) {
        _arguments[name] = variable;
    }

    public void ClearArguments() {
        _arguments.Clear();
    }

    public void SetTable(string table) {
        _table = table;
    }
}
