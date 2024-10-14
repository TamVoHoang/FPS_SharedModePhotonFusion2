using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Auth;
using UnityEngine.UI;

public class AnonymousLogin : MonoBehaviour
{
    [SerializeField] Button anonymousLoginButton;
    [SerializeField] GameObject sucessStatus;

    private void Start() {
        anonymousLoginButton.onClick.AddListener(Anonymous_Login);
        sucessStatus.SetActive(false);
    }

    public async void Anonymous_Login() {
        await AnonymousLoginButton();
    }

    async Task AnonymousLoginButton()
    {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        await auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            print("Login Success");

            AuthResult result = task.Result;
            print("Guest name: " + result.User.DisplayName);
            print("Guest Id: " + result.User.UserId);
            
            //can save user id in playerprefs
            GuestLoginSuccess(result.User.UserId);
        });

        /* string userId = SystemInfo.deviceUniqueIdentifier;
        Invoke(nameof(GuestLoginSuccess), 1f); */
    }

    void GuestLoginSuccess(string id)
    {
        anonymousLoginButton.interactable = false;
        sucessStatus.SetActive(true);
    }

}
