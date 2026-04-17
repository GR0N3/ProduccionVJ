using UnityEngine;

public class CameraForwardOnly : MonoBehaviour
{
    private float maxX;

    void Start()
    {
        maxX = transform.position.x;
    }

    void OnEnable()
    {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += OnCameraRender;
    }

    void OnDisable()
    {
        UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= OnCameraRender;
    }

    void OnCameraRender(UnityEngine.Rendering.ScriptableRenderContext context, Camera cam)
    {
        if (cam != Camera.main) return;

        Vector3 pos = cam.transform.position;

        if (pos.x > maxX)
        {
            maxX = pos.x;
        }

        cam.transform.position = new Vector3(maxX, pos.y, pos.z);
    }
}