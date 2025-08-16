using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FireBase;
using Data_Type;
using System.Threading.Tasks;
using System;

public class Data_Manager : MonoBehaviour
{
    // Singleton instance
    public static Data_Manager instance;

    void Awake()
    {
        // Ensure only one instance of Data_Manager exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
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

    #region Game 1
    public int game1QACount = 3; // Number of Q&A pairs to fetch
    public async Task<List<Game1_QA>> game1_QAList(int Level)
    {
        string docId = $"Level{Level}";

        List<Game1_QA> qaList = new List<Game1_QA>();

        Dictionary<string, object> data = await Firebase_Manager.instance.Get_Document("Game 1 QA", docId);

        foreach (object qa in data)
        {
            String[] qaData = (string[])qa;
            if (qaData.Length >= 4)
            {
                Game1_QA game1QA = new Game1_QA(qaData);
                qaList.Add(game1QA);
            }
            else
            {
                Debug.LogError($"Invalid Q&A data for Level {Level}: {string.Join(", ", qaData)}");
            }
        }
        return null;
    }
    #endregion
}
