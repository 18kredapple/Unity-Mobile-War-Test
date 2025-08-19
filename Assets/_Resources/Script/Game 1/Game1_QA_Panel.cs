using System.Collections;
using System.Collections.Generic;
using Data_Type;
using RenderHeads.Media.AVProVideo;
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

    Game1_Man_Controller _man_Controller;
    Game1_Man_Controller man_Controller
    {
        get
        {
            if (_man_Controller == null)
                _man_Controller = FindObjectOfType<Game1_Man_Controller>();
            return _man_Controller;
        }
    }

    [SerializeField] DisplayUGUI[] videoDisplayArray;
    [SerializeField] Image videoMaskImg;
    [SerializeField] Image platformImg;
    [SerializeField] TextMeshProUGUI questionTxt;
    [SerializeField] TextMeshProUGUI[] answerTxtArray;
    [SerializeField] Button[] answerTileBtnArray;
    [SerializeField] Animation answerBtnAndResultPanelAnim;
    [SerializeField] Button answerBtn;
    [SerializeField] Game1_Cloud topCloud;
    [SerializeField] Game1_Cloud bottomCloud;

    [SerializeField] float cameraMoveOffsetYForDrop;

    Game1_QA qa;

    int currentAnswerIndex = -1;
    ANSWER_TILE currentAnswerTile = ANSWER_TILE.CENTER;


    // Start is called before the first frame update
    void Start()
    {
        answerBtn.enabled = false;
        videoMaskImg.sprite = Resource_Manager.instance.game1_qaVideoMaskSptArray[0];
        platformImg.sprite = Resource_Manager.instance.game1_stageSptArray[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Set_QA_Panel(int index, Game1_QA qa)
    {
        if (index == 0)
            answerBtnAndResultPanelAnim.Play("Answer Btn Show - Entry");
        else
            answerBtnAndResultPanelAnim.Play("Answer Btn Show");
        
        questionTxt.text = qa.question;
        this.qa = qa;

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
        Sound_Manager.instance.Play_UI_Btn_Click_Sound();

        Debug.Log($"Answer Button {index} Clicked");
        if (index == qa.correctAnswerIndex)
            Debug.Log("Correct Answer!");
        else
            Debug.Log("Wrong Answer!");

        if (currentAnswerIndex == index)
            return; // If the same button is clicked again, do nothing

        currentAnswerIndex = index;

        GAME1_MAN_MOVE_DIRECTION direction = GAME1_MAN_MOVE_DIRECTION.NONE;

        if (currentAnswerTile == ANSWER_TILE.CENTER && (ANSWER_TILE)index == ANSWER_TILE.LEFT)
            direction = GAME1_MAN_MOVE_DIRECTION.CL;
        else if (currentAnswerTile == ANSWER_TILE.CENTER && (ANSWER_TILE)index == ANSWER_TILE.RIGHT)
            direction = GAME1_MAN_MOVE_DIRECTION.CR;
        else if (currentAnswerTile == ANSWER_TILE.LEFT && (ANSWER_TILE)index == ANSWER_TILE.CENTER)
            direction = GAME1_MAN_MOVE_DIRECTION.LC;
        else if (currentAnswerTile == ANSWER_TILE.LEFT && (ANSWER_TILE)index == ANSWER_TILE.RIGHT)
            direction = GAME1_MAN_MOVE_DIRECTION.LR;
        else if (currentAnswerTile == ANSWER_TILE.RIGHT && (ANSWER_TILE)index == ANSWER_TILE.CENTER)
            direction = GAME1_MAN_MOVE_DIRECTION.RC;
        else if (currentAnswerTile == ANSWER_TILE.RIGHT && (ANSWER_TILE)index == ANSWER_TILE.LEFT)
            direction = GAME1_MAN_MOVE_DIRECTION.RL;

        if (!man_Controller.isJumping)
            man_Controller.Start_Jump();

        man_Controller.Set_Move_Direction(direction);
        currentAnswerTile = (ANSWER_TILE)index;

        videoMaskImg.sprite = Resource_Manager.instance.game1_qaVideoMaskSptArray[index + 1];

        if (!answerBtn.enabled)
            answerBtn.enabled = true;
            
        for (int i = 0; i < answerTileBtnArray.Length; i++)
        {
            if (i != index)
                answerTileBtnArray[i].enabled = true;
            else
                answerTileBtnArray[i].enabled = false;
        }

        for (int i = 0; i < answerTxtArray.Length; i++)
        {
            if (i != index)
                answerTxtArray[i].color = Color.black;
            else
                answerTxtArray[i].color = Color.white;
        }

        for (int i = 0; i < videoDisplayArray.Length; i++)
        {
            if (i == index)
                videoDisplayArray[i].gameObject.SetActive(true);
            else
                videoDisplayArray[i].gameObject.SetActive(false);
        }
    }

    public void OnAnswerBtn_clicked()
    {
        if (qa.correctAnswerIndex == currentAnswerIndex)
        {
            Sound_Manager.instance.Play_UI_Btn_Click_Sound();

            Debug.Log("Correct Answer!");
            answerBtn.enabled = false;
            answerBtn.GetComponent<Image>().sprite = Resource_Manager.instance.game1_qaBtnBgSptArray[1];
            StartCoroutine(CorrectAnswer_Process());
        }
        else
        {
            Debug.Log("Wrong Answer!");
            // Handle wrong answer logic here
            StartCoroutine(WrongAnswer_Process());
            answerBtn.GetComponent<Image>().sprite = Resource_Manager.instance.game1_qaBtnBgSptArray[0];
        }
    }

    IEnumerator CorrectAnswer_Process()
    {
        switch (currentAnswerTile)
        {
            case ANSWER_TILE.LEFT:
                man_Controller.Set_Move_Direction(GAME1_MAN_MOVE_DIRECTION.LC);
                break;
            case ANSWER_TILE.RIGHT:
                man_Controller.Set_Move_Direction(GAME1_MAN_MOVE_DIRECTION.RC);
                break;
        }

        man_Controller.Drop();

        yield return new WaitForSeconds(0.5f);

        Sound_Manager.instance.Play_Game1_True_Answer_Sound();

        yield return new WaitForSeconds(1f);

        bottomCloud.Start_Disappear_Animation();
        game_Manager.MoveCamera(-cameraMoveOffsetYForDrop, 1.5f, AnimationCurve.EaseInOut(0, 0, 1, 1));

        yield return new WaitForSeconds(.5f);

        platformImg.gameObject.SetActive(true);

        yield return new WaitForSeconds(5f);

        // Handle correct answer logic here
        game_Manager.OnNextQA();
    }

    IEnumerator WrongAnswer_Process()
    {
        man_Controller.Fault_Drop();
                yield return new WaitForSeconds(0.5f);

        Sound_Manager.instance.Play_Game1_False_Answer_Sound();

        yield return new WaitForSeconds(1.5f);

        game_Manager.OnFailedQA();

        yield return new WaitForSeconds(1.5f);

        man_Controller.Hide_Man();
    }
}
