using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class SetNickname : MonoBehaviour
{
    TMP_InputField m_InputField;
    Button m_SubmitButton;

    protected async void OnEnable()
    {
        m_InputField = GetComponentInChildren<TMP_InputField>();
        m_SubmitButton = GetComponentInChildren<Button>();

        try
        {
            // Initialize Unity Services first
            await UnityServices.InitializeAsync();
            
            await WaitForAuthentication();
            await UpdatePlayerNameUI();

            m_InputField.onValueChanged.AddListener(OnInputValueChanged);
            m_SubmitButton.onClick.AddListener(() => OnNicknameSubmitted(m_InputField.text));
        }
        catch (ServicesInitializationException ex)
        {
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
        }
        catch (System.TimeoutException)
        {
            Debug.LogError("Authentication service initialization timed out");
        }
    }

    private void OnInputValueChanged(string value)
    {
        m_SubmitButton.interactable = !string.IsNullOrEmpty(value);
    }

    private async Task WaitForAuthentication(float timeoutSeconds = 10f)
    {
        var startTime = Time.time;
        while (!AuthenticationService.Instance.IsAuthorized)
        {
            if (Time.time - startTime > timeoutSeconds)
            {
                throw new System.TimeoutException("Authentication service initialization timed out");
            }
            await Task.Delay(100);
        }
    }

    // Remove the #0000 pattern from the end of the name
    private string TrimDenominator(string playerName)
    {
        return Regex.Replace(playerName, @"#\d{4}$", "");
    }

    private async Task UpdatePlayerNameUI()
    {
        string fullName = await AuthenticationService.Instance.GetPlayerNameAsync();
        m_InputField.text = TrimDenominator(fullName);
    }

    private void OnNicknameSubmitted(string nickname)
    {
        if (!string.IsNullOrEmpty(nickname))
        {
            string trimmedName = TrimDenominator(nickname);
            _ = AuthenticationService.Instance.UpdatePlayerNameAsync(trimmedName);
            _ = UpdatePlayerNameUI();
        }
    }

    protected void OnDisable()
    {
        if (m_InputField != null)
        {
            m_InputField.onEndEdit.RemoveListener(OnNicknameSubmitted);
        }
    }
}