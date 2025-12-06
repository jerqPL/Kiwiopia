using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCode;
    public void StartHost()
    {
        Global.sessionHandler.StartSessionAsHost();
    }

    public void StartClient()
    {
        Global.sessionHandler.JoinSessionByCode(joinCode.text.ToUpper());
    }
}
