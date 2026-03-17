using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
public class SlimeSupply : MonoBehaviour
{
    [Header("Water mesh")]
    [SerializeField] private int verticesAmountPerSide;
    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private Material material;
    [SerializeField] private BoxCollider2D edgeCollider;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private SpriteRenderer placeholder;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] topVerticesIndex;

    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (placeholder != null)
                placeholder.enabled = false;
        }
        else
        {
            if (placeholder != null)
                placeholder.enabled = true;
        }
    }

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        mesh = new();
        vertices = new Vector3[verticesAmountPerSide * 2];
        topVerticesIndex = new int[verticesAmountPerSide];
        for (int y = 0; y < 2; y++)
            for (int x = 0; x < verticesAmountPerSide; x++)
            {
                float xPos = (x / (float) (verticesAmountPerSide - 1)) * width - width / 2;
                float yPos = y * height - height / 2;
                vertices[y * verticesAmountPerSide + x] = new Vector3(xPos, yPos, 0f);
                if (y == 1)
                {
                    topVerticesIndex[x] = y * verticesAmountPerSide + x;
                }
            }

        int[] triangles = new int[(verticesAmountPerSide - 1) * 6]; // 6 vertices are needed to build a square with 2 triangles
        int index = 0;
        for (int y = 0; y < 1; y++)
        {
            for (int x = 0; x < verticesAmountPerSide - 1; x++)
            {
                int bottomLeft = y * verticesAmountPerSide + x;
                int topLeft = bottomLeft + verticesAmountPerSide;
                int topRight = topLeft + 1;
                int bottomRight = bottomLeft + 1;

                // create left triangle
                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;

                // create right
                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
            }
        }
        Vector2[] uv = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            uv[i] = new Vector2((vertices[i].x + width / 2) / width, (vertices[i].y + height / 2) / height);
        meshRenderer.material = material;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;
    }
}
