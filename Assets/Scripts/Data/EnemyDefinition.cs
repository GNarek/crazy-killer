using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition", menuName = "CrazyKiller/Enemy Definition")]
public class EnemyDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;
    public float maxHealth = 10f;
    public float moveSpeed = 2f;
    public float damage = 1f;
    public int scoreValue = 1;
}
