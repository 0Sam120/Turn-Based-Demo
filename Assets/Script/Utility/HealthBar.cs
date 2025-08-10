using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private TextMeshProUGUI value;
    [SerializeField] private TextMeshProUGUI nameText;

    public void SetName(string name)
    {
        nameText.text = name;
    }

    public void UpdateHealthBar(float currentValue, float maxValue)
    {
        slider.value = currentValue / maxValue;
        value.text = $"{currentValue}/{maxValue}";
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = gameCamera.transform.rotation;
        transform.position = target.position + offset;
    }
}
