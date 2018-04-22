using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField]
    private Text ipAddress;

    [SerializeField]
    private GameObject mainMenuObj;

    [SerializeField]
    private NetworkedGameManager networkManager;

    [SerializeField]
    private GameObject ipAddressInputField;

    // Use this for initialization
    void Start()
    {
#if UNITY_IOS
        ipAddressInputField.SetActive(false);
#else
        ipAddressInputField.GetComponent<InputField>().text = networkManager.networkAddress;
#endif
    }

    public void StartTheGame()
    {
        mainMenuObj.SetActive(false);
        if (ipAddress.text.Length == 0)
        {
            networkManager.StartHost();
        }
        else
        {
            networkManager.networkAddress = ipAddress.text;
            networkManager.StartClient();
        }
    }
}
