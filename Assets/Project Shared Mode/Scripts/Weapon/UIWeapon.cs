
using UnityEngine;

public class UIWeapon : MonoBehaviour
{
    [SerializeField] Transform weaponSlotUIHolder;
    [SerializeField] Transform[] weaponSlotsUI;

    private void Awake() {
        weaponSlotsUI = new Transform[weaponSlotUIHolder.GetComponent<Transform>().childCount];
    }
    private void Start() {
        

        for (int i = 0; i < weaponSlotUIHolder.GetComponent<Transform>().childCount; i++)
        {
            weaponSlotsUI[i] = weaponSlotUIHolder.GetChild(i).transform;
        }
    }

    public void Set(WeaponSwitcher weaponSwitcher) {
        weaponSwitcher.updateWeaponUI += UpdateWeaponUI;
    }

    void UpdateWeaponUI(int indexSlotActive, int gunsNumber, bool isGun) {

        Debug.Log($"de dang ky updateWeaponUI() cho weaponSwitcher" + isGun);
        for (int i = 0; i < weaponSlotsUI.Length; i++)
        {
            weaponSlotsUI[i].GetChild(0).gameObject.SetActive(false);
        }

        // neu drop va ko con sung thi off het selection image
        if(gunsNumber >=1) {
            weaponSlotsUI[indexSlotActive].GetChild(0).gameObject.SetActive(true);

            weaponSlotsUI[indexSlotActive].GetChild(1).gameObject.SetActive(true);
        }
        else {
            weaponSlotsUI[indexSlotActive].GetChild(0).gameObject.SetActive(false);
        }
        
        if(!isGun) weaponSlotsUI[indexSlotActive].GetChild(1).gameObject.SetActive(false);
        
        
    }
} 
