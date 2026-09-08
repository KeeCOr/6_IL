using UnityEngine;

namespace IL6
{
    public sealed class WolfAnimationTest : MonoBehaviour
    {
        private Animator _animator;
        private SpriteRenderer _sr;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            if (_animator == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _animator.SetInteger("state", 0); // Idle
                Debug.Log("[WolfAnimationTest] Idle");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _animator.SetInteger("state", 1); // Move
                Debug.Log("[WolfAnimationTest] Move");
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _animator.SetInteger("state", 2); // Attack / Charge
                Debug.Log("[WolfAnimationTest] Attack");
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                if (_sr != null)
                {
                    _sr.flipX = !_sr.flipX;
                    Debug.Log("[WolfAnimationTest] FlipX: " + _sr.flipX);
                }
            }
        }
    }
}