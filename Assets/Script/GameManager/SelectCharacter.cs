using UnityEngine;

public class SelectCharacter : MonoBehaviour
{
    // Component references
    CursorData cursorData;
    CommandMenu menu;
    CommandInput input;
    ClearUtility clearUtility;

    private void Awake()
    {
        cursorData = GetComponent<CursorData>();
        menu = GetComponent<CommandMenu>();
        input = GetComponent<CommandInput>();
        clearUtility = GetComponent<ClearUtility>();
    }

    // Currently selected character
    public Character selected;

    // Character currently hovered over
    public Character hoverOverCharacter;

    // Grid object currently hovered over
    GridObject hoverOverGridObject;

    // Tracks the last grid position the cursor was on
    Vector2Int positionOnGrid = new Vector2Int(-1, -1);

    // Reference to the target grid map
    [SerializeField] GridMap targetGrid;

    private void Update()
    {
        // Check if cursor moved to a different grid position
        if (positionOnGrid != cursorData.positionOnGrid)
        {
            // Update current cursor position
            positionOnGrid = cursorData.positionOnGrid;

            // Get grid object at the new position
            hoverOverGridObject = targetGrid.GetPlacedObject(positionOnGrid);

            // Check if the grid object contains a Character component
            if (hoverOverGridObject != null)
            {
                hoverOverCharacter = hoverOverGridObject.GetComponent<Character>();
            }
            else
            {
                hoverOverCharacter = null;
            }
        }
    }

    // Called when Move command is selected from menu
    public void MoveCommandSelected()
    {
        clearUtility.ClearGridHighlightAttack();
        input.SetCommandType(CommandType.MoveTo);
        input.InitCommand();
    }

    // Called when Attack command is selected from menu
    public void AttackCommandSelected()
    {
        clearUtility.ClearGridHighlightMove();
        input.SetCommandType(CommandType.Attack);
        input.InitCommand();
    }

    // Opens or closes the command menu based on selection state
    private void UpdateMenu()
    {
        if (selected != null)
        {
            menu.OpenPanel();
        }
        else
        {
            menu.ClosePanel();
        }
    }

    // Selects the character currently hovered over
    public void Select()
    {
        if (hoverOverCharacter == null) { return; }

        selected = hoverOverCharacter;
        UpdateMenu();
    }

    // Deselects the current character and clears highlights
    public void Deselect()
    {
        selected = null;
        clearUtility.FullClear();
        UpdateMenu();
        input.SetCommandType(CommandType.Default);
    }
}
