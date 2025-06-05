using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeOptionUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public Image icon;
    public Button button;

    public void Setup(UpgradeData upgrade, System.Action onClick)
    {
        titleText.text = upgrade.upgradeName;
        descriptionText.text = upgrade.description;
        icon.sprite = upgrade.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick.Invoke());
    }
}
