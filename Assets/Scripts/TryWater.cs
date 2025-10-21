// using UnityEngine;

// namespace Bundos.WaterSystem
// {
//     public class Spring
//     {
//         public Vector2 weightPosition, sineOffset, velocity, acceleration;
//     }

//     public enum TangentMode { Auto, Flat, Custom }

//     [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
//     public class Water : MonoBehaviour
//     {
//         [Header("Dynamic Wave Settings")]
//         public bool interactive = true;
//         public float splashInfluence = 0.005f;
//         public float waveHeight = .25f;

//         [Header("Constant Waves Settings")]
//         public bool hasConstantWaves = true;
//         public float waveAmplitude = 1f;
//         public float waveSpeed = 1f;
//         public int waveStep = 1;

//         [Header("Spring Settings (Physics)")]
//         public int numSprings = 10;
//         public float spacing = 1f;
//         public float springConstant = 0.05f;
//         public float springDamping = 0.25f;

//         [Header("Splash/Ripple Settings")]
//         public int splashRadius = 6;            // neighbors on each side (in springs)
//         public float splashSharpness = 0.5f;    // 0.2–1.0 (smaller = wider bump)
//         public bool impulseAffectsVelocity = true;

//         [Header("Spline Rendering")]
//         [Min(1)] public int visualSegmentsPerSpring = 4; // >1 = real curves
//         public TangentMode tangentMode = TangentMode.Auto;
//         [Tooltip("Kochanek–Bartels parameters: 0 = Catmull-Rom-like")]
//         [Range(-1f, 1f)] public float tension = 0f;
//         [Range(-1f, 1f)] public float continuity = 0f;
//         [Range(-1f, 1f)] public float bias = 0f;
//         [Tooltip("Only used when TangentMode.Custom")]
//         public float[] customSlopeY; // per-spring slope (dy/dx) for the top edge

//         [Header("Particles")]
//         public GameObject splashParticle;

//         // Runtime
//         Spring[] springs;
//         MeshFilter meshFilter;
//         Mesh mesh;

//         // Mesh data (visual)
//         public Vector2[] vertices, baseVertecies;
//         public int[] triangles;
//         Vector2[] uvs;

//         // Internal spline cache
//         float[] slopeY; // computed/used per-spring slope on y

//         private void Awake()
//         {
//             Initialize();
//             InitializeSprings();
//             CreateShape();
//         }

//         public void Initialize()
//         {
//             mesh = new Mesh() { name = "WaterMesh" };
//             meshFilter = GetComponent<MeshFilter>();
//             meshFilter.mesh = mesh;
//         }

//         private void InitializeSprings()
//         {
//             springs = new Spring[numSprings];
//             for (int i = 0; i < numSprings; i++)
//             {
//                 springs[i] = new Spring
//                 {
//                     weightPosition = Vector2.zero,
//                     sineOffset = Vector2.zero,
//                     velocity = Vector2.zero,
//                     acceleration = Vector2.zero
//                 };
//             }

//             // prepare slope array
//             slopeY = new float[numSprings];
//             if (customSlopeY == null || customSlopeY.Length != numSprings)
//                 customSlopeY = new float[numSprings]; // default zeros
//         }

//         public void CreateShape()
//         {
//             int visualCount = Mathf.Max(2, numSprings * visualSegmentsPerSpring);

//             // Vertices (2 rows: bottom y=0, top y=1; X spread by spacing/segments)
//             vertices = new Vector2[visualCount * 2];
//             for (int i = 0; i < visualCount; i++)
//             {
//                 float x = i * spacing / (float)visualSegmentsPerSpring;
//                 vertices[(2 * i) + 0] = new Vector2(x, 0f); // bottom
//                 vertices[(2 * i) + 1] = new Vector2(x, 1f); // top (will be curved)
//             }

//             baseVertecies = new Vector2[vertices.Length];
//             vertices.CopyTo(baseVertecies, 0);

//             // Triangles
//             triangles = new int[((visualCount * 2) - 2) * 3];
//             int vert = 0;
//             int tris = 0;
//             for (int x = 0; x < visualCount - 1; x++)
//             {
//                 triangles[tris + 0] = vert + 0;
//                 triangles[tris + 1] = vert + 1;
//                 triangles[tris + 2] = vert + 3;

//                 triangles[tris + 3] = vert + 0;
//                 triangles[tris + 4] = vert + 3;
//                 triangles[tris + 5] = vert + 2;

