using UnityEngine;

namespace MadKnight
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Isla : MonoBehaviour
    {
        [SerializeField] private Transform _player;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            _sr.flipX = transform.position.x - _player.position.x > 0;
        }
    }
}