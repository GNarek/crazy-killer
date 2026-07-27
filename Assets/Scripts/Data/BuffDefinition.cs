using UnityEngine;

public enum BuffType { Damage, FireRate, MoveSpeed, Health }

[CreateAssetMenu(fileName = "BuffDefinition", menuName = "CrazyKiller/Buff Definition")]
public class BuffDefinition : ScriptableObject
{
    public string id;
    public BuffType type;
    public float value = 1f;
    public float duration = 0f;
    public Sprite icon;
}