//                 vert += 2;
//                 tris += 6;
//             }

//             // UVs
//             uvs = new Vector2[vertices.Length];
//             for (int i = 0; i < visualCount; i++)
//             {
//                 float u = i / (float)(visualCount - 1);
//                 uvs[(2 * i) + 0] = new Vector2(u, 0f);
//                 uvs[(2 * i) + 1] = new Vector2(u, 1f);
//             }

//             // Seed initial curve
//             UpdateMeshVerticePositions();
//             UpdateMesh();
//         }

//         private Vector3[] ConvertVector2ArrayToVector3(Vector2[] v2)
//         {
//             Vector3[] v3 = new Vector3[v2.Length];
//             for (int i = 0; i < v2.Length; i++)
//                 v3[i] = new Vector3(v2[i].x, v2[i].y, 0);
//             return v3;
//         }

//         private void Update()
//         {
//             UpdateSpringPositions();
//             UpdateMeshVerticePositions();
//             UpdateMesh();
//         }

//         // ----------------- Spline core -----------------

//         // Hermite basis on scalar y, with tangents m0,m1 (slopes dy/dx) and segment width dx
//         private static float HermiteY(float y0, float y1, float m0, float m1, float t, float dx)
//         {
//             float t2 = t * t, t3 = t2 * t;
//             float h00 =  2f * t3 - 3f * t2 + 1f;
//             float h10 =        t3 - 2f * t2 + t;
//             float h01 = -2f * t3 + 3f * t2;
//             float h11 =        t3 -       t2;
//             return h00 * y0 + h10 * (m0 * dx) + h01 * y1 + h11 * (m1 * dx);
//         }

//         // Kochanek–Bartels slope (dy/dx) for control point i
//         private float ComputeKBslope(int i, Vector2[] ctrl)
//         {
//             // End handling: clamp neighbors
//             int iPrev = Mathf.Max(0, i - 1);
//             int iNext = Mathf.Min(numSprings - 1, i + 1);

//             float x0 = ctrl[iPrev].x, y0 = ctrl[iPrev].y;
//             float x1 = ctrl[i].x,     y1 = ctrl[i].y;
//             float x2 = ctrl[iNext].x, y2 = ctrl[iNext].y;

//             float dx1 = Mathf.Max(1e-6f, x1 - x0);
//             float dx2 = Mathf.Max(1e-6f, x2 - x1);
//             float dy1 = (y1 - y0) / dx1;
//             float dy2 = (y2 - y1) / dx2;

//             float T = tension;
//             float C = continuity;
//             float B = bias;

//             // From Kochanek–Bartels formulation (averaged)
//             float m = 0.5f * (1 - T) * (
//                         (1 + C) * (1 + B) * dy1 +
//                         (1 - C) * (1 - B) * dy2
//                     );
//             return m;
//         }

//         private void ComputeTangents(Vector2[] ctrl)
//         {
//             for (int i = 0; i < numSprings; i++)
//             {
//                 switch (tangentMode)
//                 {
//                     case TangentMode.Flat:
//                         slopeY[i] = 0f;
//                         break;

//                     case TangentMode.Custom:
//                         // use provided customSlopeY (dy/dx)
//                         slopeY[i] = (customSlopeY != null && customSlopeY.Length == numSprings)
//                                     ? customSlopeY[i] : 0f;
//                         break;

//                     default: // Auto (Kochanek–Bartels)
//                         slopeY[i] = ComputeKBslope(i, ctrl);
//                         break;
//                 }
//             }
//         }

//         private void UpdateMeshVerticePositions()
//         {
//             // 1) Build control points from springs (top edge)
//             Vector2[] ctrl = new Vector2[numSprings];
//             for (int i = 0; i < numSprings; i++)
//             {
//                 float x = i * spacing;
//                 float y = 1f + springs[i].weightPosition.y + springs[i].sineOffset.y;
//                 ctrl[i] = new Vector2(x, y);
//             }

//             // 2) Compute per-spring tangents (slopes dy/dx)
//             if (slopeY == null || slopeY.Length != numSprings)
//                 slopeY = new float[numSprings];
//             ComputeTangents(ctrl);

//             // 3) Fill the visual mesh rows
//             int visualCount = Mathf.Max(2, numSprings * visualSegmentsPerSpring);

