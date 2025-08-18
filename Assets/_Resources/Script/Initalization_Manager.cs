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

public class Initalization_Manager : MonoBehaviour
{
    [Header("LSS Manager")]
    public LSS_Manager lss_Manager;

    // Singleton
    public static Initalization_Manager instance;
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
        StartCoroutine(Show_Message());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Show_Message()
    {
        yield return new WaitForSeconds(2f);
        if (!Firebase_Manager.instance.isFirebaseInitialized)
            Debug.Log("Error : Firebase Initialization Failed. Please check your internet connection and restart the app.");

        Debug.Log("Success : Firebase Initialized Successfully!");

        Start_Game().Forget();
    }

    public async Task Start_Game()
    {
        // Start Game
        await Data_Manager.instance.Initialize();
        SceneManager.LoadScene(1);
    }
}
