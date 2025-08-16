using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource_Manager : MonoBehaviour
{
    // Singleton
    public static Resource_Manager instance;
    void Awake()
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

    #region Game 1 Data
    [Header("Game 1 Data")]
    public Sprite[] game1_qaVideoMaskSptArray;
    public Sprite[] game1_qaBtnBgSptArray;
    public Sprite[] game1_topCloudSptArray;
    public Sprite[] game1_bottomCloudSptArray;
    public Sprite[] game1_stageSptArray;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
