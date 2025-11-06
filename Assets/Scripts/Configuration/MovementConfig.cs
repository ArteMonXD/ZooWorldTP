using UnityEngine;

[CreateAssetMenu(menuName = "ZooWorld/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    public float MoveSpeed = 2f;
    public float JumpInterval = 2f;
    public float JumpForce = 5f;
    public float DirectionChangeInterval = 3f;
    public float JumpCooldown = 0.1f;
}