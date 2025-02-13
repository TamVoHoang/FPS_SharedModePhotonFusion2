using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkPlayerInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Slider healthSlider;

    //others
    HPHandler hPHandler;
    
    private void Awake() {
        hPHandler = GetComponent<HPHandler>();
    }

    private void Start() {
        hPHandler.UpdateSliderHealth += OnUpdateSliderHealth_NetworkPlayerInfo;
    }

    private void OnDisable() {
        hPHandler.UpdateSliderHealth -= OnUpdateSliderHealth_NetworkPlayerInfo;
    }

    private void OnUpdateSliderHealth_NetworkPlayerInfo(byte hpMax, byte hpCurr)
    {
        healthSlider.maxValue = hpMax;
        healthSlider.value = hpCurr;
        healthText.text = "HP: " + healthSlider.value.ToString();
    }
}
