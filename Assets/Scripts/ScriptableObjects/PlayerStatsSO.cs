using UnityEngine;

namespace MadKnight.ScriptableObjects
{
    [CreateAssetMenu(menuName = "MadKnight/PlayerStatsSO")]
    public class PlayerStatsSO : ScriptableObject
    {
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float JumpForce { get; private set; }
        [field: SerializeField] public int MaxJumpCount { get; private set; }
        [field: SerializeField] public float CrouchSpeedMultiplier { get; private set; }
        [field: SerializeField] public float WallSlideMaxSpeed { get; private set; }
        [field: SerializeField] public float WallBounceForce { get; private set; }
    }
}
