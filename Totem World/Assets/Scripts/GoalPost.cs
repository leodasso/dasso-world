using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalPost : MonoBehaviour
{
    public Stage stage;

    public void CompleteStage()
    {
        Debug.Log("Stage is complete bro");
       GameMaster.Get().CompleteStage();
    }
}
