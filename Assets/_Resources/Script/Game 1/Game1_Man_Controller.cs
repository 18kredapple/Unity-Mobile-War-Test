using System.Collections;
using System.Collections.Generic;
using Data_Type;
using UnityEngine;

public class Game1_Man_Controller : MonoBehaviour
{
    [SerializeField] GameObject pompObj;
    [SerializeField] Animator manAnimator;
    [SerializeField] Animation manAnimation;
    [SerializeField] GameObject flyEffectObj;

    GAME1_MAN_MOVE_DIRECTION moveDirection;
    public bool isJumping = false;

    // Start is called before the first frame update
    void Start()
    {
        moveDirection = GAME1_MAN_MOVE_DIRECTION.NONE;
        pompObj.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Start_Jump()
    {
        isJumping = true;
        manAnimator.Play("Jumping");
        manAnimator.SetBool("Jumping", false);
        manAnimator.SetBool("Fault Drop", false);
        manAnimator.SetBool("Drop", false);
        manAnimator.SetBool("Fly", false);
        manAnimator.SetBool("Entry", false);
        manAnimator.SetBool("Star", false);
        manAnimator.SetBool("Good", false);
        manAnimator.SetBool("Greet", false);
    }

    public void Show_Pomp()
    {
        if (pompObj.activeSelf)
            return;
        pompObj.SetActive(true);
    }

    public void Hide_Pomp()
    {
        if (!pompObj.activeSelf)
            return;
        pompObj.SetActive(false);
    }

    public void Set_Move_Direction(GAME1_MAN_MOVE_DIRECTION direction)
    {
        moveDirection = direction;
        Debug.Log("--- Set Move Direction: " + moveDirection + " ---");
    }

    public void End_Jump()
    {
        Debug.Log("--- End Jump ---");
        switch (moveDirection)
        {
            case GAME1_MAN_MOVE_DIRECTION.CL:
                manAnimation.Play("Move C-L");
                break;
            case GAME1_MAN_MOVE_DIRECTION.CR:
                manAnimation.Play("Move C-R");
                break;
            case GAME1_MAN_MOVE_DIRECTION.LC:
                manAnimation.Play("Move L-C");
                break;
            case GAME1_MAN_MOVE_DIRECTION.LR:
                manAnimation.Play("Move L-R");
                break;
            case GAME1_MAN_MOVE_DIRECTION.RC:
                manAnimation.Play("Move R-C");
                break;
            case GAME1_MAN_MOVE_DIRECTION.RL:
                manAnimation.Play("Move R-L");
                break;
        }
        moveDirection = GAME1_MAN_MOVE_DIRECTION.NONE;
        isJumping = false;
    }

    public void Drop()
    {
        pompObj.SetActive(false);
        manAnimator.SetBool("Jumping", false);
        manAnimator.SetBool("Fault Drop", false);
        manAnimator.SetBool("Drop", true);
        manAnimator.SetBool("Fly", false);
        manAnimator.SetBool("Entry", false);
        manAnimator.SetBool("Start", false);
        manAnimator.SetBool("Good", false);
        manAnimator.SetBool("Greet", false);

        End_Jump();
    }

    public void Fault_Drop()
    {
        pompObj.SetActive(false);
        manAnimator.SetBool("Jumping", false);
        manAnimator.SetBool("Fault Drop", true);
        manAnimator.SetBool("Drop", false);
        manAnimator.SetBool("Fly", false);
        manAnimator.SetBool("Entry", false);
        manAnimator.SetBool("Start", false);
        manAnimator.SetBool("Good", false);
        manAnimator.SetBool("Greet", false);

        GetComponent<tazo_rotate>().enabled = true;

        isJumping = false;
    }

    public void Fly()
    {
        pompObj.SetActive(false);

        manAnimator.SetBool("Jumping", false);
        manAnimator.SetBool("Fault Drop", false);
        manAnimator.SetBool("Drop", false);
        manAnimator.SetBool("Fly", true);
        manAnimator.SetBool("Entry", false);
        manAnimator.SetBool("Start", false);
        manAnimator.SetBool("Good", false);
        manAnimator.SetBool("Greet", false);

        StartCoroutine(Fly_Progress());
    }

    IEnumerator Fly_Progress()
    {
        yield return new WaitForSeconds(0.5f);

        flyEffectObj.SetActive(true);
        Sound_Manager.instance.Play_Game1_Character_Rocket_Sound();

        yield return new WaitForSeconds(3.5f);
        flyEffectObj.SetActive(false);
    }

    public void Greet()
    {
        pompObj.SetActive(false);

        manAnimation.Play("Greet");

        manAnimator.SetBool("Jumping", false);
        manAnimator.SetBool("Fault Drop", false);
        manAnimator.SetBool("Drop", false);
        manAnimator.SetBool("Fly", false);
        manAnimator.SetBool("Entry", false);
        manAnimator.SetBool("Star", false);
        manAnimator.SetBool("Good", false);
        manAnimator.SetBool("Greet", true);
    }

    public void Hide_Man()
    {
        gameObject.SetActive(false);
    }
}
