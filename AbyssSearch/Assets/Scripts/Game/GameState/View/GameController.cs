using System;
using UnityEngine;

public class GameController:MonoBehaviour
{
    public PauseAbleController PauseAbleController;
    private void Awake()
    {
        PauseAbleController = PauseAbleController.Instance;
    }

    private void Update()
    {
        PauseAbleController.LogicUpdateAll();
    }
}