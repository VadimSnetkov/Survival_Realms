using System.Collections;
using UnityEngine;

namespace BlockBuildingCraftingSystem
{
    public class MobAI : MonoBehaviour
    {
        public bool isEnemy = false;
        public bool spawnsAtOnlyNight = false;

        [Header("Enemy Stats")]
        public float health = 100f;
        public float speed = 2.5f;
        public int damagePower = 10;

        [Header("Detection & Combat")]
        public float detectionRange = 18f;
        public float attackRange = 2f;
        public float attackCooldown = 1.2f;

        [Header("Audio")]
        public AudioClip attackSound;
        public AudioClip deathSound;
        public AudioClip idleSound;
        public AudioClip getDamageSound;

        public AudioSource audioSource;

        [Header("Lifetime")]
        public float minLifetime = 10f;
        public float maxLifetime = 30f;

        [Header("Daylight")]
        public bool dieAtDayLight = false;

        [Header("Wander")]
        public float wanderRadius = 10f;
        public float wanderWaitTime = 3f;
        public float wanderSpeedMultiplier = 0.55f;

        [Header("Obstacle Avoidance")]
        public float obstacleCheckDistance = 0.9f;
        public float obstacleSphereRadius = 0.32f;
        public float avoidanceAngle1 = 45f;
        public float avoidanceAngle2 = 90f;

        [Header("Auto Jump")]
        public bool enableAutoJump = true;
        public float jumpForce = 8.5f;
        public float jumpCooldown = 0.7f;
        public float jumpForwardCheckDistance = 0.9f;
        public float jumpForwardCheckHeight = 0.45f;
        public float maxJumpableHeight = 1.6f;
        public float headClearanceRadius = 0.35f;
        public LayerMask obstacleLayers = ~0;
        public float stepHeight = 1.2f;
        public float stepForward = 0.35f;
        public float stepCooldown = 0.15f;
        private float lastStepTime = -999f;
        private Vector3 lastPos;
        private float stuckTimer = 0f;
        public float stuckTimeToRepath = 0.4f;
        public float stuckMinMove = 0.03f;
        public float minStepTriggerHeight = 0.75f;
        public float landingCheckUp = 1.1f;
        public float landingCheckForward = 0.6f;

        private Transform player;
        private Rigidbody rb;
        private bool isDead = false;
        private float lastAttackTime = -999f;

        private Vector3 wanderTarget;
        private float wanderTimer = 0f;
        private bool isWandering = true;

        private float idleSoundTimer = 0f;
        private float idleSoundInterval = 8f;

        private float lastJumpTime = -999f;
        private bool isGrounded;
        private Collider selfCol;

        void Start()
        {
            lastPos = rb != null ? rb.position : transform.position;
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.useGravity = true;
            rb.isKinematic = false;
            selfCol = GetComponent<Collider>();
            if (HeroPlayerScript.Instance != null)
                player = HeroPlayerScript.Instance.transform;

            SetNewWanderTarget();

            if (maxLifetime > 0f)
                Destroy(gameObject, Random.Range(Mathf.Max(0.1f, minLifetime), Mathf.Max(minLifetime, maxLifetime)));
        }

