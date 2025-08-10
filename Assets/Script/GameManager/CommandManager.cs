using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Defines the types of commands that can be executed
public enum CommandType
{
    Default,
    MoveTo,
    Attack
}

// Represents a command to be executed by a character
public class Command
{
    public Character character;              // The character performing the command
    public Vector2Int selectedGrid;          // Target grid position for the command
    public CommandType type;                 // Type of command (Move or Attack)

    public Command(Character character, Vector2Int selectedGrid, CommandType type)
    {
        this.character = character;
        this.selectedGrid = selectedGrid;
        this.type = type;
    }

    public List<Vector2Int> path;              // Path to follow (for MoveTo)
    public GridObject target;                // Target object (for Attack)
}

// Handles execution of move and attack commands
public class CommandManager : MonoBehaviour
{
    public Command currentCommand;           // Currently active command
    ClearUtility clearUtility;

    private void Awake()
    {
        clearUtility = GetComponent<ClearUtility>();
    }


    // Executes the current command based on its type
    public void ExecuteCommand()
    {
        switch (currentCommand.type)
        {
            case CommandType.MoveTo:
                ExecuteMoveCommand();
                break;
            case CommandType.Attack:
                ExecuteAttackCommand();
                break;
        }
    }

    // Executes an attack command
    public void ExecuteAttackCommand()
    {
        Debug.Log("Shoot");
        Character receiver = currentCommand.character;

        if (!receiver.GetComponent<CharacterTurn>().SpendMomentum(2))
        {
            return;
        }

        clearUtility.FullClear();
        int total = receiver.RollToHit();
        receiver.GetComponent<AttackComponent>().AttackPosition(currentCommand.target, total);
        currentCommand = null;
    }

    // Executes a move command
    public void ExecuteMoveCommand()
    {
        Character receiver = currentCommand.character;
        var characterTurn = receiver.GetComponent<CharacterTurn>();

        // Check both requirements BEFORE executing anything
        bool canSpend = characterTurn.CanSpendMomentum(2);
        bool pathIsValid = receiver.GetComponent<UnitMovement>().PathIsValid(currentCommand.path);

        if (!canSpend || !pathIsValid)
        {
            return; // Abort if either fails
        }

        // Now we can spend momentum and move
        characterTurn.SpendMomentum(2);
        clearUtility.FullClear(); // Clear any previous highlights
        receiver.GetComponent<UnitMovement>().Move(currentCommand.path);

        currentCommand = null;
    }

    // Sets up a move command with path information
    public void AddMoveCommand(Character character, Vector2Int selectedGrid, List<Vector2Int> path)
    {
        currentCommand = new Command(character, selectedGrid, CommandType.MoveTo);
        currentCommand.path = path;
    }

    // Sets up an attack command with a target
    public void AddAttackCommand(Character attacker, Vector2Int selectGrid, GridObject target)
    {
        currentCommand = new Command(attacker, selectGrid, CommandType.Attack);
        if (target == null) { return; }
        currentCommand.target = target;
    }
}
