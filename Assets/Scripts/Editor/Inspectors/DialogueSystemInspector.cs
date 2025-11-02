using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(DialogueChoicer))]
public class DialogueSystemInspector : Editor {
    private SerializedProperty _dialogueManager;
    private SerializedProperty _dialogueContainer;
    private SerializedProperty _dialogueGroup;
    private SerializedProperty _dialogue;

    private SerializedProperty _isGroupedDialogues;
    private SerializedProperty _isStartingDialogues;

    private SerializedProperty _selectedDialogueGroupIndex;
    private SerializedProperty _selectedDialogueIndex;

    private void OnEnable() {
        _dialogueContainer = serializedObject.FindProperty(nameof(_dialogueContainer));
        _dialogueManager = serializedObject.FindProperty(nameof(_dialogueManager));
        _dialogueGroup = serializedObject.FindProperty(nameof(_dialogueGroup));
        _dialogue = serializedObject.FindProperty(nameof(_dialogue));

        _isGroupedDialogues = serializedObject.FindProperty(nameof(_isGroupedDialogues));
        _isStartingDialogues = serializedObject.FindProperty(nameof(_isStartingDialogues));

        _selectedDialogueGroupIndex = serializedObject.FindProperty(nameof(_selectedDialogueGroupIndex));
        _selectedDialogueIndex = serializedObject.FindProperty(nameof(_selectedDialogueIndex));
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        _dialogueManager.DrawPropertyField();

        DrawDialogueContainerArea();
        DialogueContainer dialogueContainer = _dialogueContainer.objectReferenceValue as DialogueContainer;
        if (dialogueContainer == null) {
            StopDrawing("Select Dialogue Container to contunie.");
            return;
        }

        DrawFiltersArea();

        List<string> dialogueNames;
        string dialogueFolderPath = $"Assets/_Project/ScriptableObjects/Dialogues/{dialogueContainer.FileName}";
        string dialogueInfoMessage;

        if (_isGroupedDialogues.boolValue) {
            if (!dialogueContainer.HaveGroups()) {
                StopDrawing("There are no dialogue groups in this container.");
                return;
            }

            DrawDialogueGroupArea(dialogueContainer);

            DialogueGroup dialogueGroup = _dialogueGroup.objectReferenceValue as DialogueGroup;
            dialogueNames = dialogueContainer.GetGroupedDialoguesNames(dialogueGroup, _isStartingDialogues.boolValue);
            dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";
            dialogueInfoMessage = "There are no " + (_isStartingDialogues.boolValue ? "Starting" : "") + " Dialouges in this Dialogue Group";
        } else {
            dialogueNames = dialogueContainer.GetUngroupedDialoguesNames(_isStartingDialogues.boolValue);
            dialogueFolderPath += $"/Global/Dialogues";
            dialogueInfoMessage = "There are no " + (_isStartingDialogues.boolValue ? "Starting" : "") + " Ungrouped Dialouges in this Dialogue Container";
        }

        if (dialogueNames.Count == 0) {
            StopDrawing(dialogueInfoMessage);
            return;
        }

        DrawDialogueArea(dialogueNames, dialogueFolderPath);

        serializedObject.ApplyModifiedProperties();
    }

    #region Draw
    private void DrawDialogueContainerArea() {
        InspectorUtility.DrawHeader("Dialogue Container");
        _dialogueContainer.DrawPropertyField();
        InspectorUtility.DrawSpace();
    }

    private void DrawFiltersArea() {
        InspectorUtility.DrawHeader("Filters");
        _isGroupedDialogues.DrawPropertyField();
        _isStartingDialogues.DrawPropertyField();
        InspectorUtility.DrawSpace();
    }

    private void DrawDialogueGroupArea(DialogueContainer dialogueContainer) {
        InspectorUtility.DrawHeader("Dialogue Group");
        string[] groupNames = dialogueContainer.GetGroupsNames();

        int oldGroupIndex = _selectedDialogueGroupIndex.intValue;
        DialogueGroup oldGroup = _dialogueGroup.objectReferenceValue as DialogueGroup;
        bool isOldGroupNull = oldGroup == null;
        string oldGroupName = isOldGroupNull ? string.Empty : oldGroup.GroupName;
        UpdatePropertyIndex(groupNames, _selectedDialogueGroupIndex, oldGroupIndex, isOldGroupNull, oldGroupName);

        _selectedDialogueGroupIndex.intValue = InspectorUtility.DrawPopup("Dialogoue Group", _selectedDialogueGroupIndex, groupNames);

        string selectedDialogueGroupName = groupNames[_selectedDialogueGroupIndex.intValue];
        DialogueGroup selectedDialogueGroup = AssetsUtility.LoadAsset<DialogueGroup>($"Assets/_Project/ScriptableObjects/Dialogues/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);


        _dialogueGroup.objectReferenceValue = selectedDialogueGroup;
        InspectorUtility.DrawDisabledField(() => _dialogueGroup.DrawPropertyField());
        InspectorUtility.DrawSpace();
    }

    private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath) {
        InspectorUtility.DrawHeader("Dialogue");

        int oldDialogueIndex = _selectedDialogueIndex.intValue;
        Dialogue oldDialogue = _dialogue.objectReferenceValue as Dialogue;
        bool isOldDialogueNull = oldDialogue == null;
        string oldDialogueName = isOldDialogueNull ? string.Empty : oldDialogue.Name;
        UpdatePropertyIndex(dialogueNames.ToArray(), _selectedDialogueIndex, oldDialogueIndex, isOldDialogueNull, oldDialogueName);

        _selectedDialogueIndex.intValue = InspectorUtility.DrawPopup("Dialogue", _selectedDialogueIndex, dialogueNames.ToArray());

        string dialogueName = dialogueNames[_selectedDialogueIndex.intValue];
        Dialogue dialogue = AssetsUtility.LoadAsset<Dialogue>(dialogueFolderPath, dialogueName);
        _dialogue.objectReferenceValue = dialogue;

        InspectorUtility.DrawDisabledField(() => _dialogue.DrawPropertyField());
    }

    private void StopDrawing(string reason) {
        InspectorUtility.DrawHelpBox(reason);
        InspectorUtility.DrawSpace();
        InspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly/", MessageType.Warning);
        serializedObject.ApplyModifiedProperties();
    }
    #endregion

    #region IndexMethods
    private void UpdatePropertyIndex(string[] names, SerializedProperty property, int oldPropertyIndex, bool isOldNameNull, string oldName) {
        if (isOldNameNull) {
            property.intValue = 0;
            return;
        }

        if (oldPropertyIndex < names.Length && oldName == names[oldPropertyIndex])
            return;

        if (names.Contains(oldName))
            property.intValue = Array.IndexOf(names, oldName);
        else
            property.intValue = 0;
    }
    #endregion
}
