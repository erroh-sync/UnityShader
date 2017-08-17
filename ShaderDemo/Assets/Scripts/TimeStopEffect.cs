using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TimeStopEffect : MonoBehaviour
{
    public Transform ScannerOrigin;
    public Material EffectMaterial;
    public float pulseDuration;
    public float basePulseSpeed;
    public float pulseSpeed;
    public float maxDistance;

    private Camera _camera;

    // Demo Code
    bool _theWorlding;

    void Start()
    {
    }

    void Update()
    {
        if (_theWorlding)
        {
            pulseDuration += Time.deltaTime * pulseSpeed;

            if (pulseDuration < Mathf.PI / 2)
                pulseSpeed = Mathf.Lerp(pulseSpeed, basePulseSpeed / 2, Time.deltaTime * 4);
            else
                pulseSpeed = Mathf.Lerp(pulseSpeed, basePulseSpeed * 4, Time.deltaTime * 2);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _theWorlding = true;
            pulseDuration = 0;
            pulseSpeed = basePulseSpeed;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                _theWorlding = true;
                pulseDuration = 0;
                pulseSpeed = basePulseSpeed;
                ScannerOrigin.position = hit.point;
            }
        }
    }
    // End Demo Code

    float getDistance()
    {
        return Mathf.Lerp(0, maxDistance, Mathf.Abs(Mathf.Sin(Mathf.Clamp(pulseDuration, 0, Mathf.PI))));
    }

    void OnEnable()
    {
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        EffectMaterial.SetVector("_WorldSpaceScannerPos", ScannerOrigin.position);
        EffectMaterial.SetFloat("_ScanDistance", getDistance());
        RaycastCornerBlit(src, dst, EffectMaterial);
    }

    void RaycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat)
    {
        // Compute Frustum Corners
        float camFar = _camera.farClipPlane;
        float camFov = _camera.fieldOfView;
        float camAspect = _camera.aspect;

        float fovWHalf = camFov * 0.5f;

        Vector3 toRight = _camera.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        Vector3 toTop = _camera.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (_camera.transform.forward - toRight + toTop);
        float camScale = topLeft.magnitude * camFar;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (_camera.transform.forward + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (_camera.transform.forward + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (_camera.transform.forward - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        // Custom Blit, encoding Frustum Corners as additional Texture Coordinates
        RenderTexture.active = dest;

        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.MultiTexCoord(1, bottomLeft);
        GL.Vertex3(0.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.MultiTexCoord(1, bottomRight);
        GL.Vertex3(1.0f, 0.0f, 0.0f);

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.MultiTexCoord(1, topRight);
        GL.Vertex3(1.0f, 1.0f, 0.0f);

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.MultiTexCoord(1, topLeft);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }
}
