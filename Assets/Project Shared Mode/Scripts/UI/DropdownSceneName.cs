using TMPro;
using UnityEngine;

public class DropdownSceneName : MonoBehaviour
{
    //[SerializeField] TextMeshProUGUI sceneName;
    [SerializeField] MainMenuUIHandler mainMenuUIHandler;
    [SerializeField] Spawner spawner;

    private void Awake() {
        //mainMenuUIHandler = FindObjectOfType<MainMenuUIHandler>();
        //spawner = FindObjectOfType<Spawner>();
    }
    private void Start() {
        //mainMenuUIHandler.SceneName = "World1";
    }

    public void DropdownNumber(int index) {
        spawner = FindObjectOfType<Spawner>();
        switch (index)
        {
            case 0:
            {
                //sceneName.text = "1";
                //mainMenuUIHandler.SceneName = "World1";
                spawner.SceneName = "World1";
                break;
            }
                
            case 1:
            {
                //sceneName.text = "2";
                //mainMenuUIHandler.SceneName = "World2";
                spawner.SceneName = "World2";

                break;
            }
                
            case 2:
            {
                //sceneName.text = "3";
                //mainMenuUIHandler.SceneName = "World3";
                spawner.SceneName = "World3";

                break;
            }
                
            
        }
    }
}
