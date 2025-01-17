using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultListUIHandler_Team : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup_TeamA;
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup_TeamB;

    [SerializeField] GameObject resultItemListPF;

    private void Awake() {
        ClearList();
    }

    public void ClearList() {
        foreach (Transform item in verticalLayoutGroup_TeamA.transform) {
            Destroy(item.gameObject);
        }

        foreach (Transform item in verticalLayoutGroup_TeamB.transform) {
            Destroy(item.gameObject);
        }
    }

    public void AddToList(NetworkPlayer networkPlayer) {
        // neu team A add vao verticalLayoutGroup_TeamA
        if(!networkPlayer.isEnemy_Network) {
            ResultInfoUIListItem resultInfoUIListItem = Instantiate(resultItemListPF, verticalLayoutGroup_TeamA.transform).GetComponent<ResultInfoUIListItem>();
            resultInfoUIListItem.SetInfomation(networkPlayer);
        } else {
            ResultInfoUIListItem resultInfoUIListItem = Instantiate(resultItemListPF, verticalLayoutGroup_TeamB.transform).GetComponent<ResultInfoUIListItem>();
            resultInfoUIListItem.SetInfomation(networkPlayer);
        }
    }

}
