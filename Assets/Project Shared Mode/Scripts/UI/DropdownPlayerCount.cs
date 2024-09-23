using UnityEngine;

public class DropdownPlayerCount : MonoBehaviour
{
    [SerializeField] NetworkRunnerHandler networkRunnerHandler;

    private void Awake() {
        networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
    }

    private void Start() {
        networkRunnerHandler.PlayerCount = 4;
    }

    public void DropdownNumber(int index) {
        switch (index)
        {
            case 0:
                networkRunnerHandler.PlayerCount = 4;
                break;
            case 1:
                networkRunnerHandler.PlayerCount = 6;
                break;
            case 2:
                networkRunnerHandler.PlayerCount = 10;
                break;
            
        }
    }
}
