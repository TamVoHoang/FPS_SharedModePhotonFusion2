
using UnityEngine;

public static class GameManager
{
    public static string[] names = new string[] {
        "John",
        "Alex",
        "Michael",
        "Emma",
        "David",
        "Sophia",
        "Rabbit",
        "Cat",
        "Tiger",
        "Monkey"
    };

    // mainmenu input file nickName gan vao day -> 
    //khi spawn this.nickName gan cho RPC_SetNickName() coll 63 NetworkPlayer
    public static string playerNickName = null;
    public static bool isEnemy;

    public static string GetRandomPlayerNickName() {
        return "Guest_" + names[Random.Range(0, names.Length)].ToString();
    }

    public static string GetRandomRoomName() {
        int roomNum = Random.Range(0, 100);
        return "Room_" + roomNum;
    }
}
