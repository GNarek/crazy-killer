using UnityEngine;

public class FloatingText : MonoBehaviour
{
    public static void Spawn(string text, Vector3 worldPosition, Color color)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = worldPosition;

        TextMesh textMesh = go.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.color = color;
        textMesh.fontSize = 48;
        textMesh.characterSize = 0.15f;
        textMesh.anchor = TextAnchor.LowerCenter;
        textMesh.alignment = TextAlignment.Center;

        go.AddComponent<FloatingText>();
    }

    private const float Duration = 1f;
    private const float RiseSpeed = 1.5f;

    private float timer;
    private TextMesh textMesh;
    private Camera targetCamera;

    private void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        targetCamera = Camera.main;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * RiseSpeed * Time.deltaTime;

        if (targetCamera != null)
        {
            transform.rotation = targetCamera.transform.rotation;
        }

        Color color = textMesh.color;
        color.a = Mathf.Clamp01(1f - timer / Duration);
        textMesh.color = color;

        if (timer >= Duration)
        {
            Destroy(gameObject);
        }
    }
}
