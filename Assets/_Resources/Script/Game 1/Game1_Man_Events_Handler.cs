using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game1_Man_Events_Handler : MonoBehaviour
{
    [SerializeField] Game1_Man_Controller man_Controller;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Start_Jump()
    {
        Sound_Manager.instance.Play_Game1_Character_Jump_Sound();
    }

    public void Show_Pomp()
    { 
        man_Controller.Show_Pomp();
    }

    public void Hide_Pomp()
    {
        man_Controller.Hide_Pomp();
    }

    public void End_Jump()
    {
        man_Controller.End_Jump();
    }
}
