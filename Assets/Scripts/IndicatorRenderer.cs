using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class IndicatorRenderer : MonoBehaviour
{
    [Header("位置设置")]
    public Vector3 location = Vector3.zero;

    [Header("正方形设置")]
    public Color squareColor = Color.green;
    public float squareLineWidth = 0.05f;

    [Header("圆圈设置")]
    public Color circleColor = Color.red;
    public float circleLineWidth = 0.05f;
    public float circleRadius = 30f;
    public int circleSegmentCount = 100;

    [Header("渲染排序设置")]
    public int sortingOrder = 10; // 比背景大的 order

    private GameObject squareObj;
    private GameObject circleObj;

    void Start()
    {
        // 创建正方形
        squareObj = new GameObject("SquareIndicator");
        squareObj.transform.parent = transform;
        squareObj.transform.localPosition = Vector3.zero;
        var squareLR = squareObj.AddComponent<LineRenderer>();
        SetupLineRenderer(squareLR, squareColor, squareLineWidth);
        DrawHollowSquare(squareLR);

        // 创建圆圈
        circleObj = new GameObject("CircleIndicator");
        circleObj.transform.parent = transform;
        circleObj.transform.localPosition = Vector3.zero;
        var circleLR = circleObj.AddComponent<LineRenderer>();
        SetupLineRenderer(circleLR, circleColor, circleLineWidth);
        DrawHollowCircle(circleLR, Mathf.Sqrt(circleRadius), circleSegmentCount);
    }

    // 初始化 LineRenderer 的公共设置
    void SetupLineRenderer(LineRenderer lr, Color color, float width)
    {
        lr.material = new Material(Shader.Find("Sprites/Default")); // 使用默认 Sprites shader
        lr.widthMultiplier = width;
        lr.loop = true; // 使线条闭合
        lr.startColor = color;
        lr.endColor = color;
        // 设置排序（如果希望在背景之上，确保此值高于背景对象）
        lr.sortingOrder = sortingOrder;
    }

    // 绘制以 location 为中心、边长为1 的空心正方形
    void DrawHollowSquare(LineRenderer lr)
    {
        lr.positionCount = 5;
        Vector3[] positions = new Vector3[5];
        positions[0] = location + new Vector3(-0.5f, -0.5f, 0);
        positions[1] = location + new Vector3(-0.5f, 0.5f, 0);
        positions[2] = location + new Vector3(0.5f, 0.5f, 0);
        positions[3] = location + new Vector3(0.5f, -0.5f, 0);
        positions[4] = positions[0]; // 封闭正方形
        lr.SetPositions(positions);
    }

    // 绘制以 location 为中心、半径可调节的空心圆圈
    void DrawHollowCircle(LineRenderer lr, float radius, int segmentCount)
    {
        lr.positionCount = segmentCount;
        Vector3[] positions = new Vector3[segmentCount];

        float deltaTheta = (2f * Mathf.PI) / segmentCount;
        float theta = 0f;

        for (int i = 0; i < segmentCount; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            positions[i] = location + new Vector3(x, y, 0);
            theta += deltaTheta;
        }
        lr.SetPositions(positions);
    }

    // 如果需要在 Inspector 动态调整圆半径或位置，可以在 Update 中实时更新
    void Update()
    {
        if (circleObj != null)
        {
            var lr = circleObj.GetComponent<LineRenderer>();
            DrawHollowCircle(lr, Mathf.Sqrt(circleRadius), circleSegmentCount);
        }
        if (squareObj != null)
        {
            var lr = squareObj.GetComponent<LineRenderer>();
            DrawHollowSquare(lr);
        }
    }
}
