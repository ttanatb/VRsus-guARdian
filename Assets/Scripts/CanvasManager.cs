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

        for(int i = 0; i < count; i++)
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
}
