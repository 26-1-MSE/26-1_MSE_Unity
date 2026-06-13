using UnityEngine;

//Makes a pet automatically wander left and right within a defined range.
public class PetWander : MonoBehaviour
{
    [Header("Wander Settings")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private bool useAbsoluteRange = false; // If true, uses minX/maxX; otherwise uses startX +/- wanderRangeX
    [SerializeField] private float wanderRangeX = 3f; // Wander range relative to start position
    [SerializeField] private float minX = -5f; // Absolute left boundary (used when useAbsoluteRange is true)
    [SerializeField] private float maxX = 5f;  // Absolute right boundary (used when useAbsoluteRange is true)

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float startX; // Initial X position used as wander center
    private float targetX;  // Current movement destination
    private bool isMoving = false;
    private float waitTimer = 0f;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        startX = transform.position.x;
        SetNewTarget();
    }

    private void Update()
    {
        if (isMoving)
        {
            float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.deltaTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // Flip sprite to face the movement direction
            spriteRenderer.flipX = targetX > transform.position.x;

            if (animator != null)
                animator.SetBool("isWalking", true);

            // Reached target: stop and wait before picking a new target
            if (Mathf.Abs(transform.position.x - targetX) < 0.05f)
            {
                isMoving = false;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
                if (animator != null)
                    animator.SetBool("isWalking", false);
            }
        }
        else
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
                SetNewTarget();
        }
    }

    // Picks a new random target X position within the configured range
    private void SetNewTarget()
    {
        float randomX;

        if (useAbsoluteRange)
            randomX = Random.Range(minX, maxX);
        else
            randomX = startX + Random.Range(-wanderRangeX, wanderRangeX);

        targetX = randomX;
        isMoving = true;
    }
}