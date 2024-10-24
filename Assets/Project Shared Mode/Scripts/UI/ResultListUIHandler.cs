using UnityEngine;
using UnityEngine.UI;

public class ResultListUIHandler : MonoBehaviour
{
    [SerializeField] VerticalLayoutGroup verticalLayoutGroup;
    [SerializeField] GameObject resultItemListPF;

    private void Awake() {
        ClearList();
    }

    public void ClearList() {
        foreach (Transform item in verticalLayoutGroup.transform) {
            Destroy(item.gameObject);
        }
    }

    public void AddToList(NetworkPlayer networkPlayer) {
        //tao ra doi tuong
        ResultInfoUIListItem resultInfoUIListItem = Instantiate(resultItemListPF, verticalLayoutGroup.transform).GetComponent<ResultInfoUIListItem>();
        resultInfoUIListItem.SetInfomation(networkPlayer);
    }
}
