using UnityEngine;

public class DropdownSceneName : MonoBehaviour
{
    [SerializeField] Spawner spawner;

    public void DropdownNumber(int index) {
        spawner = FindObjectOfType<Spawner>();
        switch (index)
        {
            case 0:
            {
                //spawner.SceneName = "World_1";
                spawner.GameMap = GameMap.World_1;
                break;
            }
            
            case 1:
            {
                //spawner.SceneName = "World_2";
                spawner.GameMap = GameMap.World_2;
                break;
            }
            
            case 2:
            {
                //spawner.SceneName = "World_3";
                spawner.GameMap = GameMap.World_3;
                break;
            }
        }
    }
}
