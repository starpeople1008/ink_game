using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [Tooltip("생성할 적의 프리팹을 여기에 연결하세요. (유저님이 만든 스프라이트가 들어간 적)")]
    public GameObject enemyPrefab;
    
    [Tooltip("몇 초 간격으로 적을 생성할지 결정합니다.")]
    public float spawnInterval = 3f;
    
    [Tooltip("화면에 동시에 존재할 수 있는 최대 적의 수입니다.")]
    public int maxEnemies = 10;

    private float spawnTimer = 0f;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 타이머 증가
        spawnTimer += Time.deltaTime;
        
        // 지정된 시간(텀)이 지났는지 확인
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;

            // 현재 화면에 생성된 적의 개수 파악
            Enemy[] currentEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
            
            // 맵에 적이 꽉 차지 않았다면 스폰
            if (currentEnemies.Length < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner에 Enemy Prefab이 비어있습니다! 인스펙터에서 프리팹을 넣어주세요.");
            return;
        }

        // 화면 내부(Viewport 기준 10% ~ 90% 사이의 안전 구역)에서 랜덤 좌표 추출
        float randomX = Random.Range(0.1f, 0.9f);
        float randomY = Random.Range(0.1f, 0.9f);
        
        // Viewport 좌표를 월드 좌표로 변환
        Vector3 spawnViewportPos = new Vector3(randomX, randomY, Mathf.Abs(mainCamera.transform.position.z));
        Vector3 spawnWorldPos = mainCamera.ViewportToWorldPoint(spawnViewportPos);
        spawnWorldPos.z = 0f; // 2D이므로 깊이는 0

        // 지정된 랜덤 위치에 적 프리팹 생성
        Instantiate(enemyPrefab, spawnWorldPos, Quaternion.identity);
    }
}
