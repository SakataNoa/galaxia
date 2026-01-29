using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] _enemyPrefabs; // 生成する敵のバリエーション

    [Header("Spawn Settings")]
    public float _spawnInterval = 2.0f; // 生成間隔
    public bool _isSpawning = true;

    [Header("Spawn Area (Random Mode)")]
    public bool _useRandomSpawn = true;
    public Vector2 _spawnRangeX = new Vector2(-20, 20);
    public Vector2 _spawnRangeY = new Vector2(-10, 15);
    public float _spawnZ = 100f; // プレイヤーの遥か前方

    [Header("Fixed Spawn Points")]
    public Transform[] _spawnPoints; // 特定の場所から出したい場合

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (_isSpawning)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (_enemyPrefabs.Length == 0) return;

        // 1. 生成する敵をランダムに選ぶ
        GameObject prefab = _enemyPrefabs[Random.Range(0, _enemyPrefabs.Length)];

        Vector3 spawnPosition;

        if (_useRandomSpawn)
        {
            // 2. 画面外のランダムな位置を計算
            float randomX = Random.Range(_spawnRangeX.x, _spawnRangeX.y);
            float randomY = Random.Range(_spawnRangeY.x, _spawnRangeY.y);

            // プレイヤーの現在のZ位置を基準に前方に生成
            float playerZ = 0;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerZ = player.transform.position.z;

            spawnPosition = new Vector3(randomX, randomY, playerZ + _spawnZ);
        }
        else if (_spawnPoints.Length > 0)
        {
            // 3. 決められたポイントから選ぶ
            spawnPosition = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;
        }
        else
        {
            return;
        }

        // 生成
        Instantiate(prefab, spawnPosition, Quaternion.Euler(0, 180, 0)); // プレイヤーの方を向かせる
    }

    // 範囲を可視化（デバッグ用）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3(
            (_spawnRangeX.x + _spawnRangeX.y) / 2,
            (_spawnRangeY.x + _spawnRangeY.y) / 2,
            _spawnZ
        );
        Vector3 size = new Vector3(
            _spawnRangeX.y - _spawnRangeX.x,
            _spawnRangeY.y - _spawnRangeY.y,
            1
        );
        Gizmos.DrawWireCube(center, size);
    }
}