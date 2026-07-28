using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Health))]
public class WallHealthBarWorld : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1f, -1f);
    [SerializeField] private Vector2 barSize = new Vector2(850f, 55f);

    private Image fillImage;
    private Text hpText;
    private Health health;
    private RectTransform barRoot;
    private Camera trackedCamera;

    private void Awake()
    {
        health = GetComponent<Health>();
        trackedCamera = Camera.main;
        BuildBar();
        health.HealthChanged += HandleHealthChanged;
        HandleHealthChanged(health.Current, health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (health != null) health.HealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        if (barRoot == null || trackedCamera == null) return;

        Vector3 screenPos = trackedCamera.WorldToScreenPoint(transform.position + worldOffset);
        barRoot.position = new Vector3(screenPos.x, screenPos.y, 0f);
    }

    private void BuildBar()
    {
        GameObject hudCanvasGO = GameObject.Find("HUD");
        Transform parent = hudCanvasGO != null ? hudCanvasGO.transform : null;

        GameObject rootGO = new GameObject("WallHealthBarScreen", typeof(RectTransform));
        if (parent != null) rootGO.transform.SetParent(parent, false);

        barRoot = rootGO.GetComponent<RectTransform>();
        barRoot.anchorMin = new Vector2(0.5f, 0.5f);
        barRoot.anchorMax = new Vector2(0.5f, 0.5f);
        barRoot.pivot = new Vector2(0.5f, 0.5f);
        barRoot.sizeDelta = barSize;

        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(barRoot, false);
        Image background = bgGO.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.6f);
        RectTransform bgRT = bgGO.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(bgGO.transform, false);
        fillImage = fillGO.GetComponent<Image>();
        fillImage.color = new Color(0.85f, 0.2f, 0.2f, 1f);
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillAmount = 1f;
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("HPText", typeof(RectTransform), typeof(Text));
        textGO.transform.SetParent(barRoot, false);
        hpText = textGO.GetComponent<Text>();
        hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpText.alignment = TextAnchor.MiddleCenter;
        hpText.color = Color.white;
        hpText.fontSize = 30;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (fillImage != null) fillImage.fillAmount = max > 0f ? current / max : 0f;
        if (hpText != null) hpText.text = $"{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
    }
}