        void Update()
        {
            if (isDead) return;

            if (HeroPlayerScript.Instance != null && player == null)
                player = HeroPlayerScript.Instance.transform;

            if (player == null) return;

            if (dieAtDayLight && DayNightManager.Instance != null && !DayNightManager.Instance.isDark)
            {
                Die();
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (isEnemy && distanceToPlayer <= detectionRange)
            {
                isWandering = false;

                if (distanceToPlayer <= attackRange)
                    AttackPlayer();
            }
            else
            {
                isWandering = true;
                WanderUpdateTimers();
            }
        }

        void UpdateStuck()
        {
            Vector3 pos = rb != null ? rb.position : transform.position;
            float moved = (pos - lastPos).magnitude;
            lastPos = pos;

            if (moved < stuckMinMove) stuckTimer += Time.fixedDeltaTime;
            else stuckTimer = 0f;
        }
        void FixedUpdate()
        {
            if (isDead || player == null) return;

            UpdateGrounded();
            UpdateStuck();
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (isEnemy && distanceToPlayer <= detectionRange)
            {
                FollowPlayer(distanceToPlayer);
            }
            else
            {
                WanderMove();
            }
        }

        void FollowPlayer(float distanceToPlayer)
        {
            Vector3 dir = (player.position - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
            {
                StopHorizontal();
                return;
            }

            dir.Normalize();

            if (distanceToPlayer > attackRange)
            {
                if (IsObstacleAhead(dir))
                {
                    if (TryStepUp(dir))
                    {
                        RotateTowards(dir, 6f);
                        return;
                    }

                    Vector3 avoid = GetAvoidanceDirection(dir);

                    if (avoid != Vector3.zero && !IsObstacleAhead(avoid))
                    {
                        MoveRB(avoid, speed);
                    }
                    else
                    {
                        if (stuckTimer > stuckTimeToRepath)
                        {
                            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                            if (Random.value < 0.5f) side = -side;

                            if (!IsObstacleAhead(side))
                                MoveRB(side, speed);
                            else
                                MoveRB(-side, speed);

                            stuckTimer = 0f;
                        }
                        else
                        {
                            StopHorizontal();
                        }
                    }
                }
                else
                {
                    MoveRB(dir, speed);
                }
            }
            else
            {
                StopHorizontal();
            }

            RotateTowards(dir, 6f);
        }

        void WanderMove()
        {
            if (idleSound != null && audioSource != null)
            {
                idleSoundTimer += Time.deltaTime;
                if (idleSoundTimer >= idleSoundInterval)
                {
                    idleSoundTimer = 0f;
                    if (!audioSource.isPlaying) audioSource.PlayOneShot(idleSound);
                }
            }

            float dist = Vector3.Distance(transform.position, wanderTarget);
            wanderTimer += Time.fixedDeltaTime;

            if (dist < 1.0f || wanderTimer >= wanderWaitTime)
            {
                wanderTimer = 0f;
                SetNewWanderTarget();

                if (Random.value < 0.25f)
                {
                    StopHorizontal();
                    return;
                }
            }

            Vector3 dir = (wanderTarget - transform.position);
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.0001f)
            {
                StopHorizontal();
                return;
            }

            dir.Normalize();

            float ws = speed * wanderSpeedMultiplier;

            if (IsObstacleAhead(dir))
            {
                if (TryStepUp(dir))
                {
                    RotateTowards(dir, 3.5f);
                    return;
                }

                Vector3 avoid = GetAvoidanceDirection(dir);

                if (avoid != Vector3.zero && !IsObstacleAhead(avoid))
                {
                    MoveRB(avoid, ws);
                }
                else
                {
                    if (stuckTimer > stuckTimeToRepath)
                    {
                        Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;
                        if (Random.value < 0.5f) side = -side;

                        if (!IsObstacleAhead(side))
                            MoveRB(side, ws);
                        else
                            MoveRB(-side, ws);

                        stuckTimer = 0f;
                    }
                    else
                    {
                        StopHorizontal();
                    }
                }
            }
            else
            {
                MoveRB(dir, ws);
            }

            RotateTowards(dir, 3.5f);
        }


        void WanderUpdateTimers()
        {

        }

        void SetNewWanderTarget()
        {
            float randomAngle = Random.Range(0f, 360f);
            float randomDistance = Random.Range(3f, Mathf.Max(3.1f, wanderRadius));

            Vector3 randomDirection = new Vector3(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                0,
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );

            wanderTarget = transform.position + randomDirection * randomDistance;
            wanderTarget.y = transform.position.y;
        }

        void AttackPlayer()
        {
            RotateTowards((player.position - transform.position), 10f);

            if (Time.time < lastAttackTime + attackCooldown) return;
            lastAttackTime = Time.time;

            if (attackSound != null && audioSource != null) audioSource.PlayOneShot(attackSound);

            if (HeroPlayerScript.Instance != null)
                HeroPlayerScript.Instance.GetDamage(damagePower);
        }

        public void TakeDamage(float damage)
        {
            if (isDead) return;

            health -= damage;

            if (getDamageSound != null && audioSource != null)
                audioSource.PlayOneShot(getDamageSound);

            if (health <= 0f) Die();
        }

        void Die()
        {
            if (isDead) return;
            isDead = true;

            StopAllCoroutines();

            if (deathSound != null && audioSource != null)
                audioSource.PlayOneShot(deathSound);

            StartCoroutine(DeathAnimation());
        }

        IEnumerator DeathAnimation()
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            float elapsed = 0f;
            float dur = 0.5f;
            Quaternion start = transform.rotation;
            Quaternion target = transform.rotation * Quaternion.Euler(0, 0, 90);

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(start, target, elapsed / dur);
                yield return null;
            }

            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        void MoveRB(Vector3 direction, float speedValue)
        {
            if (rb == null) return;

            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f)
            {
                StopHorizontal();
                return;
            }

