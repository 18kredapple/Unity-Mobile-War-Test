using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data_Type
{
    #region Game 1
    public class Game1_QA
    {
        public string question;
        public string[] answers;
        public int correctAnswerIndex;

        public Game1_QA(string[] a)
        {
            if (a == null || a.Length < 4)
            {
                Debug.LogError("Array 'a' must have at least 4 elements: [question, correct, wrong, wrong]");
                return;
            }

            question = a[0];
            string correctAnswer = a[1];

            // Collect all answers (correct + wrong)
            answers = new string[] { a[1], a[2], a[3] };

            // Shuffle answers array
            System.Random rng = new System.Random();
            int n = answers.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = answers[k];
                answers[k] = answers[n];
                answers[n] = value;
            }

            // Find new index of correct answer
            correctAnswerIndex = System.Array.IndexOf(answers, correctAnswer);
        }
    }

    public enum CLOUD_TYPE { Top, Bottom }
    public enum GAME1_MAN_MOVE_DIRECTION { NONE, CL, CR, LC, LR, RC, RL }
    public enum ANSWER_TILE {LEFT, CENTER, RIGHT }
    #endregion
}