//             // bottom row stays flat at y=0
//             for (int i = 0; i < visualCount; i++)
//             {
//                 Vector2 v0 = vertices[(2 * i) + 0];
//                 vertices[(2 * i) + 0] = new Vector2(v0.x, 0f);
//             }

//             // top row: sample Hermite per segment [i .. i+1]
//             for (int i = 0; i < visualCount; i++)
//             {
//                 float s = (i / (float)(visualCount - 1)) * (numSprings - 1);
//                 int i0 = Mathf.FloorToInt(s);
//                 float t = Mathf.Clamp01(s - i0);

//                 int i1 = Mathf.Min(numSprings - 1, i0 + 1);

//                 float x0 = ctrl[i0].x;
//                 float y0 = ctrl[i0].y;
//                 float x1 = ctrl[i1].x;
//                 float y1 = ctrl[i1].y;

//                 float m0 = slopeY[i0];
//                 float m1 = slopeY[i1];

//                 float dx = Mathf.Max(1e-6f, x1 - x0);
//                 float y = HermiteY(y0, y1, m0, m1, t, dx);

//                 float xVisual = i * spacing / (float)visualSegmentsPerSpring;
//                 vertices[(2 * i) + 1] = new Vector2(xVisual, y);
//             }
//         }

//         private void UpdateSpringPositions()
//         {
//             // simple coupled mass-spring update
//             for (int i = 0; i < springs.Length; i++)
//             {
//                 springs[i].acceleration = (-springConstant * springs[i].weightPosition.y) * Vector2.up
//                                           - (springs[i].velocity * springDamping);

//                 if (i > 0)
//                 {
//                     float leftDelta = splashInfluence * (springs[i].acceleration.y - springs[i - 1].acceleration.y);
//                     springs[i].velocity += leftDelta * Vector2.up;
//                 }

//                 if (i < springs.Length - 1)
//                 {
//                     float rightDelta = splashInfluence * (springs[i].acceleration.y - springs[i + 1].acceleration.y);
//                     springs[i].velocity += rightDelta * Vector2.up;
//                 }

//                 springs[i].velocity += springs[i].acceleration;

//                 if (hasConstantWaves)
//                 {
//                     springs[i].sineOffset = new Vector2(
//                         0f,
//                         waveAmplitude * Mathf.Sin((Time.realtimeSinceStartup * waveSpeed) + i * waveStep)
//                     );
//                 }

//                 springs[i].weightPosition += springs[i].velocity;
//             }
//         }

//         public void UpdateMesh()
//         {
//             mesh.Clear();
//             mesh.vertices = ConvertVector2ArrayToVector3(vertices);
//             mesh.triangles = triangles;
//             mesh.uv = uvs;
//             mesh.RecalculateNormals();
//         }

//         private void Ripple(Vector3 contactPoint, bool sink)
//         {
//             Vector3 local = transform.InverseTransformPoint(contactPoint);

//             int center = Mathf.RoundToInt(local.x / spacing);
//             center = Mathf.Clamp(center, 0, numSprings - 1);

//             float amp = (sink ? -1f : 1f) * waveHeight;

//             int range = Mathf.Max(1, splashRadius);
//             float sigma = Mathf.Max(0.001f, range * splashSharpness);
//             float twoSigma2 = 2f * sigma * sigma;

//             for (int o = -range; o <= range; o++)
//             {
//                 int idx = center + o;
//                 if (idx < 0 || idx >= numSprings) continue;

//                 float falloff = Mathf.Exp(-(o * o) / twoSigma2);

//                 if (impulseAffectsVelocity)
//                     springs[idx].velocity.y += amp * falloff;
//                 else
//                     springs[idx].weightPosition.y += amp * falloff;
//             }
//         }

//         void OnTriggerEnter2D(Collider2D other)
//         {
//             if (!interactive) return;
//             if (other.TryGetComponent<Rigidbody2D>(out var rb))
//             {
//                 Vector2 contactPoint = other.ClosestPoint(transform.position);
//                 Ripple(contactPoint, false);
//             }
//         }

//         void OnTriggerExit2D(Collider2D other)
//         {
//             if (!interactive) return;
//             if (other.TryGetComponent<Rigidbody2D>(out var rb))
//             {
//                 Vector2 contactPoint = other.ClosestPoint(transform.position);
//                 Ripple(contactPoint, true);
//             }
//         }
//     }
// }
