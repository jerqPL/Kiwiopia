using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCode;
    [SerializeField] private TMP_Text joinCodeText;

    void Awake()
    {
        joinCode.onValidateInput += OnValidateJoinCodeInput;
    }

    private char OnValidateJoinCodeInput(string text, int charIndex, char addedChar)
    {
        return char.ToUpper(addedChar);
    }

    public void StartHost()
    {
        Global.sessionHandler.StartSessionAsHost();
    }

    public void StartClient()
    {
        Global.sessionHandler.JoinSessionByCode(joinCode.text.ToUpper());
    }

    public void DisplayJoinCode(string code)
    {
        joinCodeText.text = code;
    }
}
