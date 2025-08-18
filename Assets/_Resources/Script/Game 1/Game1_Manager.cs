using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.LSS;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;
using FireBase;
using Data_Type;


public class Game1_Manager : MonoBehaviour
{
    [Header("Prefabs & Transforms")]
    public GameObject qaPanelPrefab;

    [Header("Start UI")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject startPanel;
    [SerializeField] GameObject resultPanel;
    [SerializeField] Button answerBtn;

    [Header("Offset")]
    [SerializeField] float cameraMoveOffsetY;
    [SerializeField] float panelOffsetY;

    int qaCount = 0;
    int currentQAIndex = 0;
    Camera_Controller cameraController;

    GameObject previousPanel;
    GameObject currentPanel;

    List<Game1_QA> qaList = new List<Game1_QA>();

    Transform canvasTransform;

    // Start is called before the first frame update
    void Start()
    {
        cameraController = Camera.main.GetComponent<Camera_Controller>();
        canvasTransform = FindObjectOfType<Canvas>().transform;

        loadingPanel.SetActive(true);
        resultPanel.SetActive(false);
        startPanel.SetActive(false);

        StartCoroutine(Start_Game());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Start_Game()
    {
        yield return new WaitForSeconds(2f);
        if (!Firebase_Manager.instance.isFirebaseInitialized)
            Debug.Log("Error : Firebase Initialization Failed. Please check your internet connection and restart the app.");

        Debug.Log("Success : Firebase Initialized Successfully!");

        Load_GameData().Forget();
    }

    public async Task Load_GameData()
    {
        // Start Game
        await Data_Manager.instance.Initialize();
        qaList = Data_Manager.instance.game1_QA_List;
        qaCount = qaList.Count;
        Debug.Log($"QA Count: {qaCount}");

        loadingPanel.SetActive(false);
        startPanel.SetActive(true);
    }

    public void OnPlayBtn_clicked()
    {
        previousPanel = currentPanel;
        currentPanel = Instantiate(qaPanelPrefab, canvasTransform);
        currentPanel.GetComponent<Game1_QA_Panel>().Set_QA_Panel(0, qaList[0]);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, panelOffsetY * (currentQAIndex - 1));
        cameraController.MoveCamera(-cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        StartCoroutine(showAnswerPanel());
    }

    IEnumerator showAnswerPanel()
    {
        yield return new WaitForSeconds(2f);
        startPanel.SetActive(false);
    }

    public void OnNextQA()
    {
        currentQAIndex++;
        if (currentQAIndex < qaCount)
        {
            previousPanel = currentPanel;
            currentPanel = Instantiate(qaPanelPrefab, canvasTransform);
            currentPanel.GetComponent<Game1_QA_Panel>().Set_QA_Panel(currentQAIndex + 1, qaList[currentQAIndex + 1]);
            currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, panelOffsetY * (currentQAIndex - 1));

            cameraController.MoveCamera(cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));

            Destroy(previousPanel, 2f);
        }
        else
        {
            previousPanel = currentPanel;
            currentPanel = resultPanel;
            currentPanel.SetActive(true);
            currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - panelOffsetY);
            cameraController.MoveCamera(-cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
            Destroy(previousPanel, 2f);
        }
    }

    public void OnFailedQA()
    {
        previousPanel = currentPanel;
        currentPanel = resultPanel;
        currentPanel.SetActive(true);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - 3600);
        cameraController.MoveCamera(-cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        Destroy(previousPanel, 2f);
    }
}
