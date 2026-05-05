using UnityEngine;

public class PetWanderInArea : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 3f;
    [SerializeField] private BoxCollider2D moveArea;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private float targetX;
    private float waitTimer;
    private bool isMoving;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        SetNewTarget();
    }

    private void Update()
    {
        if (moveArea == null) return;

        if (isMoving)
        {
            float newX = Mathf.MoveTowards(transform.position.x, targetX, moveSpeed * Time.deltaTime);
            Bounds bounds = moveArea.bounds;
            newX = Mathf.Clamp(newX, bounds.min.x, bounds.max.x);

            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            if (spriteRenderer != null)
                spriteRenderer.flipX = targetX > transform.position.x;

            if (animator != null)
                animator.SetBool("isWalking", true);

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

    private void SetNewTarget()
    {
        Bounds bounds = moveArea.bounds;
        targetX = Random.Range(bounds.min.x, bounds.max.x);
        isMoving = true;
    }

    public void SetMoveArea(BoxCollider2D area)
    {
        moveArea = area;
    }
}