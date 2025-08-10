using UnityEngine;

public class CommandMenu : MonoBehaviour
{
    [SerializeField] GameObject panel;

    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
