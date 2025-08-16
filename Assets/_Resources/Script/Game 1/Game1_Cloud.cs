using System.Collections;
using System.Collections.Generic;
using Data_Type;
using UnityEngine;
using UnityEngine.UI;

public class Game1_Cloud : MonoBehaviour
{
    public CloudType type;
    [SerializeField] Image cloudImg { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        cloudImg = GetComponent<Image>();
        cloudImg.sprite = type == CloudType.Top
            ? Resource_Manager.instance.game1_topCloudSptArray[0]
            : Resource_Manager.instance.game1_bottomCloudSptArray[0];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Start_Disappear_Animation()
    {
        StartCoroutine(Disappear_Animation());
    }

    IEnumerator Disappear_Animation()
    {
        float duration = 1f; // Total animation duration in seconds
        Sprite[] spriteArray = type == CloudType.Top
            ? Resource_Manager.instance.game1_topCloudSptArray
            : Resource_Manager.instance.game1_bottomCloudSptArray;

        int spriteCount = spriteArray.Length;
        if (spriteCount == 0)
            yield break;

        float timePerSprite = duration / spriteCount;

        for (int i = 0; i < spriteCount; i++)
        {
            cloudImg.sprite = spriteArray[i];
            float elapsedTime = 0f;
            while (elapsedTime < timePerSprite)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
