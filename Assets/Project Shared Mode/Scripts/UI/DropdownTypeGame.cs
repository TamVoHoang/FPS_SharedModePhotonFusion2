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
                spawner.CustomLobbyName = "OurLobbyID_Survial";
                spawner.TypeGame = TypeGame.Survival;
                break;
            }
                
            case 1:
            {
                //networkRunnerHandler.customLobbyName = "OurLobbyID_Team";
                spawner.CustomLobbyName = "OurLobbyID_Team";
                spawner.TypeGame = TypeGame.Team;
                break;
            }
        }
    }
}
