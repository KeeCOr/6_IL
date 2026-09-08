using UnityEngine;

namespace IL6
{
    public sealed class WolfAi : AnimalAi
    {
        private enum WolfState
        {
            Idle = 0,
            Chase = 1,
            PrepareAttack = 2,
            Charge = 3,
            Recover = 4
        }

        public float SightRange = 9f;
        public float AttackRange = 2.2f;
        public float MoveSpeed = 4.0f;
        public int Damage = 6;

        [Header("Charge Attack")]
        public float PrepareDuration = 0.35f;
        public float ChargeDuration = 0.35f;
        public float RecoverDuration = 0.25f;
        public float ChargeSpeed = 7f;
        public float AttackCooldown = 1.2f;


        private WolfState _state = WolfState.Idle;
        private float _stateTimer;
        private float _attackCd;
        private Vector2 _chargeDirection;
        private Transform _currentTarget;
        private bool _hasHit;

        private Transform FindTarget()
        {
            Transform best = null;
            float bestDist = SightRange;

            if (_player != null)
            {
                var pc = _player.GetComponent<PlayerController>();
                if (pc != null && !pc.IsDead)
                {
                    float d = Vector2.Distance(transform.position, _player.position);
                    if (d < bestDist)
                    {
                        best = _player;
                        bestDist = d;
                    }
                }
            }

            var comps = Object.FindObjectsByType<Companion>(FindObjectsSortMode.None);
            foreach (var c in comps)
            {
                if (c == null || c.IsDead) continue;
                if (c.CurrentMode == Companion.Mode.Hiding) continue;

                float d = Vector2.Distance(transform.position, c.transform.position);
                if (d < bestDist)
                {
                    best = c.transform;
                    bestDist = d;
                }
            }

            return best;
        }

        protected override void DoBehavior()
        {
            if (_attackCd > 0f)
                _attackCd -= Time.fixedDeltaTime;

            switch (_state)
            {
                case WolfState.Idle:
                case WolfState.Chase:
                    UpdateChase();
                    break;

                case WolfState.PrepareAttack:
                    UpdatePrepareAttack();
                    break;

                case WolfState.Charge:
                    UpdateCharge();
                    break;

                case WolfState.Recover:
                    UpdateRecover();
                    break;
            }

            UpdateWolfAnimation();
        }

        private void UpdateChase()
        {
            _currentTarget = FindTarget();

            if (_currentTarget == null)
            {
                _state = WolfState.Idle;
                _rb.velocity = Vector2.zero;
                return;
            }

            Vector2 toTarget = (Vector2)_currentTarget.position - _rb.position;
            float dist = toTarget.magnitude;
            Vector2 dir = toTarget.normalized;

            if (dist <= AttackRange && _attackCd <= 0f)
            {
                StartPrepareAttack(dir);
                return;
            }

            _state = WolfState.Chase;
            _rb.velocity = dir * MoveSpeed;
            UpdateFacing(dir);
        }

        private void StartPrepareAttack(Vector2 direction)
        {
            _state = WolfState.PrepareAttack;
            _stateTimer = PrepareDuration;
            _rb.velocity = Vector2.zero;
            UpdateFacing(direction);
        }

        private void UpdatePrepareAttack()
        {
            _stateTimer -= Time.fixedDeltaTime;

            if (_currentTarget != null)
            {
                Vector2 lookDir = ((Vector2)_currentTarget.position - _rb.position).normalized;
                UpdateFacing(lookDir);
            }

            if (_stateTimer <= 0f)
            {
                Vector2 dir;

                if (_currentTarget != null)
                    dir = ((Vector2)_currentTarget.position - _rb.position).normalized;
                else
                    dir = _sr != null && _sr.flipX ? Vector2.left : Vector2.right;

                StartCharge(dir);
            }
        }

        private void StartCharge(Vector2 direction)
        {
            _state = WolfState.Charge;
            _stateTimer = ChargeDuration;
            _chargeDirection = direction.normalized;
            _hasHit = false;

            UpdateFacing(_chargeDirection);
        }

        private void UpdateCharge()
        {
            _stateTimer -= Time.fixedDeltaTime;
            _rb.velocity = _chargeDirection * ChargeSpeed;
            UpdateFacing(_chargeDirection);

            if (_stateTimer <= 0f)
                StartRecover();
        }

        private void StartRecover()
        {
            _state = WolfState.Recover;
            _stateTimer = RecoverDuration;
            _attackCd = AttackCooldown;
            _rb.velocity = Vector2.zero;
        }

        private void UpdateRecover()
        {
            _stateTimer -= Time.fixedDeltaTime;
            _rb.velocity = Vector2.zero;

            if (_stateTimer <= 0f)
                _state = WolfState.Chase;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryHitTarget(collision.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryHitTarget(collision.gameObject);
        }

        private void TryHitTarget(GameObject hitObject)
        {
            if (_state != WolfState.Charge)
                return;

            if (_hasHit)
                return;

            var pc = hitObject.GetComponent<PlayerController>();
            if (pc != null)
            {
                _hasHit = true;
                pc.TakeDamage(Damage);
                StartRecover();
                return;
            }

            var c = hitObject.GetComponent<Companion>();
            if (c != null)
            {
                _hasHit = true;
                c.TakeDamage(Damage);
                StartRecover();
            }
        }

        private void UpdateWolfAnimation()
        {
            SetAnimatorState((int)_state);
        }
    }
}