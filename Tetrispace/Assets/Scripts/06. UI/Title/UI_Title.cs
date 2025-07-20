using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Title : UI_Base
{

    enum Images
    {
        Logo,
        Press,
        Guide
    }
    
    private void Awake()
    {
        Init();
    }
    
    public override void Init()
    {
        Bind<Image>(typeof(Images));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TetriminoManager.Instance.ResetGrid();
            GameManager.Instance.StartGame();
            gameObject.SetActive(false);
        }
    }
}
