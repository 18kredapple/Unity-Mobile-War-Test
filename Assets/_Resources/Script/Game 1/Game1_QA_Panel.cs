using System.Collections;
using System.Collections.Generic;
using Data_Type;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Game1_QA_Panel : MonoBehaviour
{
    Game1_Manager _game1_Manager;
    Game1_Manager game_Manager
    {
        get
        {
            if (_game1_Manager == null)
                _game1_Manager = FindObjectOfType<Game1_Manager>();
            return _game1_Manager;
        }
    }
    [SerializeField] Image videoMaskImg;
    [SerializeField] Image platformImg;
    [SerializeField] TextMeshProUGUI questionTxt;
    [SerializeField] TextMeshProUGUI[] answerTxtArray;
    [SerializeField] Button[] answerTileBtnArray;
    [SerializeField] Button answerBtn;
    [SerializeField] Game1_Cloud topCloud;
    [SerializeField] Game1_Cloud bottomCloud;

    int qaIndex;
    int currentAnswerIndex;
    int correctAnswerIndex;

    // Start is called before the first frame update
    void Start()
    {
        currentAnswerIndex = -1;
        correctAnswerIndex = -2;
        answerBtn.interactable = false;
        videoMaskImg.sprite = Resource_Manager.instance.game1_qaVideoMaskSptArray[0];
        platformImg.sprite = Resource_Manager.instance.game1_stageSptArray[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set_QA_Panel(int index, Game1_QA qa)
    {
        qaIndex = index;
        questionTxt.text = qa.question;
        correctAnswerIndex = qa.correctAnswerIndex;
        Debug.Log($"Correct Answer Index: {correctAnswerIndex}");

        if (qa.answers.Length > answerTxtArray.Length)
        {
            Debug.LogWarning("More answers provided than available buttons. Some answers will be ignored.");
            return;
        }

        for (int i = 0; i < answerTxtArray.Length; i++)
        {
            if (i < qa.answers.Length)
            {
                answerTxtArray[i].text = qa.answers[i];
                answerTxtArray[i].transform.parent.gameObject.SetActive(true);
            }
        }
    }

    public void OnAnswerTileBtn_clicked(int index)
    {
        Debug.Log($"Answer Button {index} Clicked");
        if (index == correctAnswerIndex)
            Debug.Log("Correct Answer!");
        else
            Debug.Log("Wrong Answer!");

        currentAnswerIndex = index;
        videoMaskImg.sprite = Resource_Manager.instance.game1_qaVideoMaskSptArray[index + 1];
        answerBtn.interactable = true;
        for (int i = 0; i < answerTileBtnArray.Length; i++)
        {
            if (i != index)
                answerTileBtnArray[i].interactable = false;
            else
                answerTileBtnArray[i].interactable = true;
        }
    }

    public void OnAnswerBtn_clicked()
    {
        if (correctAnswerIndex == currentAnswerIndex)
        {
            Debug.Log("Correct Answer!");
            // Handle correct answer logic here
            game_Manager.OnNextQA();
            bottomCloud.Start_Disappear_Animation();
            StartCoroutine(Show_PlatformImage());
        }
        else
        {
            Debug.Log("Wrong Answer!");
            // Handle wrong answer logic here
            game_Manager.OnFailedQA();
        }
    }

    IEnumerator Show_PlatformImage()
    {
        yield return new WaitForSeconds(0.5f);
        platformImg.gameObject.SetActive(true);
    }
}
