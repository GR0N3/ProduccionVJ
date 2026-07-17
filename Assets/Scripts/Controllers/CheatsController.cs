using UnityEngine;
using UnityEngine.InputSystem;

public class CheatsController : MonoBehaviour
{
    private static CheatsController instance;

    private SessionController sessionController;
    private PlayerManager playerManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {

        GameObject cheatsControllerObject = new("CheatsController");
        cheatsControllerObject.AddComponent<CheatsController>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null || !IsControlPressed(keyboard))
        {
            return;
        }

        ResolveServices();

        if (keyboard.gKey.wasPressedThisFrame)
        {
            ToggleGodMode();
            return;
        }

        if (sessionController == null)
        {
            return;
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            sessionController.RestartCurrentLevel();
            return;
        }

        if (keyboard.fKey.wasPressedThisFrame)
        {
            sessionController.FinalizeCurrentLevel();
            return;
        }

        if (WasLevelShortcutPressed(keyboard, 0))
        {
            sessionController.LoadLevelByIndex(0);
            return;
        }

        if (WasLevelShortcutPressed(keyboard, 1))
        {
            sessionController.LoadLevelByIndex(1);
            return;
        }

        if (WasLevelShortcutPressed(keyboard, 2))
        {
            sessionController.LoadLevelByIndex(2);
            return;
        }

        if (WasLevelShortcutPressed(keyboard, 3))
        {
            sessionController.LoadLevelByIndex(3);
            return;
        }

        if (WasLevelShortcutPressed(keyboard, 4))
        {
            sessionController.LoadLevelByIndex(4);
        }
    }

    private void ResolveServices()
    {
        if (sessionController == null)
        {
            ServiceLocator.TryGet(out sessionController);
        }

        if (playerManager == null)
        {
            ServiceLocator.TryGet(out playerManager);
        }
    }

    private void ToggleGodMode()
    {
        if (playerManager == null)
        {
            return;
        }

        playerManager.ToggleGodMode();
        Debug.Log($"GodMode: {playerManager.IsGodMode}");
    }

    private static bool IsControlPressed(Keyboard keyboard)
    {
        return keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed;
    }

    private static bool WasLevelShortcutPressed(Keyboard keyboard, int levelIndex)
    {
        return levelIndex switch
        {
            0 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
            1 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
            2 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
            3 => keyboard.digit4Key.wasPressedThisFrame || keyboard.numpad4Key.wasPressedThisFrame,
            4 => keyboard.digit5Key.wasPressedThisFrame || keyboard.numpad5Key.wasPressedThisFrame,
            _ => false,
        };
    }
}
