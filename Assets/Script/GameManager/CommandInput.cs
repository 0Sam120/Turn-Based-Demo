using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CommandInput : MonoBehaviour
{
    // References to other components
    SelectCharacter selectedCharacter;
    [SerializeField] CommandType currentCommand;

    CommandManager commandManager;
    CursorData cursorData;
    MoveUnit moveUnit;
    CharacterAttack characterAttack;
    MouseInput mouseInput;

    private void Awake()
    {
        currentCommand = CommandType.Default;

        // Initialize input system
        mouseInput = new MouseInput();

        // Get component references
        commandManager = GetComponent<CommandManager>();
        cursorData = GetComponent<CursorData>();
        moveUnit = GetComponent<MoveUnit>();
        characterAttack = GetComponent<CharacterAttack>();
        selectedCharacter = GetComponent<SelectCharacter>();
    }

    private void OnEnable()
    {
        // Enable input
        mouseInput.Enable();

        // Register input callbacks
        mouseInput.UnitCommand.ConfirmAction.performed += HandleLeftClick;
        mouseInput.UnitControl.Deselect.performed += HandleRightClick;
    }

    private void OnDisable()
    {
        // Disable input
        mouseInput.Disable();

        // Unregister input callbacks
        mouseInput.UnitCommand.ConfirmAction.performed -= HandleLeftClick;
    }

    // Sets the current command type
    public void SetCommandType(CommandType commandType)
    {
        currentCommand = commandType;
    }

    // Initializes the command by highlighting valid areas or targets
    public void InitCommand()
    {
        switch (currentCommand)
        {
            case CommandType.MoveTo:
                HighlightWalkableTerrain();
                break;
            case CommandType.Attack:
                characterAttack.CalculateAttackArea(
                    selectedCharacter.selected.GetComponent<GridObject>().positionOnGrid,
                    selectedCharacter.selected.atkRange);
                break;
        }
    }

    // Handles left-click input
    private void HandleLeftClick(InputAction.CallbackContext input)
    {
        switch (currentCommand)
        {
            case CommandType.Default:
                selectedCharacter.Select();
                break;
            case CommandType.MoveTo:
                MoveCommand();
                break;
            case CommandType.Attack:
                AttackCommand();
                break;
        }
    }

    // Handles right-click input
    private void HandleRightClick(InputAction.CallbackContext input)
    {
        selectedCharacter.Deselect();
    }

    // Highlights tiles that are walkable for the selected character
    public void HighlightWalkableTerrain()
    {
        moveUnit.CheckWalkableTerrain(selectedCharacter.selected);
    }

    // Processes the attack command
    private void AttackCommand()
    {
        GridObject gridObject = characterAttack.GetAttackTarget(cursorData.positionOnGrid);
        if (gridObject == null) { return; }

        commandManager.AddAttackCommand(selectedCharacter.selected, cursorData.positionOnGrid, gridObject);
        commandManager.ExecuteCommand();
        currentCommand = CommandType.Default; // Reset command after execution
    }

    // Processes the move command
    private void MoveCommand()
    {
        Vector2Int startPos;
        if (selectedCharacter == null)
        {
            Debug.LogError("No character selected for movement.");
            return;
        }

        startPos = selectedCharacter.selected.GetComponent<GridObject>().positionOnGrid;

        List<Vector2Int> path = moveUnit.GetPath(startPos, cursorData.positionOnGrid);

        commandManager.AddMoveCommand(selectedCharacter.selected, cursorData.positionOnGrid, path);
        commandManager.ExecuteCommand();
        currentCommand = CommandType.Default; // Reset command after execution
    }
}
