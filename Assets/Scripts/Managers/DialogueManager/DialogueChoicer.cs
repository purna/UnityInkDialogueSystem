using System;
using UnityEngine;

public class DialogueChoicer : MonoBehaviour {
    [SerializeField] private DialogueManager _dialogueManager;
    [SerializeField] private DialogueContainer _dialogueContainer;
    [SerializeField] private DialogueGroup _dialogueGroup;
    [SerializeField] private Dialogue _dialogue;

    [SerializeField] private bool _isGroupedDialogues;
    [SerializeField] private bool _isStartingDialogues;

    [SerializeField] private int _selectedDialogueGroupIndex;
    [SerializeField] private int _selectedDialogueIndex;

    public Dialogue Dialogue => _dialogue;

    public event Action PartEnded;

    private void Awake() {
        _dialogueManager.DialogueEnded += EndTutorialPart;
    }

    public void StartTutorialPart() {
        _dialogueManager.StartDialogue(this.Dialogue);
    }

    private void EndTutorialPart() {
        PartEnded?.Invoke();
    }
}
