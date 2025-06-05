using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth; // Drag your player GameObject here
    public Image fillImage; // Drag the fill image (HealthBarFill) here

    void Update()
    {
        float fill = (float)playerHealth.GetHealth() / playerHealth.GetMaxHealth();
        fillImage.fillAmount = fill;
    }
}
