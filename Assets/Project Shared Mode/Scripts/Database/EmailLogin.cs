using System.Collections;
using UnityEngine;
using TMPro;
using Firebase.Extensions;
using Firebase.Auth;
using Firebase;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class EmailLogin : MonoBehaviour
{
    #region variables
    [Header("Overview")]
    [SerializeField] Button QuitButton;
    [SerializeField] Button LoginGestButton;


    [Header("Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    [SerializeField] Button loginButton;
    public TMP_InputField emailRequest;
    [SerializeField] Button sendRequestPass;

    [Header("Reset Request")]
    public TMP_InputField resetEmail;
    [SerializeField] Button sendRequestButton;

    [Header("Sign up")]
    public TMP_InputField signupEmail;
    public TMP_InputField userName;
    public TMP_InputField signupPassword;
    public TMP_InputField signupPasswordConfirm;
    [SerializeField] Button signUpButton;


    [Header("Extra")]
    public GameObject loadingScreen;
    public TextMeshProUGUI logTxt;

    public GameObject loginUi, signupUi, SuccessUi;
    [SerializeField] TextMeshProUGUI id;
    #endregion

    [SerializeField] GameObject PlayerInfoUI;
    const string MAILKEY = "mail";
    const string PASSKEY = "pass";

    private void Start() {

        loginButton.onClick.AddListener(Login);
        signUpButton.onClick.AddListener(SignUp);
        // sendRequestButton.onClick.AddListener(SendRequest);
        PlayerInfoUI.SetActive(false);
        sendRequestPass.onClick.AddListener(SendResetPass);
        QuitButton.onClick.AddListener(() => Application.Quit());
        LoginGestButton.onClick.AddListener(LoginGuest);

        // show mail and pass if PlayerPrefs having key "mail" "pass"
        if(PlayerPrefs.HasKey(MAILKEY) && PlayerPrefs.HasKey(PASSKEY)) {
            loginEmail.text = PlayerPrefs.GetString(MAILKEY);
            loginPassword.text = PlayerPrefs.GetString(PASSKEY);
        }
    }

    #region signup

    public void SignUp()
    {
        loadingScreen.SetActive(true);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = signupEmail.text;
        string useName = this.userName.text;
        string password = signupPassword.text;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                ShowLogMsg_SingUP("Sign up unsuccessfully");
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }
            // Firebase user has been created.

            //loadingScreen.SetActive(false);
            StartCoroutine(ResetLoadingScreenCo());
            AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            signupEmail.text = "";
            this.userName.text = "";
            signupPassword.text = "";
            signupPasswordConfirm.text = "";

            if (result.User.IsEmailVerified)
            {
                ShowLogMsg_SingUP("Sign up Successful");
            }
            else {
                ShowLogMsg_SingUP("Please verify your email!!");
                SendEmailVerification();
            }

            //? save after signup with having email and password
            DataSaver.Instance.SaveToSignup(useName, result.User.UserId);                       // save info realtime
            DataSaveLoadHander.Instance.SavePlayerDataToSignup(useName, result.User.UserId);    // save info firestore

            /* DataSaveLoadHander.Instance.SaveInventoryDataFireStoreToSignUp(); */                 // save inventory firestore
        });
    }

    IEnumerator ResetLoadingScreenCo() {
        yield return new WaitForSeconds(1f);
        loadingScreen.SetActive(false);
    }

    public void SendEmailVerification() {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    IEnumerator SendEmailForVerificationAsync() {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user!=null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                print("Email send error");
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                switch (error)
                {
                    case AuthError.None:
                        break;
                    case AuthError.Unimplemented:
                        break;
                    case AuthError.Failure:
                        break;
                    case AuthError.InvalidCustomToken:
                        break;
                    case AuthError.CustomTokenMismatch:
                        break;
                    case AuthError.InvalidCredential:
                        break;
                    case AuthError.UserDisabled:
                        break;
                    case AuthError.AccountExistsWithDifferentCredentials:
                        break;
                    case AuthError.OperationNotAllowed:
                        break;
                    case AuthError.EmailAlreadyInUse:
                        break;
                    case AuthError.RequiresRecentLogin:
                        break;
                    case AuthError.CredentialAlreadyInUse:
                        break;
                    case AuthError.InvalidEmail:
                        break;
                    case AuthError.WrongPassword:
                        break;
                    case AuthError.TooManyRequests:
                        break;
                    case AuthError.UserNotFound:
                        break;
                    case AuthError.ProviderAlreadyLinked:
                        break;
                    case AuthError.NoSuchProvider:
                        break;
                    case AuthError.InvalidUserToken:
                        break;
                    case AuthError.UserTokenExpired:
                        break;
                    case AuthError.NetworkRequestFailed:
                        break;
                    case AuthError.InvalidApiKey:
                        break;
                    case AuthError.AppNotAuthorized:
                        break;
                    case AuthError.UserMismatch:
                        break;
                    case AuthError.WeakPassword:
                        break;
                    case AuthError.NoSignedInUser:
                        break;
                    case AuthError.ApiNotAvailable:
                        break;
                    case AuthError.ExpiredActionCode:
                        break;
                    case AuthError.InvalidActionCode:
                        break;
                    case AuthError.InvalidMessagePayload:
                        break;
                    case AuthError.InvalidPhoneNumber:
                        break;
                    case AuthError.MissingPhoneNumber:
                        break;
                    case AuthError.InvalidRecipientEmail:
                        break;
                    case AuthError.InvalidSender:
                        break;
                    case AuthError.InvalidVerificationCode:
                        break;
                    case AuthError.InvalidVerificationId:
                        break;
                    case AuthError.MissingVerificationCode:
                        break;
                    case AuthError.MissingVerificationId:
                        break;
                    case AuthError.MissingEmail:
                        break;
                    case AuthError.MissingPassword:
                        break;
                    case AuthError.QuotaExceeded:
                        break;
                    case AuthError.RetryPhoneAuth:
                        break;
                    case AuthError.SessionExpired:
                        break;
                    case AuthError.AppNotVerified:
                        break;
                    case AuthError.AppVerificationFailed:
                        break;
                    case AuthError.CaptchaCheckFailed:
                        break;
                    case AuthError.InvalidAppCredential:
                        break;
                    case AuthError.MissingAppCredential:
                        break;
                    case AuthError.InvalidClientId:
                        break;
                    case AuthError.InvalidContinueUri:
                        break;
                    case AuthError.MissingContinueUri:
                        break;
                    case AuthError.KeychainError:
                        break;
                    case AuthError.MissingAppToken:
                        break;
                    case AuthError.MissingIosBundleId:
                        break;
                    case AuthError.NotificationNotForwarded:
                        break;
                    case AuthError.UnauthorizedDomain:
                        break;
                    case AuthError.WebContextAlreadyPresented:
                        break;
                    case AuthError.WebContextCancelled:
                        break;
                    case AuthError.DynamicLinkNotActivated:
                        break;
                    case AuthError.Cancelled:
                        break;
                    case AuthError.InvalidProviderId:
                        break;
                    case AuthError.WebInternalError:
                        break;
                    case AuthError.WebStorateUnsupported:
                        break;
                    case AuthError.TenantIdMismatch:
                        break;
                    case AuthError.UnsupportedTenantOperation:
                        break;
                    case AuthError.InvalidLinkDomain:
                        break;
                    case AuthError.RejectedCredential:
                        break;
                    case AuthError.PhoneNumberNotFound:
                        break;
                    case AuthError.InvalidTenantId:
                        break;
                    case AuthError.MissingClientIdentifier:
                        break;
                    case AuthError.MissingMultiFactorSession:
                        break;
                    case AuthError.MissingMultiFactorInfo:
                        break;
                    case AuthError.InvalidMultiFactorSession:
                        break;
                    case AuthError.MultiFactorInfoNotFound:
                        break;
                    case AuthError.AdminRestrictedOperation:
                        break;
                    case AuthError.UnverifiedEmail:
                        break;
                    case AuthError.SecondFactorAlreadyEnrolled:
                        break;
                    case AuthError.MaximumSecondFactorCountExceeded:
                        break;
                    case AuthError.UnsupportedFirstFactor:
                        break;
                    case AuthError.EmailChangeNeedsVerification:
                        break;
                    default:
                        break;
                }
            }
            else {
                print("Email successfully send");
            }
        }
    }

    #endregion

    #region ResetPassword
    public void SendResetPass() {
        if(string.IsNullOrEmpty(emailRequest.text)) {
            ShowLogMsg("Email is not accepted, Please input again");
            return;
        }

        SendResetPass(emailRequest.text);
    }
    
    public void SendResetPass(string emailRequest) {
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SendPasswordResetEmailAsync(emailRequest).ContinueWithOnMainThread(task => {
            if(task.IsCanceled) {
                Debug.Log($"SendPassWordResetEmailAsync was canceled");
            }
            if(task.IsFaulted) {
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as FirebaseException;
                    if(firebaseEx != null) {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        // thong bao
                        ShowLogMsg("Can not send reset mail");
                    }
                }
            }

            ShowLogMsg("succesully send mail to reset pass, Please check mail");
        });
    }

    #endregion ReadPassword

    #region Login
    public void Login() {
        loadingScreen.SetActive(true);

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        string email = loginEmail.text;
        string password = loginPassword.text;

        Credential credential =
        EmailAuthProvider.GetCredential(email, password);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                ShowLogMsg("Log in fail");
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }
            
            StartCoroutine(ResetLoadingScreenCo());
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            if (result.User.IsEmailVerified)
            {
                ShowLogMsg("Log in Successful");
                //StartCoroutine(ShowingPlayerInfoCo(0.5f));

                id.text = $"ID: {result.User.UserId}";

                //? gan userId cho saveLoadHander Firebase | FireStore
                DataSaver.Instance.userId = result.User.UserId;
                DataSaveLoadHander.Instance.userId = result.User.UserId;
            }
            else {
                ShowLogMsg("Please verify email!!");
            }

            SetPlayerPref(email, password);

            //? Load data after Login with UserID
            DataSaver.Instance.LoadData();
            DataSaveLoadHander.Instance.LoadPlayerDataFireStore();

            // chuyen qua scene MainLobby
            StartCoroutine(LoadMainLobbySceneCo());
            
        });
    }

    IEnumerator LoadMainLobbySceneCo() {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadSceneAsync("MainLobby");
    }

    IEnumerator ShowingPlayerInfoCo(float time) {
        SuccessUi.SetActive(true);
        yield return new WaitForSeconds(time);
        loginUi.SetActive(false);
        
        yield return new WaitForSeconds(0.2f);
        if(PlayerInfoUI != null) PlayerInfoUI.SetActive(true);
    }

    // Login As Guest
    public void LoginGuest()
    {
        loadingScreen.SetActive(true);
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled) {
                Debug.LogError("SignIn Guest was canceled.");
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignIn Guest encountered an error: " + task.Exception);
                StartCoroutine(ResetLoadingScreenCo());
                return;
            }

            StartCoroutine(ResetLoadingScreenCo());
            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

            //? dave userId cho saveLoadHander Firebase | FireStore
            string userNameTemp = GameManager.GetRandomPlayerNickName();
            DataSaveLoadHander.Instance.SavePlayerDataToSignup(userNameTemp, result.User.UserId); 

            ShowLogMsg("Log in Successful");
            loginUi.SetActive(false);
            SuccessUi.SetActive(true);
            id.text = $"ID: {result.User.UserId}";

            DataSaveLoadHander.Instance.userId = result.User.UserId;
            DataSaveLoadHander.Instance.LoadPlayerDataFireStore();

            // chuyen qua scene MainLobby
            StartCoroutine(LoadMainLobbySceneCo());
        });
    }
    // Login As Guest

    #endregion Login

    #region extra
    void ShowLogMsg(string msg)
    {
        logTxt.text = msg;
        //logTxt.GetComponent<Animation>().Play("FadeOutAnimation");
        StartCoroutine(TextFadeOut(1.5f));
    }
    IEnumerator TextFadeOut(float time) {
        yield return new WaitForSeconds(time);
        logTxt.text = "";

        if(SuccessUi != null) {
            SuccessUi.SetActive(false);   // chu tat -> tat vong loading
        }
    }

    void ShowLogMsg_SingUP(string msg)
    {
        logTxt.text = msg;
        //logTxt.GetComponent<Animation>().Play("FadeOutAnimation");
        StartCoroutine(TextFadeOut_SignUp(1.5f));
    }
    
    IEnumerator TextFadeOut_SignUp(float time) {
        yield return new WaitForSeconds(time);
        logTxt.text = "";

        if(SuccessUi != null) {
            SuccessUi.SetActive(false);
        }
    }
    #endregion

    void SetPlayerPref(string mail, string password) {
        PlayerPrefs.SetString(MAILKEY, mail);
        PlayerPrefs.SetString(PASSKEY, password);
        PlayerPrefs.Save();
    }
}