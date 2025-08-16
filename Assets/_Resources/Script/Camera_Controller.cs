using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    public Camera mainCamera;
    float startYPos;
    float endYPos;
    float moveTime;
    AnimationCurve moveCurve;
    bool isMoving;

    // Start is called before the first frame update
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveCamera(float moveY, float time, AnimationCurve curve = null)
    {
        if (!isMoving)
        {
            startYPos = transform.position.y;
            endYPos = startYPos + moveY;
            moveTime = time;
            moveCurve = curve == null ? AnimationCurve.Linear(0, 0, 1, 1) : curve;

            StartCoroutine(Move());
        }
    }
    IEnumerator Move()
    {
        isMoving = true;
        float elapsedTime = 0;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveTime);
            float curveValue = moveCurve.Evaluate(t);
            float newYPos = Mathf.Lerp(startYPos, endYPos, curveValue);
            transform.position = new Vector3(transform.position.x, newYPos, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(transform.position.x, endYPos, transform.position.z);
        isMoving = false;
    }
}
