using System;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    private SessionController sessionController;

    private void Awake()
    {
        sessionController = ServiceLocator.Get<SessionController>();
        AudioManager.instance.Play("Shop");
    }

    public void GoToMatch()
    {
        sessionController.GoToMatch();
    }

    private void OnDestroy()
    {
        AudioManager.instance.Stop("Shop");
    }

}
