using UnityEngine;

[RequireComponent(typeof(BuffReceiver))]
public class BuffPopupSpawner : MonoBehaviour
{
    private BuffReceiver receiver;

    private void Awake()
    {
        receiver = GetComponent<BuffReceiver>();
        receiver.BuffApplied += HandleBuffApplied;
    }

    private void OnDestroy()
    {
        if (receiver != null) receiver.BuffApplied -= HandleBuffApplied;
    }

    private void HandleBuffApplied(BuffDefinition buff)
    {
        HUDController.Instance?.ShowBuffPopup(BuffLabel(buff.type), BuffColor(buff.type));
    }

    private static string BuffLabel(BuffType type)
    {
        return type switch
        {
            BuffType.Damage => "+DAMAGE",
            BuffType.FireRate => "+FIRE RATE",
            BuffType.MoveSpeed => "+SPEED",
            BuffType.Health => "+HEALTH",
            BuffType.MultiShot => "+MULTI SHOT",
            _ => "+BUFF"
        };
    }

    private static Color BuffColor(BuffType type)
    {
        return type switch
        {
            BuffType.Damage => new Color(0.8f, 0.2f, 0.7f),
            BuffType.FireRate => new Color(1f, 0.6f, 0.1f),
            BuffType.MoveSpeed => new Color(0.2f, 0.8f, 1f),
            BuffType.Health => new Color(0.2f, 1f, 0.3f),
            BuffType.MultiShot => new Color(0.1f, 0.85f, 0.85f),
            _ => Color.white
        };
    }
}
