using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetController : MonoBehaviour
{
    [Header("生成区域设置")]
    public Vector3 spawnAreaCenter = new Vector3(0, 2, 10);
    public Vector3 spawnAreaSize = new Vector3(20, 8, 2);

    [Header("生成设置")]
    public GameObject targetPrefab;
    public int maxConcurrentTargets = 3;
    public float minSpawnDelay = 1f;
    public float maxSpawnDelay = 3f;
    public float targetLifetime = 5f;

    [Header("目标外观设置")]
    public Material[] targetMaterials;
    public Vector2 targetScaleRange = new Vector2(0.8f, 1.5f);

    private List<GameObject> activeTargets = new List<GameObject>();
    private int score = 0;

    void Start()
    {
        if (targetPrefab == null)
        {
            Debug.LogError("请设置目标预制体！");
            return;
        }

        StartCoroutine(SpawnTargetsRoutine());
    }

    IEnumerator SpawnTargetsRoutine()
    {
        while (true)
        {
            if (activeTargets.Count < maxConcurrentTargets)
            {
                SpawnTarget();
            }

            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnTarget()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject target = Instantiate(targetPrefab, spawnPosition, GetRandomRotation());

        // 设置目标外观
        SetupTargetAppearance(target);

        // 添加目标脚本
        Target targetScript = target.AddComponent<Target>();
        targetScript.Initialize(this, targetLifetime);

        activeTargets.Add(target);

        Debug.Log($"生成目标，当前位置目标数: {activeTargets.Count}");
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomPoint = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        );

        return spawnAreaCenter + randomPoint;
    }

    Quaternion GetRandomRotation()
    {
        return Quaternion.Euler(
            Random.Range(0, 360),
            Random.Range(0, 360),
            Random.Range(0, 360)
        );
    }

    void SetupTargetAppearance(GameObject target)
    {
        // 随机缩放
        float scale = Random.Range(targetScaleRange.x, targetScaleRange.y);
        target.transform.localScale = Vector3.one * scale;

        // 随机颜色材质
        if (targetMaterials != null && targetMaterials.Length > 0)
        {
            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material randomMaterial = targetMaterials[Random.Range(0, targetMaterials.Length)];
                renderer.material = randomMaterial;
            }
        }
    }

    // 目标被命中时调用
    public void OnTargetHit(GameObject target, bool byPlayer = true)
    {
        if (activeTargets.Contains(target))
        {
            activeTargets.Remove(target);

            if (byPlayer)
            {
                score++;
                Debug.Log($"命中目标！当前分数: {score}");

                // 播放命中效果
                PlayHitEffect(target.transform.position);
            }

            Destroy(target);
        }
    }

    // 目标超时消失时调用
    public void OnTargetTimeout(GameObject target)
    {
        if (activeTargets.Contains(target))
        {
            activeTargets.Remove(target);
            Debug.Log("目标超时消失");
            Destroy(target);
        }
    }

    void PlayHitEffect(Vector3 position)
    {
        // 这里可以添加粒子效果、声音等
        Debug.Log("播放命中效果在位置: " + position);
    }

    // 在Scene视图中显示生成区域
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);

        // 显示当前生成区域中心点
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(spawnAreaCenter, 0.2f);
    }

    public int GetCurrentScore()
    {
        return score;
    }

    public int GetActiveTargetCount()
    {
        return activeTargets.Count;
    }

    // 清空所有目标（用于重新开始游戏等）
    public void ClearAllTargets()
    {
        foreach (GameObject target in activeTargets)
        {
            Destroy(target);
        }
        activeTargets.Clear();
    }
}