using System.Collections;
using System.Collections.Generic;
using Data_Type;
using UnityEngine;

public class Game1_Manager : MonoBehaviour
{
    [Header("Prefabs & Transforms")]
    [SerializeField] GameObject startPanelPrefab;
    [SerializeField] GameObject qaPanelPrefab;
    [SerializeField] GameObject resultPanelPrefab;
    [SerializeField] Transform canvasTransform;

    [Header("UI Panel Offset")]
    [SerializeField] float panelOffsetY;

    int qaCount = 0;
    int currentQAIndex = 0;
    Camera_Controller cameraController;

    GameObject previousPanel;
    GameObject currentPanel;

    List<Game1_QA> qaList = new List<Game1_QA>();

    // Start is called before the first frame update
    void Start()
    {
        qaList = Data_Manager.instance.game1_QAList(1).Result;
        qaCount = qaList.Count;
        Debug.Log($"QA Count: {qaCount}");
        currentPanel = Instantiate(startPanelPrefab, canvasTransform);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayBtn_clicked()
    {
        previousPanel = currentPanel;
        currentPanel = Instantiate(qaPanelPrefab, canvasTransform);
        currentPanel.GetComponent<Game1_QA_Panel>().Set_QA_Panel(0, qaList[0]);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -panelOffsetY);
        cameraController.MoveCamera(-135f, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        Destroy(previousPanel, 2f);
    }

    public void OnNextQA()
    {
        currentQAIndex++;
        if (currentQAIndex + 1 < qaCount)
        {
            previousPanel = currentPanel;
            currentPanel = Instantiate(qaPanelPrefab, canvasTransform);
            currentPanel.GetComponent<Game1_QA_Panel>().Set_QA_Panel(currentQAIndex + 1, qaList[currentQAIndex + 1]);
            currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, panelOffsetY * (currentQAIndex - 1));
            cameraController.MoveCamera(panelOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
            Destroy(previousPanel, 2f);
        }
        else
        {
            previousPanel = currentPanel;
            currentPanel = Instantiate(resultPanelPrefab, canvasTransform);
            currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - 3600);
            cameraController.MoveCamera(-panelOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
            Destroy(previousPanel, 2f);
        }
    }

    public void OnFailedQA()
    {
        previousPanel = currentPanel;
        currentPanel = Instantiate(resultPanelPrefab, canvasTransform);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - 3600);
        cameraController.MoveCamera(-panelOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        Destroy(previousPanel, 2f);
    }
}
