using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Singleton that manages anything that has to do with the canvas
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class CanvasManager : SingletonMonoBehaviour<CanvasManager>
{
    #region Fields
    [Tooltip("Prefab for buttons for trap placing")]
    public GameObject trapButtonPrefab;

    [Tooltip("The crosshair UI")]
    public GameObject crossHairUI;

    [Tooltip("The base GameObject containing the AR elements")]
    public GameObject ARUI;

    [Tooltip("Text field for the amount of planes")]
    public Text planeCountText;

    [Tooltip("Text field for the total area")]
    public Text planeAreaText;

    [Tooltip("The UI element for displaying messages")]
    public MessageUI message;

    [Tooltip("The GameObject containing the reset button")]
    public GameObject gameOverBtn;

    [Tooltip("The GameObject containing the Jump Energy UI")]
    public GameObject jumpUIObj;

    [Tooltip("The GameObject containing the Health UI")]
    public GameObject healthUIObj;

    public GameObject leftHookshot;
    public GameObject rightHookshot;

    private GameObject[] arUIbuttons;   //array of buttons
    private Button doneBtn;
    #endregion

    #region Set Up & Life Cycle
    private void Start()
    {
#if !UNITY_IOS
        planeCountText.enabled = false;
        planeAreaText.enabled = false;
#endif
    }
    /// <summary>
    /// Sets Activates the Jump Energy UI
    /// </summary>
    /// <param name="movement">Movement script to tie it to</param>
    public void InitJumpEnergyBar(Movement movement)
    {
        jumpUIObj.SetActive(true);
        jumpUIObj.GetComponent<JumpEnergyUI>().Init(movement);
    }

    /// <summary>
    /// Sets Activates the Health UI
    /// </summary>
    /// <param name="combat">Movement script to tie it to</param>
    public void InitHealthEnergyBar(VRCombat combat)
    {
        healthUIObj.SetActive(true);
        healthUIObj.GetComponent<HealthBarUI>().Init(combat);
    }

    public void SetUpLeftHookshot(Grappling grapplingHook)
    {
        leftHookshot.SetActive(true);
        leftHookshot.GetComponent<GrapplingUI>().grappling = grapplingHook;
    }

    public void SetUpRightHookshot(Grappling grapplingHook)
    {
        rightHookshot.SetActive(true);
        rightHookshot.GetComponent<GrapplingUI>().grappling = grapplingHook;
    }

    /// <summary>
    /// Changes how the UI should look like based on the game phase of the AR Player
    /// </summary>
    /// <param name="manager">The SetUp Manager that tells what game phase it is</param>
    public void SetUpUI(ARSetUp manager)
    {
        switch (manager.CurrGamePhase)
        {
            //Adds a button to complete scanning phase
            case GamePhase.Scanning:
                planeAreaText.gameObject.SetActive(true);
                planeCountText.gameObject.SetActive(true);
                ARUI.SetActive(true);

                if (!doneBtn)
                {
                    //loops through to find the correct button
                    foreach (Button b in ARUI.GetComponentsInChildren<Button>())
                    {
                        if (b.gameObject.name == "Done")
                        {
                            doneBtn = b;
                            break;
                        }
                    }
                }

                //Callback function after clicking the 'done' button
                doneBtn.onClick.AddListener(() =>
                {
                    if (manager.CheckAggregrateArea())
                    {
                        GamePhase nextLvl = (GamePhase)((int)manager.CurrGamePhase + 1);
                        manager.SetPhaseTo(nextLvl);
                    }
                    else
                    {
                        message.SetMessage("Not enough play area! (Recommended: 3+ planes and over 4 units of play area)");
                    }
                });
                break;

            //UI for placing traps
            case GamePhase.Placing:
                planeAreaText.gameObject.SetActive(false);
                planeCountText.gameObject.SetActive(false);
                ARUI.SetActive(true);

                //activates the 'Done' button
                doneBtn.gameObject.SetActive(true);

                //'resets' previously created button
                if (arUIbuttons != null)
                {
                    foreach (GameObject obj in arUIbuttons)
                        Destroy(obj);
                }

                //refills the array of buttons
                arUIbuttons = new GameObject[manager.trapList.Length];
                for (int i = 0; i < manager.trapList.Length; i++)
                {
                    arUIbuttons[i] = Instantiate(trapButtonPrefab, ARUI.transform);
                    arUIbuttons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -100 - i * (trapButtonPrefab.GetComponent<RectTransform>().sizeDelta.y + 40));
                    arUIbuttons[i].name = "TrapBtn";
                    arUIbuttons[i].GetComponent<Image>().sprite = manager.trapList[i].image;
                }

                //loop through the buttons and hook up events
                for (int i = 0; i < arUIbuttons.Length; i++)
                {
                    //variables for scope purposes
                    int num = i;
                    int count = arUIbuttons.Length;
                    GameObject[] arbuttons = arUIbuttons;

                    //callback function
                    UnityAction action = () =>
                    {
                        manager.SetCurrTrapSelection(num);

                        //loops through all the buttons
                        for (int j = 0; j < count; j++)
                        {
                            //sets currently selected button to un-interactable
                            if (j == num)
                                arbuttons[j].GetComponent<Button>().interactable = false;

                            //sets other buttons (if count > 0) to interactable
                            else if (manager.trapList[j].count > 0)
                                arbuttons[j].GetComponent<Button>().interactable = true;
                        }
                    };

                    arUIbuttons[i].GetComponent<Button>().onClick.AddListener(action);
                }

                UpdateTrapCount(manager);
                break;

            //Disable UI elements
            case GamePhase.Playing:
                ARUI.SetActive(false);
                //foreach (Button b in ARUI.GetComponentsInChildren<Button>())
                //    b.gameObject.SetActive(false);
                break;

            default:
                ARUI.SetActive(false);
                break;
        }

        foreach (Text t in ARUI.GetComponentsInChildren<Text>())
        {
            if (t.name == "Phase")
            {
                t.text = "Current Phase: " + manager.CurrGamePhase.ToString();
                break;
            }
        }
    }
    #endregion

    #region Helper Functions
    /// <summary>
    /// Clears the selection of the trap buttons
    /// </summary>
    /// <param name="manager">ARSetUp script with the traps</param>
    public void ClearSelection(ARSetUp manager)
    {
        for (int i = 0; i < arUIbuttons.Length; i++)
        {
            if (manager.trapList[i].count > 0)
                arUIbuttons[i].GetComponent<Button>().interactable = true;
        }
    }

    /// <summary>
    /// Updates the text to reflect trap count
    /// </summary>
    /// <param name="manager">ARSetUp script with the traps</param>
    public void UpdateTrapCount(ARSetUp manager)
    {
        for (int i = 0; i < arUIbuttons.Length; i++)
            arUIbuttons[i].GetComponentInChildren<Text>().text = "(" + manager.trapList[i].count + ")";
    }

    /// <summary>
    /// Shows the Game Over button
    /// </summary>
    /// <param name="manager">The ARSetUp script</param>
    public void ShowGameOverBtn(ARSetUp manager)
    {
        ClearMsg();

        gameOverBtn.SetActive(true);
        gameOverBtn.GetComponent<Button>().onClick.RemoveAllListeners();
        gameOverBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            manager.ResetGame();
            gameOverBtn.SetActive(false);
        });
    }

    /// <summary>
    /// Sets the state of the crosshair object
    /// </summary>
    public void SetCrossHairUI(bool value)
    {
        crossHairUI.SetActive(value);
    }

    /// <summary>
    /// Updates the text to reflect the count of planes
    /// </summary>
    /// <param name="newCount">The count of planes</param>
    public void UpdatePlaneCount(int newCount)
    {
        planeCountText.text = "Plane Count: " + newCount;
    }

    /// <summary>
    /// Updates the text to reflect
    /// </summary>
    /// <param name="newArea"></param>
    public void UpdateTotalPlaneArea(float newArea)
    {
        planeAreaText.text = "Total Plane Area: " + newArea;
    }

    /// <summary>
    /// Sets and displays the message
    /// </summary>
    /// <param name="msg">Message to display</param>
    public void SetMessage(string msg)
    {
        message.SetMessage(msg);
    }

    /// <summary>
    /// Sets the message and displays it for a very long time
    /// </summary>
    /// <param name="msg">Message to display</param>
    public void SetPermanentMessage(string msg)
    {
        message.SetPermanentMessage(msg);
    }

    /// <summary>
    /// Clears out any message
    /// </summary>
    public void ClearMsg()
    {
        message.ClearMsg();
    }
    #endregion
}