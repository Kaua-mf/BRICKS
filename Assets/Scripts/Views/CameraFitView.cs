using UnityEngine;


[RequireComponent(typeof(Camera))]
public class CameraFitView : MonoBehaviour
{
    [SerializeField] private float _targetAspect = 0.6f;   
    [SerializeField] private float _orthoSize    = 5f;     

    private Camera _camera;
    private int _width, _height;

    void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic     = true;
        _camera.orthographicSize = _orthoSize;
        CreateBarsCamera();
        Apply();
    }

    void Update()
    {
        if (Screen.width != _width || Screen.height != _height) Apply();
    }

    private void CreateBarsCamera()
    {
        GameObject go = new GameObject("BarsCamera");
        go.transform.SetParent(transform, false);
        Camera bars = go.AddComponent<Camera>();
        bars.clearFlags      = CameraClearFlags.SolidColor;
        bars.backgroundColor = Color.black;
        bars.cullingMask     = 0;
        bars.depth           = _camera.depth - 1f;
    }

    private void Apply()
    {
        _width  = Screen.width;
        _height = Screen.height;
        float window = (float)_width / _height;

        Rect r;
        if (window > _targetAspect)
        {
            float w = _targetAspect / window;
            r = new Rect((1f - w) * 0.5f, 0f, w, 1f);
        }
        else
        {
            float h = window / _targetAspect;
            r = new Rect(0f, (1f - h) * 0.5f, 1f, h);
        }
        _camera.rect = r;
    }
}
