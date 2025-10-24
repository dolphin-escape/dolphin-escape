using UnityEngine;

// [ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DynamicWaterSurface : MonoBehaviour
{
    [Header("Water Dimensions")]
    [SerializeField] private float width = 10f;
    [SerializeField] private float height = 5f;
    [SerializeField] private int segments = 50; // Higher = smoother waves

    [Header("Wave Settings")]
    [SerializeField] private float waveSpeed = 1f;
    [SerializeField] private float waveAmplitude = 0.3f;
    [SerializeField] private float noiseScale = 2f;
    [SerializeField] private float domainWarpStrength = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private Color waterColor = new Color(0.2f, 0.6f, 0.9f, 0.7f);
    [SerializeField] private Color deepWaterColor = new Color(0.1f, 0.3f, 0.6f, 0.9f);
    [SerializeField] private float refractionStrength = 0.1f;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material waterMaterial;
    private Mesh waterMesh;

    void Start()
    {
        GenerateWaterMesh();
        SetupMaterial();
    }

    // void OnEnable()
    // {
    //     // Regenerate mesh when component is enabled (including in Edit Mode)
    //     if (meshFilter == null)
    //     {
    //         GenerateWaterMesh();
    //         SetupMaterial();
    //     }
    // }

    void Update()
    {
        // Update shader parameters each frame (works in Edit Mode too)
        if (waterMaterial != null)
        {
            float currentTime;
            #if UNITY_EDITOR
                currentTime = Application.isPlaying
                    ? Time.time
                    : (float)UnityEditor.EditorApplication.timeSinceStartup;
            #else
                currentTime = Time.time;
            #endif

            waterMaterial.SetFloat("_CustomTime", currentTime * waveSpeed);
            waterMaterial.SetFloat("_WaveAmplitude", waveAmplitude);
            waterMaterial.SetFloat("_NoiseScale", noiseScale);
            waterMaterial.SetFloat("_DomainWarp", domainWarpStrength);
            waterMaterial.SetColor("_WaterColor", waterColor);
            waterMaterial.SetColor("_DeepWaterColor", deepWaterColor);
            waterMaterial.SetFloat("_RefractionStrength", refractionStrength);
        }
    }

    void GenerateWaterMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        waterMesh = new Mesh();
        waterMesh.name = "Dynamic Water Surface";

        // Create vertices
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float x = -width / 2f + width * t;

            // Top vertex (surface)
            vertices[i * 2] = new Vector3(x, 0, 0);
            uvs[i * 2] = new Vector2(t, 1);

            // Bottom vertex
            vertices[i * 2 + 1] = new Vector3(x, -height, 0);
            uvs[i * 2 + 1] = new Vector2(t, 0);
        }

        // Create triangles
        int[] triangles = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 2;
            int triIndex = i * 6;

            // First triangle
            triangles[triIndex] = baseIndex;
            triangles[triIndex + 1] = baseIndex + 2;
            triangles[triIndex + 2] = baseIndex + 1;

            // Second triangle
            triangles[triIndex + 3] = baseIndex + 1;
            triangles[triIndex + 4] = baseIndex + 2;
            triangles[triIndex + 5] = baseIndex + 3;
        }

        waterMesh.vertices = vertices;
        waterMesh.uv = uvs;
        waterMesh.triangles = triangles;
        waterMesh.RecalculateNormals();
        waterMesh.RecalculateBounds();

        meshFilter.mesh = waterMesh;
    }

    void SetupMaterial()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Create material with the water shader
        Shader waterShader = Shader.Find("Custom/DynamicWater");
        if (waterShader != null)
        {
            // In Edit Mode, reuse existing material if available
            if (!Application.isPlaying && meshRenderer.sharedMaterial != null
                && meshRenderer.sharedMaterial.shader == waterShader)
            {
                waterMaterial = meshRenderer.sharedMaterial;
            }
            else
            {
                waterMaterial = new Material(waterShader);
                if (Application.isPlaying)
                {
                    meshRenderer.material = waterMaterial;
                }
                else
                {
                    meshRenderer.sharedMaterial = waterMaterial;
                }
            }
        }
        else
        {
            Debug.LogError("Water shader not found! Make sure 'Custom/DynamicWater' shader is in your project.");
        }
    }

    // void OnValidate()
    // {
    //     // Clamp values in editor
    //     segments = Mathf.Max(10, segments);
    //     width = Mathf.Max(0.1f, width);
    //     height = Mathf.Max(0.1f, height);
    //     GenerateWaterMesh();
    // }
}