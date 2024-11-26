using Firebase.Auth;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class SignUpPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField emailInputField;
    [SerializeField] TMP_InputField passwordInputField;
    [SerializeField] TMP_InputField passwordConfirmField;

    /// <summary>
    /// Sign Up 버튼을 눌렀을때 반응할 함수
    /// </summary>
    public void SignUp()
    {
        string email = emailInputField.text;
        string pass = passwordInputField.text;
        string confirm = passwordConfirmField.text;

        if (email.IsNullOrEmpty())
        {
            Debug.LogWarning("이메일을 입력해주세요.");
            return;
        }

        if (pass != confirm)
        {
            Debug.LogWarning("패스워드가 일치하지 않습니다.");
            return;
        }

        // 파이어베이스 가입절차
        BackendManager.Auth.CreateUserWithEmailAndPasswordAsync(email, pass)
            .ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            // Firebase user has been created.
            AuthResult result = task.Result;
            Debug.Log($"Firebase user created successfully: {result.User.DisplayName} ({result.User.UserId})");

            gameObject.SetActive(false);
        });
    }



}
