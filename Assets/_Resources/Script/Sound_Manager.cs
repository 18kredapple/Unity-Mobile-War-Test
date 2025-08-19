using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound_Manager : MonoBehaviour
{
    // Singleton instance
    public static Sound_Manager instance;

    [Header("Game 1 Sounds")]
    [SerializeField] AudioClip game1_FundClip;
    [SerializeField] AudioClip game1_JumpClip;
    [SerializeField] AudioClip game1_RocketClip;
    [SerializeField] AudioClip game1_TrueAnswerClip;
    [SerializeField] AudioClip game1_FalseAnswerClip;
    [SerializeField] AudioClip game1_BtnClickClip;

    [SerializeField] AudioSource characterAudioSource;
    [SerializeField] AudioSource bgAudioSource;
    [SerializeField] AudioSource uiAudioSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Play_UI_Btn_Click_Sound()
    {
        if (uiAudioSource == null || game1_FundClip == null)
            return;
        uiAudioSource.PlayOneShot(game1_BtnClickClip);
    }

    public void Play_Game1_Character_Jump_Sound()
    {
        if (characterAudioSource == null || game1_JumpClip == null)
            return;
        characterAudioSource.PlayOneShot(game1_JumpClip);
    }

    public void Play_Game1_Character_Rocket_Sound()
    {
        if (characterAudioSource == null || game1_RocketClip == null)
            return;
        characterAudioSource.PlayOneShot(game1_RocketClip);
    }

    public void Play_Game1_True_Answer_Sound()
    {
        if (uiAudioSource == null || uiAudioSource == null)
            return;
        uiAudioSource.PlayOneShot(game1_TrueAnswerClip);
    }

    public void Play_Game1_False_Answer_Sound()
    {
        if (uiAudioSource == null || uiAudioSource == null)
            return;
        uiAudioSource.PlayOneShot(game1_FalseAnswerClip);
    }

    public void Stop_Game1_BackgroundSound()
    {
        bgAudioSource.Stop();
    }
}
