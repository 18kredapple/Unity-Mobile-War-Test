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
    [Header("Character")]
    public GameObject manObj;
    [Header("Prefabs & Transforms")]
    public GameObject qaPanelPrefab;

    [Header("Start UI")]
    [SerializeField] GameObject loadingPanel;
    [SerializeField] GameObject startPanel;
    [SerializeField] GameObject resultPanel;

    [Header("Offset")]
    [SerializeField] float cameraMoveOffsetY;
    [SerializeField] float panelOffsetY;
    [SerializeField] float cameraMoveOffsetYForDrop;

    int qaCount = 0;
    int currentQAIndex = 0;
    Camera_Controller cameraController;

    GameObject previousPanel;
    GameObject currentPanel;

    List<Game1_QA> qaList = new List<Game1_QA>();

    Transform canvasTransform;

    Game1_Man_Controller man_Controller;

    // Start is called before the first frame update
    void Start()
    {
        cameraController = Camera.main.GetComponent<Camera_Controller>();
        canvasTransform = FindObjectOfType<Canvas>().transform;

        loadingPanel.SetActive(true);
        resultPanel.SetActive(false);
        startPanel.SetActive(false);

        manObj.SetActive(false);

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

        manObj.SetActive(true);
        man_Controller = manObj.GetComponent<Game1_Man_Controller>();

        Destroy(previousPanel, 2f);
    }

    public void MoveCamera(float offsetY, float duration, AnimationCurve curve)
    {
        cameraController.MoveCamera(offsetY, duration, curve);
    }

    public void OnNextQA()
    {
        currentQAIndex++;
        if (currentQAIndex < qaCount)
        {
            man_Controller.Fly();

            previousPanel = currentPanel;
            currentPanel = Instantiate(qaPanelPrefab, canvasTransform);
            currentPanel.GetComponent<Game1_QA_Panel>().Set_QA_Panel(currentQAIndex, qaList[currentQAIndex]);
            currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, panelOffsetY * (currentQAIndex - 1));

            cameraController.MoveCamera(cameraMoveOffsetY + cameraMoveOffsetYForDrop, 4f, AnimationCurve.EaseInOut(0, 0, 1, 1));

            Destroy(previousPanel, 4f);
        }
        else
            StartCoroutine(End_Game());
    }

    IEnumerator End_Game()
    {
        // Handle end game logic here
        Debug.Log("Game Ended");
        man_Controller.Greet();

        yield return new WaitForSeconds(2f);

        previousPanel = currentPanel;
        currentPanel = resultPanel;
        currentPanel.SetActive(true);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - panelOffsetY);
        cameraController.MoveCamera(-cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        Destroy(previousPanel, 2f);

        StartCoroutine(Hide_Man());
    }

    public void OnFailedQA()
    {
        previousPanel = currentPanel;
        currentPanel = resultPanel;
        currentPanel.SetActive(true);
        currentPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, previousPanel.GetComponent<RectTransform>().anchoredPosition.y - 3600);
        cameraController.MoveCamera(-cameraMoveOffsetY, 2f, AnimationCurve.EaseInOut(0, 0, 1, 1));
        Destroy(previousPanel, 2f);

        StartCoroutine(Hide_Man());
    }

    IEnumerator Hide_Man()
    {
        yield return new WaitForSeconds(1.5f);

        Sound_Manager.instance.Stop_Game1_BackgroundSound();
        man_Controller.gameObject.SetActive(false);
    }

    public void OnApplicationQuit()
    {
        // Handle application quit logic here
        Debug.Log("Application is quitting.");
        // You can save game state or perform cleanup here if needed
        Application.Quit();   
    }
}
