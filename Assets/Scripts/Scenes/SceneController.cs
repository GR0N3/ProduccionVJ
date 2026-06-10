using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    #region Singleton-
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    [SerializeField] private LoadingOverlay loadingOverlay;

    private Dictionary<string, string> loadedSceneBySlot = new Dictionary<string, string>();

    private bool isBusy = false;

    //API
    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    private Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Escena cambiando en progreso");
            return null;
        }
        isBusy = true;
        return StartCoroutine(ChangeSceneCoroutine(plan));
    }

    private IEnumerator ChangeSceneCoroutine(SceneTransitionPlan plan)
    {
        if (plan.Overlay && loadingOverlay != null)
        {
            yield return loadingOverlay.FadeInBlack();
            yield return new WaitForSeconds(0.5f);
        }

        foreach (var slotkey in plan.ScenesToUnload)
        {
            yield return UnloadSceneRoutine(slotkey);
        }

        if (plan.ClearUnusedAssets) yield return CleanupUnusedAssetsRoutine();

        foreach (var kvp in plan.ScenesToLoad)
        {
            if (loadedSceneBySlot.ContainsKey(kvp.Key))
            {
                yield return UnloadSceneRoutine(kvp.Key);
            }
            yield return LoadAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }

        if (plan.Overlay && loadingOverlay != null)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    private IEnumerator LoadAdditiveRoutine(string slotKey, string sceneName, bool setActive)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) yield break;

        loadOp.allowSceneActivation = false;
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }
        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
        {
            yield return null;
        }

        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }
        loadedSceneBySlot[slotKey] = sceneName;
    }

    private IEnumerator UnloadSceneRoutine(string keyToUnload)
    {
        string sceneName = "";
        string slotKey = keyToUnload;

        if (!loadedSceneBySlot.TryGetValue(slotKey, out sceneName))
        {
            var match = loadedSceneBySlot.FirstOrDefault(x => x.Value == keyToUnload);
            if (match.Key != null)
            {
                slotKey = match.Key;
                sceneName = match.Value;
            }
            else
            {
                yield break;
            }
        }

        if (string.IsNullOrEmpty(sceneName)) yield break;

        AsyncOperation unloadOP = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadOP != null)
        {
            while (!unloadOP.isDone)
            {
                yield return null;
            }
        }
        loadedSceneBySlot.Remove(slotKey);
    }

    private IEnumerator CleanupUnusedAssetsRoutine()
    {
        AsyncOperation cleanupOp = Resources.UnloadUnusedAssets();
        while (!cleanupOp.isDone)
        {
            yield return null;
        }
    }

    public class SceneTransitionPlan
    {
        public Dictionary<string, string> ScenesToLoad { get; } = new Dictionary<string, string>();
        public List<string> ScenesToUnload { get; } = new List<string>();
        public string ActiveSceneName { get; private set; } = "";
        public bool ClearUnusedAssets { get; private set; } = false;
        public bool Overlay { get; private set; } = false;

        public SceneTransitionPlan Load(string slotKey, string sceneName, bool setActive = false)
        {
            ScenesToLoad[slotKey] = sceneName;
            if (setActive) ActiveSceneName = sceneName;
            return this;
        }

        public SceneTransitionPlan Unload(string slotOrSceneName)
        {
            ScenesToUnload.Add(slotOrSceneName);
            return this;
        }

        public SceneTransitionPlan WithOverlay()
        {
            Overlay = true;
            return this;
        }

        public SceneTransitionPlan WithClearUnusedAssets()
        {
            ClearUnusedAssets = true;
            return this;
        }

        public Coroutine Perfrom()
        {
            return SceneController.Instance.ExecutePlan(this);
        }
    }
}