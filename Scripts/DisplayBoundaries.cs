using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

/*
 * Scripts that handles the display of the boundary
 */

public class DisplayBoundaries : MonoBehaviour
{
    private enum DisplayMode {
        Line,
        Mesh,
        Both,
        None
    }

    [SerializeField] private DisplayMode displayMode;

    private LineRenderer _lineRenderer;
    private MeshRenderer _meshRenderer;

    private DisplayMode _currentDisplayMode;

    public void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _meshRenderer = GetComponent<MeshRenderer>();
        
        InitMaterials();

        SetDisplayMode();

        InitInputSubsystem();
    }

    private void Update()
    {
        if (_currentDisplayMode == displayMode) return;

        SetDisplayMode();
        
        _currentDisplayMode = displayMode;
    }

    private void InitInputSubsystem()
    {
        var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
        if (loader == null) return;
        var inputSubsystem = loader.GetLoadedSubsystem<XRInputSubsystem>();
        inputSubsystem.boundaryChanged += UpdateCurrentBoundaries;

        UpdateCurrentBoundaries(inputSubsystem);
    }

    private void UpdateCurrentBoundaries(XRInputSubsystem inputSubsystem)
    {
        List<Vector3> boundaryPoints = new List<Vector3>();
        if (inputSubsystem.TryGetBoundaryPoints(boundaryPoints))
            UpdateBoundariesRenderers(boundaryPoints);
        else
            Debug.Log($"Could not get Boundary Points for Loader");
    }

    private void UpdateBoundariesRenderers(List<Vector3> boundaryPoints)
    {
        if (displayMode is DisplayMode.Line or DisplayMode.Both)
        {
            _lineRenderer.positionCount = boundaryPoints.Count;
            _lineRenderer.SetPositions(boundaryPoints.ToArray());            
        }

        if (displayMode is DisplayMode.Mesh or DisplayMode.Both)
        {
            Mesh m = new Mesh
            {
                name = "Scripted_Plane_New_Mesh",
                vertices = boundaryPoints.ToArray(),
                uv = new Vector2[] { new(0, 0), new(0, 1), new(1, 1), new(1, 0) },
                triangles = new [] {0, 1, 2, 0, 2, 3 }
            };
            
            m.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = m;
        }
    }

    private void InitMaterials()
    {
        if (_meshRenderer.material == null)
            _meshRenderer.material = new Material(Shader.Find("Standard"));

        if (_lineRenderer.material == null)
            _lineRenderer.material = new Material(Shader.Find("Standard"));
    }

    private void SetDisplayMode()
    {
        _lineRenderer.enabled = false;
        _meshRenderer.enabled = false;
        
        if (displayMode is DisplayMode.Line or DisplayMode.Both)
            _lineRenderer.enabled = true;
        if (displayMode is DisplayMode.Mesh or DisplayMode.Both)
            _meshRenderer.enabled = true;
    }
}