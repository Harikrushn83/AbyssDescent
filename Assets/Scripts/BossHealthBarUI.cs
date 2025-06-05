using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    public Image fillImage;                 // Drag the fill image (BossHealthBarFill)
    public GameObject barRoot;             // Drag the BG parent object (BossHealthBarBG)

    private BossAI boss;

    void Start()
    {
        barRoot.SetActive(false); // Hide by default
    }

    void Update()
    {
        if (boss == null) return;

        float fill = (float)boss.GetCurrentHealth() / boss.GetMaxHealth();
        fillImage.fillAmount = fill;
    }

    public void SetBoss(BossAI newBoss)
    {
        boss = newBoss;
        barRoot.SetActive(true);
    }

    public void ClearBoss()
    {
        boss = null;
        barRoot.SetActive(false);
    }
}
