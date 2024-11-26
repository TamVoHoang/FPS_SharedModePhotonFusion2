using UnityEngine;
using UnityEngine.SceneManagement;

public class NickNameLookAtCamera : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    private void Start()
    {
        if(SceneManager.GetActiveScene().name == "Ready") {
            cameraTransform = Camera.main.transform;
        }
        else {
            cameraTransform = FindObjectOfType<LocalCameraHandler>().transform;
        }
    }

    private void LateUpdate()
    {
        // Make the Canvas face the camera
        if(SceneManager.GetActiveScene().name == "Ready") return;
        if(!cameraTransform) return;

        transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
    }

}
