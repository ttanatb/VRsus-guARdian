using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CanvasManager : SingletonMonoBehaviour<CanvasManager>
{
    public GameObject blockPlacingUI;
    public GameObject buttonPrefab;
    public GameObject exitButtonPrefab;
    public Button placementButton;

    public GameObject crossHairUI;
    public GameObject goalPlacingUI;

    public GameObject ARUI;
    private GameObject[] arUIbuttons;

    public Text planeCountText;
    public Text planeAreaText;

    public Message message;

    public void SetUpBlockPlacingUI(BlockManager blockPlacer, int count)
    {
        placementButton.gameObject.SetActive(true);
        blockPlacingUI.SetActive(true);
        //Clears all the buttons
        for (int i = 0; i < blockPlacingUI.transform.childCount; i++)
        {
            Destroy(blockPlacingUI.transform.GetChild(i).gameObject);
        }

        Button[] btns = new Button[count];

        for (int i = 0; i < count; i++)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, blockPlacingUI.transform);
            Vector2 pos = new Vector2((15 * (i + 1)) + (120 * i), 0f);
            buttonObj.GetComponent<RectTransform>().anchoredPosition = pos;
            btns[i] = buttonObj.GetComponent<Button>();

            UnityAction action = blockPlacer.GetActionToSetPlacingMode(i, placementButton);
            btns[i].onClick.AddListener(action);
        }

        //GameObject exitButton = Instantiate(exitButtonPrefab, blockPlacingUI.transform);
        //set button listener for exit

        UnityAction placementBtnAction = blockPlacer.GetActionToPlaceBlock(btns, placementButton);
        placementButton.onClick.AddListener(placementBtnAction);
    }
    public void DisableBlockPlacingUI()
    {
        blockPlacingUI.SetActive(false);
        placementButton.gameObject.SetActive(false);
    }

    public void ToggleCrossHairUI()
    {
        crossHairUI.SetActive(!crossHairUI.activeSelf);
    }

    public void SetUpGoalPlacingUI(Combat combat)
    {
        goalPlacingUI.SetActive(true);
        goalPlacingUI.GetComponent<Button>().onClick.AddListener(combat.GetActionToSwitchToPlacingMode());
        goalPlacingUI.GetComponent<Button>().onClick.AddListener(() => { goalPlacingUI.SetActive(false); });
    }

    public void SetUI(GameManager manager)
    {
        switch (manager.CurrGamePhase)
        {
            case GamePhase.Scanning:
                ARUI.SetActive(true);
                foreach (Button b in ARUI.GetComponentsInChildren<Button>())
                {
                    if (b.name == "Done")
                    {
                        b.onClick.AddListener(() =>
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
                    }
                }
                break;
            case GamePhase.Placing:
                manager.SetPhaseTo(GamePhase.Playing);
                break;

                List<GameObject> buttons = new List<GameObject>();
                for (int i = 0; i < manager.trapList.Length; i++)
                {
                    buttons.Add(Instantiate(buttonPrefab, ARUI.transform));
                    buttons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(-20, -70 - i * 50);
                }
                arUIbuttons = buttons.ToArray();

                for (int i = 0; i < arUIbuttons.Length; i++)
                {
                    int num = i;
                    int count = arUIbuttons.Length;
                    GameObject[] arbuttons = arUIbuttons;
                    UnityAction action = () =>
                    {
                        manager.SetCurrTrapSelection(num);

                        for (int j = 0; j < count; j++)
                        {
                            if (j == num)
                            {
                                arbuttons[j].GetComponent<Button>().interactable = false;
                            }
                            else if (manager.trapList[j].count > 0)
                            {
                                arbuttons[j].GetComponent<Button>().interactable = true;
                            }
                        }
                    };

                    arUIbuttons[i].GetComponent<Button>().onClick.AddListener(action);
                }


                UpdateTrapCount(manager);
                break;
            case GamePhase.Playing:
                foreach (Button b in ARUI.GetComponentsInChildren<Button>())
                {
                    b.gameObject.SetActive(false);
                }

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

    public void ClearSelection(GameManager manager)
    {
        for (int i = 0; i < arUIbuttons.Length; i++)
        {
            if (manager.trapList[i].count > 0)
            {
                arUIbuttons[i].GetComponent<Button>().interactable = true;
            }
        }
    }

    public void UpdateTrapCount(GameManager manager)
    {
        for (int i = 0; i < arUIbuttons.Length; i++)
        {
            arUIbuttons[i].GetComponentInChildren<Text>().text = manager.trapList[i].trap.name + ": (" + manager.trapList[i].count + ")";
        }
    }

    public void UpdatePlaneCount(int newCount)
    {
        planeCountText.text = "Plane Count: " + newCount;
    }

    public void UpdateTotalPlaneArea(float newArea)
    {
        planeAreaText.text = "Total Plane Area: " + newArea;
    }

    public void SetMessage(string msg)
    {
        message.SetMessage(msg);
    }

    public void SetPermanentMessage(string msg)
    {
        message.SetPermanentMessage(msg);
    }
}
