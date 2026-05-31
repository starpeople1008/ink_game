using UnityEngine;
using UnityEngine.InputSystem;

public class CustomCursor : MonoBehaviour
{
    [Header("게임 상태")]
    [Tooltip("true이면 전투 커서를 사용하고, false이면 시스템 커서를 사용합니다.")]
    public bool isCombatPhase = true; // 다른 스크립트(GameManager 등)에서 이 값을 조작하세요.

    [Header("전투 설정")]
    [Tooltip("공격 판정이 들어가는 원의 반지름 크기입니다. 커서 이미지 크기에 맞게 조절하세요.")]
    public float attackRadius = 1.5f;

    [Header("클릭 애니메이션")]
    [Tooltip("클릭했을 때 번쩍이는 연출 시간")]
    public float clickAnimDuration = 0.15f;
    [Tooltip("클릭 시 커서가 얼마나 커질지 비율 (1.2 = 20% 더 커짐)")]
    public float clickScaleMultiplier = 1.2f;
    [Tooltip("클릭 시 깜빡일 색상")]
    public Color clickFlashColor = new Color(10f, 10f, 10f, 1f); // 밝은 흰색

    private SpriteRenderer cursorSprite;
    
    // 연출을 위한 내부 변수들
    private bool isClicked = false;
    private float clickTimer = 0f;
    private Color originalColor;
    public Vector3 baseScale; // 평상시 커서 크기 (추후 업그레이드로 크기가 변하면 이 변수값을 변경하세요)

    void Start()
    {
        // 내 오브젝트에 붙어있는 이미지를 껐다 켜기 위해 가져옵니다.
        cursorSprite = GetComponent<SpriteRenderer>();
        
        if (cursorSprite != null)
        {
            originalColor = cursorSprite.color;
        }
        
        // 처음 시작할 때의 크기를 기본 크기로 저장합니다.
        baseScale = transform.localScale;
    }

    void Update()
    {
        if (isCombatPhase)
        {
            // === 전투 단계 ===
            // 1. 윈도우 기본 마우스 커서를 숨깁니다.
            if (Cursor.visible) Cursor.visible = false;
            
            // 2. 커스텀 커서 이미지를 보이게 합니다.
            if (cursorSprite != null) cursorSprite.enabled = true;

            // 3. 커서 추적 로직
            if (Mouse.current == null) return;
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            transform.position = mousePosition;
            
            // 4. 제자리 회전 (유저님이 주석 해제하신 부분 적용)
            transform.Rotate(0f, 0f, 50f * Time.deltaTime);

            // 5. 클릭(공격) 판정
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                AttackEnemies(mousePosition);
                
                // 클릭 연출 트리거 온
                isClicked = true;
                clickTimer = 0f;
            }
        }
        else
        {
            // === 메뉴 / 스킬트리 단계 ===
            // 1. 윈도우 기본 마우스 커서를 다시 나타나게 합니다.
            if (!Cursor.visible) Cursor.visible = true;
            
            // 2. 커스텀 커서 이미지를 숨깁니다.
            if (cursorSprite != null) cursorSprite.enabled = false;
        }

        // === 클릭 연출 (번쩍이고 살짝 커지기) ===
        if (isClicked)
        {
            clickTimer += Time.deltaTime;
            float percent = clickTimer / clickAnimDuration;

            // 1. 스케일 연출 (부드럽게 커졌다가 다시 원래 크기인 baseScale로 돌아옴)
            float scaleBounce = Mathf.Sin(percent * Mathf.PI);
            transform.localScale = baseScale * (1f + (clickScaleMultiplier - 1f) * scaleBounce);

            // 2. 하얗게 번쩍이는 색상 연출
            if (cursorSprite != null)
            {
                cursorSprite.color = Color.Lerp(clickFlashColor, originalColor, percent);
            }

            // 연출 종료
            if (clickTimer >= clickAnimDuration)
            {
                isClicked = false;
                transform.localScale = baseScale;
                if (cursorSprite != null) cursorSprite.color = originalColor;
            }
        }
    }

    private void AttackEnemies(Vector2 clickPos)
    {
        // 씬에 존재하는 모든 적을 찾습니다.
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        
        foreach (Enemy enemy in allEnemies)
        {
            // 커서 중심과 적 사이의 거리를 계산
            float distance = Vector2.Distance(clickPos, enemy.transform.position);
            
            // 적의 크기도 고려하여 판정 (크기가 클수록 더 쉽게 맞도록)
            float enemyRadius = enemy.transform.localScale.x * 0.5f;

            // 적중 범위 안에 들어왔다면 피격 함수 호출
            if (distance <= attackRadius + enemyRadius)
            {
                enemy.OnHit();
            }
        }
    }
}
