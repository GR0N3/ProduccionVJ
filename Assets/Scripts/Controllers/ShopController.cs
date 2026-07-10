using System;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    private SessionController sessionController;

    private void Awake()
    {
        sessionController = ServiceLocator.Get<SessionController>();
    }

    public void GoToMatch()
    {
        sessionController.GoToMatch();
    }

    

}
