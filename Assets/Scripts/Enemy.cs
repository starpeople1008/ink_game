using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("체력 및 크기 설정")]
    [Tooltip("최소 체력")]
    public float minHealth = 10f;
    [Tooltip("최대 체력")]
    public float maxHealth = 100f;
    
    [Tooltip("최소 체력일 때의 크기")]
    public float minScale = 0.5f;
    [Tooltip("최대 체력일 때의 크기")]
    public float maxScale = 2.0f;
    
    // 현재 체력 (인스펙터에서 확인 가능하도록 SerializeField 사용)
    [SerializeField] private float currentHealth;

    [Header("이동 설정")]
    [Tooltip("부유하는 이동 속도")]
    public float moveSpeed = 1.0f;
    [Tooltip("회전하는 속도")]
    public float rotationSpeed = 15.0f;

    [Header("스폰 애니메이션 (뿅 효과)")]
    [Tooltip("등장 애니메이션이 걸리는 시간(초)")]
    public float spawnDuration = 0.5f;
    [Tooltip("등장할 때 크기가 변하는 형태를 결정하는 그래프")]
    public AnimationCurve spawnCurve = new AnimationCurve(
        new Keyframe(0f, 0f), 
        new Keyframe(0.7f, 1.2f), // 목표치보다 살짝 커짐 (통통 튀는 느낌)
        new Keyframe(1f, 1f)
    );

    [Header("피격 애니메이션")]
    [Tooltip("맞았을 때 번쩍이는 연출 시간")]
    public float hitDuration = 0.15f;
    [Tooltip("맞았을 때 얼마나 커질지 비율 (1.3 = 30% 더 커짐)")]
    public float hitScaleMultiplier = 1.3f;
    [Tooltip("맞았을 때 깜빡일 색상")]
    public Color hitColor = new Color(10f, 10f, 10f, 1f); // 매우 밝은 흰색(HDR 느낌)을 기본값으로 사용
    
    private Vector2 moveDirection;
    private Camera mainCamera;
    
    private float targetScale; // 최종 도달해야 할 목표 크기
    private float spawnTimer = 0f;
    private bool isSpawning = true; // 현재 스폰 연출 중인지 여부

    private bool isHit = false;
    private float hitTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // 1. 체력 랜덤 지정
        currentHealth = Random.Range(minHealth, maxHealth);

        // 2. 체력에 비례하여 목표 크기(targetScale) 계산
        float healthRatio = (currentHealth - minHealth) / (maxHealth - minHealth);
        targetScale = Mathf.Lerp(minScale, maxScale, healthRatio);

        // 처음 시작할 때는 크기를 0으로 만듭니다 (스폰 연출을 위해)
        transform.localScale = Vector3.zero;

        // 3. 초기 이동 방향 랜덤 설정 (360도 중 무작위 방향)
        float angle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

        // 4. 회전 방향(시계 or 반시계) 랜덤 설정
        rotationSpeed *= Random.Range(0, 2) == 0 ? 1f : -1f;
    }

    void Update()
    {
        // === 스폰 연출 (뿅 하고 커지기) ===
        if (isSpawning)
        {
            spawnTimer += Time.deltaTime;
            float percent = spawnTimer / spawnDuration; // 0.0 ~ 1.0 사이의 진행도
            
            // 그래프(Curve)에 맞춰 현재 크기를 계산
            float curveMultiplier = spawnCurve.Evaluate(percent);
            float currentScale = targetScale * curveMultiplier;
            transform.localScale = new Vector3(currentScale, currentScale, 1f);

            // 시간이 다 지나면 연출 종료
            if (spawnTimer >= spawnDuration)
            {
                transform.localScale = new Vector3(targetScale, targetScale, 1f);
                isSpawning = false;
            }
        }

        // === 기본 움직임 (천천히 부유 및 회전) ===
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // === 화면 밖으로 나가지 못하게 제한 ===
        CheckBoundsAndBounce();

        // === 피격 연출 (번쩍이고 커지기) ===
        if (isHit)
        {
            hitTimer += Time.deltaTime;
            float percent = hitTimer / hitDuration;

            // 1. 스케일 연출 (부드럽게 커졌다가 다시 원래대로 돌아옴)
            // Mathf.Sin(percent * PI)는 0 -> 1 -> 0 형태의 포물선을 그립니다.
            float scaleBounce = Mathf.Sin(percent * Mathf.PI);
            float currentHitScale = targetScale * (1f + (hitScaleMultiplier - 1f) * scaleBounce);
            
            if (!isSpawning) // 스폰 중일 때 맞으면 크기 연출이 꼬일 수 있으므로 보호
            {
                transform.localScale = new Vector3(currentHitScale, currentHitScale, 1f);
            }

            // 2. 하얗게 번쩍이는 색상 연출
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(hitColor, originalColor, percent);
            }

            // 연출 종료
            if (hitTimer >= hitDuration)
            {
                isHit = false;
                if (!isSpawning) transform.localScale = new Vector3(targetScale, targetScale, 1f);
                if (spriteRenderer != null) spriteRenderer.color = originalColor;
            }
        }
    }

    // 마우스 공격 범위에 들어와서 클릭을 당했을 때 CustomCursor에서 호출하는 함수
    public void OnHit()
    {
        isHit = true;
        hitTimer = 0f; // 타이머 초기화 (연속으로 맞으면 다시 처음부터 번쩍이게)
        
        // 여기에 체력을 깎는 로직을 추가하시면 됩니다.
        // currentHealth -= 10f;
        // if (currentHealth <= 0) { Destroy(gameObject); }
    }

    private void CheckBoundsAndBounce()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        bool isBounced = false;

        float padding = 0.05f * targetScale; // 스케일 연출 중에도 경계는 최종 크기(targetScale) 기준

        if (viewportPos.x <= 0f + padding)
        {
            viewportPos.x = 0f + padding;
            moveDirection.x = Mathf.Abs(moveDirection.x); 
            isBounced = true;
        }
        else if (viewportPos.x >= 1f - padding)
        {
            viewportPos.x = 1f - padding;
            moveDirection.x = -Mathf.Abs(moveDirection.x);
            isBounced = true;
        }

        if (viewportPos.y <= 0f + padding)
        {
            viewportPos.y = 0f + padding;
            moveDirection.y = Mathf.Abs(moveDirection.y);
            isBounced = true;
        }
        else if (viewportPos.y >= 1f - padding)
        {
            viewportPos.y = 1f - padding;
            moveDirection.y = -Mathf.Abs(moveDirection.y);
            isBounced = true;
        }

        if (isBounced)
        {
            Vector3 correctedPos = mainCamera.ViewportToWorldPoint(viewportPos);
            correctedPos.z = 0f;
            transform.position = correctedPos;
        }
    }
}