            direction.Normalize();

            Vector3 v = rb.velocity;
            Vector3 horiz = direction * speedValue;

            rb.velocity = new Vector3(horiz.x, v.y, horiz.z);
        }

        void StopHorizontal()
        {
            if (rb == null) return;
            Vector3 v = rb.velocity;
            rb.velocity = new Vector3(0f, v.y, 0f);
        }

        void RotateTowards(Vector3 dir, float rotSpeed)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) return;

            Quaternion target = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime * rotSpeed);
        }

        void UpdateGrounded()
        {
            float extra = 0.08f;
            Vector3 origin = transform.position + Vector3.up * 0.2f;
            float dist = 0.45f;

            if (selfCol != null)
            {
                origin = new Vector3(selfCol.bounds.center.x, selfCol.bounds.min.y + 0.2f, selfCol.bounds.center.z);
                dist = 0.25f + extra;
            }

            isGrounded = Physics.Raycast(origin, Vector3.down, dist, obstacleLayers, QueryTriggerInteraction.Ignore);
        }

        void TryAutoJump(Vector3 moveDir)
        {
            if (!isGrounded) return;
            if (Time.time < lastJumpTime + jumpCooldown) return;
            if (rb == null) return;

            moveDir.y = 0f;
            if (moveDir.sqrMagnitude < 0.0001f) return;
            moveDir.Normalize();

            float feetY = transform.position.y;
            Vector3 center = transform.position;

            if (selfCol != null)
            {
                feetY = selfCol.bounds.min.y;
                center = selfCol.bounds.center;
            }

            Vector3 lowOrigin = new Vector3(center.x, feetY + jumpForwardCheckHeight, center.z);

            if (!Physics.Raycast(lowOrigin, moveDir, out RaycastHit hitLow, jumpForwardCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
                return;

            if (hitLow.collider == null) return;
            if (hitLow.collider == selfCol) return;
            if (player != null && hitLow.collider.transform == player) return;

            float obstacleTopY = hitLow.collider.bounds.max.y;
            float obstacleHeight = obstacleTopY - feetY;

            if (obstacleHeight > maxJumpableHeight) return;

            Vector3 clearanceCenter = new Vector3(center.x, feetY + 1.4f, center.z) + moveDir * 0.6f;
            if (Physics.CheckSphere(clearanceCenter, headClearanceRadius, obstacleLayers, QueryTriggerInteraction.Ignore))
                return;

            Vector3 v = rb.velocity;
            if (v.y < 0f) v.y = 0f;
            rb.velocity = v;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            rb.AddForce(moveDir * 1.2f, ForceMode.VelocityChange);

            lastJumpTime = Time.time;
        }


        bool IsObstacleAhead(Vector3 direction)
        {
            if (direction.sqrMagnitude < 0.0001f) return false;

            Vector3 dir = direction; dir.y = 0f; dir.Normalize();

            Vector3 origin = transform.position + Vector3.up * 0.6f;
            if (selfCol != null)
                origin = new Vector3(selfCol.bounds.center.x, selfCol.bounds.min.y + 0.6f, selfCol.bounds.center.z);

            if (Physics.SphereCast(origin, obstacleSphereRadius, dir, out RaycastHit hit, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider == null) return false;
                if (hit.collider == selfCol) return false;
                if (player != null && hit.collider.transform == player) return false;
                return true;
            }
            return false;
        }


        Vector3 GetAvoidanceDirection(Vector3 originalDirection)
        {
            Vector3 od = originalDirection;
            od.y = 0f;
            if (od.sqrMagnitude < 0.0001f) return Vector3.zero;
            od.Normalize();

            Vector3 right1 = Quaternion.Euler(0, avoidanceAngle1, 0) * od;
            Vector3 left1 = Quaternion.Euler(0, -avoidanceAngle1, 0) * od;

            Vector3 origin = transform.position + Vector3.up * 1.0f;
            if (selfCol != null)
                origin = new Vector3(selfCol.bounds.center.x, selfCol.bounds.min.y + 1.0f, selfCol.bounds.center.z);

            if (!Physics.SphereCast(origin, obstacleSphereRadius, right1, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
                return right1.normalized;

            if (!Physics.SphereCast(origin, obstacleSphereRadius, left1, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
                return left1.normalized;

            Vector3 right2 = Quaternion.Euler(0, avoidanceAngle2, 0) * od;
            Vector3 left2 = Quaternion.Euler(0, -avoidanceAngle2, 0) * od;

            if (!Physics.SphereCast(origin, obstacleSphereRadius, right2, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
                return right2.normalized;

            if (!Physics.SphereCast(origin, obstacleSphereRadius, left2, out _, obstacleCheckDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
                return left2.normalized;

            return Vector3.zero;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        bool TryStepUp(Vector3 moveDir)
        {
            if (Time.time < lastStepTime + stepCooldown) return false;
            if (!isGrounded) return false;
            if (rb == null) return false;

            moveDir.y = 0f;
            if (moveDir.sqrMagnitude < 0.0001f) return false;
            moveDir.Normalize();

            Collider col = selfCol != null ? selfCol : GetComponent<Collider>();
            if (!col) return false;

            float feetY = col.bounds.min.y;
            Vector3 center = col.bounds.center;

            Vector3 lowOrigin = new Vector3(center.x, feetY + 0.25f, center.z);
            if (!Physics.Raycast(lowOrigin, moveDir, out RaycastHit hitLow, 0.85f, obstacleLayers, QueryTriggerInteraction.Ignore))
                return false;

            if (hitLow.collider == null || hitLow.collider == col) return false;
            if (player != null && hitLow.collider.transform == player) return false;

            float obstacleTopY = hitLow.collider.bounds.max.y;
            float height = obstacleTopY - feetY;

            if (height < minStepTriggerHeight) return false;
            if (height > stepHeight) return false;

            Vector3 landingRayStart = new Vector3(center.x, feetY + landingCheckUp, center.z) + moveDir * landingCheckForward;
            if (!Physics.Raycast(landingRayStart, Vector3.down, out RaycastHit landingHit, landingCheckUp + 0.6f, obstacleLayers, QueryTriggerInteraction.Ignore))
                return false;

            float landingY = landingHit.point.y;

            if (landingY < obstacleTopY - 0.15f) return false;

            float radius = 0.45f;
            float capsuleHeight = Mathf.Max(1.2f, col.bounds.size.y);
            Vector3 targetPos = new Vector3(rb.position.x, landingY + 0.02f, rb.position.z) + moveDir * 0.35f;

            Vector3 bottom = targetPos + Vector3.up * radius;
            Vector3 top = targetPos + Vector3.up * (capsuleHeight - radius);

            if (Physics.CheckCapsule(bottom, top, radius, obstacleLayers, QueryTriggerInteraction.Ignore))
                return false;

            Vector3 v = rb.velocity;
            rb.velocity = new Vector3(v.x, 0f, v.z);

            float upVel = Mathf.Sqrt(2f * 9.81f * Mathf.Clamp(height + 0.05f, 0.6f, stepHeight));
            rb.AddForce(Vector3.up * upVel, ForceMode.VelocityChange);
            rb.AddForce(moveDir * 1.3f, ForceMode.VelocityChange);

            lastStepTime = Time.time;
            return true;
        }


    }
}
