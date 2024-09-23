using UnityEngine;

public class DropdownTypeGame : MonoBehaviour
{
    Spawner spawner;

    public void DropdownNumber(int index) {
        spawner = FindObjectOfType<Spawner>();
        switch (index)
        {
            case 0:
            {
                //networkRunnerHandler.customLobbyName = "OurLobbyID";
                spawner.customLobbyName = "OurLobbyID";
                break;
            }
                
            case 1:
            {
                //networkRunnerHandler.customLobbyName = "OurLobbyID_Team";
                spawner.customLobbyName = "OurLobbyID_Team";
                break;
            }
        }
    }
}
