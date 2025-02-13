using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingAimSentivity : MonoBehaviour
{
    const float MAX_AIM_SENTIVITY = 6;
    const float MIN_AIM_SENTIVITY = 1;
    const int DELTA = 1;

    [SerializeField] Slider mouseSensitivity_Slider;
    [SerializeField] TMP_Text mouseSensitivity_Text;
    [SerializeField] Button PlusButton;
    [SerializeField] Button MinusButton;

    public static Action<int> OnUpdateSentivitySlider;
    private void Start() {
        if (mouseSensitivity_Slider != null && mouseSensitivity_Text != null) {
            mouseSensitivity_Slider.maxValue = MAX_AIM_SENTIVITY;
            mouseSensitivity_Slider.minValue = MIN_AIM_SENTIVITY;
            mouseSensitivity_Slider.value = 2;

        }

        PlusButton.onClick.AddListener(PlusButton_OnClicked);
        MinusButton.onClick.AddListener(MinusButton_OnClicked);
    }

    private void OnEnable() {
        OnUpdateSentivitySlider += UpdateSensitivitySlider_InputHandler;
    }

    private void OnDisable() {
        OnUpdateSentivitySlider -= UpdateSensitivitySlider_InputHandler;

    }

    private void MinusButton_OnClicked() {
        SetSliderLocal(-DELTA);
    }

    private void PlusButton_OnClicked() {
        SetSliderLocal(DELTA);
    }

    void SetSliderLocal(int delta) {
        mouseSensitivity_Slider.value += delta;
        mouseSensitivity_Text.text = mouseSensitivity_Slider.value.ToString();

        // set slider current value for aim variable in CharacterInputHandler
        int a =  (int)mouseSensitivity_Slider.value;
        CharacterInputHandler.OnSetAimSentivity?.Invoke(a);
    }

    private void UpdateSensitivitySlider_InputHandler(int aimCurrent) {
        if (mouseSensitivity_Slider != null && mouseSensitivity_Text != null) {
            mouseSensitivity_Slider.value = aimCurrent;
            mouseSensitivity_Text.text = Math.Round((float)aimCurrent, 1).ToString();
        }
    }
}
