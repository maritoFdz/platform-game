using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider2D))]
public class SlimeSupply : MonoBehaviour
{
    [Header("Water mesh")]
    [SerializeField] private int verticesAmountPerSide;
    [SerializeField] private float width;
    [SerializeField] private float height;
    [SerializeField] private Material material;
    [SerializeField] private BoxCollider2D col;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private SpriteRenderer placeholder;
    [SerializeField] private float maxAmount;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] topVerticesIndex;
    private float currentAmount;

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
        currentAmount = maxAmount;
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

    public float GetSurfaceHeight()
    {
        float normalized = currentAmount / maxAmount;
        return transform.position.y + (normalized * height) - height / 2;
    }

    public bool TryConsume(float amount)
    {
        if (currentAmount <= 0f)
            return false;

        currentAmount -= amount;
        currentAmount = Mathf.Max(currentAmount, 0f);

        UpdateVisual();

        if (currentAmount <= 0f)
        {
            currentAmount = 0f;
            meshRenderer.enabled = false;
            col.enabled = false;
            return false;
        }

        return true;
    }

    private void UpdateVisual()
    {
        float normalized = currentAmount / maxAmount;
        float newHeight = normalized * height;

        for (int i = 0; i < topVerticesIndex.Length; i++)
        {
            int index = topVerticesIndex[i];
            vertices[index].y = newHeight - height / 2;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();

        col.size = new Vector2 (col.size.x, newHeight);
        // keeps collider pivot top down
        col.offset = new Vector2(0f, (newHeight - height) / 2f - 0.25f);
    }

    public bool HasSlime()
    {
        return currentAmount > 0.01f;
    }
}
