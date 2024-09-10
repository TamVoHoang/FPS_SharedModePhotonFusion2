using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;


public class SessionListUIHandler : MonoBehaviour
{
    [SerializeField] Button createSessionRandom;

    [SerializeField] Transform sessionListContenParent;
    [SerializeField] GameObject SessionInfoUIListItem;
    
    private void Awake() {

    }
}
