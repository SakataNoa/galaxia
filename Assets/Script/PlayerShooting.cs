using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject _laserPrefab;
    public GameObject _homingLaserPrefab;
    public GameObject _lockOnUIPrefab;

    [Header("Settings")]
    public float _laserSpeed = 80f;
    public float _fireRate = 0.15f;
    public int _maxLockOnCount = 10;

    [Header("Setup")]
    public Transform[] _muzzles;
    public Canvas _uiCanvas;

    private float _nextFireTime;
    private PlayerController _playerController;
    private PlayerHealth _playerHealth;
    private Camera _mainCam;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _playerHealth = GetComponent<PlayerHealth>();
        _mainCam = Camera.main;

        if (_uiCanvas == null) _uiCanvas = FindFirstObjectByType<Canvas>();
    }

    void Update()
    {
        // 通常射撃
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_playerController != null && !_playerController.isRolling)
            {
                ManualShoot();
            }
        }
    }

    /// <summary>
    /// バレルロール開始時にPlayerControllerから呼ばれる
    /// </summary>
    public void OnBarrelRollStart()
    {
        if (_playerHealth == null) return;

        // 【条件追加】ストックが最大まで溜まっているかチェック
        if (_playerHealth._currentLaserStock >= _playerHealth._maxLaserStock)
        {
            Debug.Log("フルチャージ！ホーミングレーザー発射！");
            LockAndFireHoming();
        }
        else
        {
            Debug.Log("エネルギー不足: " + _playerHealth._currentLaserStock + "/" + _playerHealth._maxLaserStock);
        }
    }

    void LockAndFireHoming()
    {
        // 画面内の敵をすべて取得
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<Transform> validTargets = new List<Transform>();

        foreach (var enemyObj in enemies)
        {
            Transform enemy = enemyObj.transform;

            if (IsPointVisible(enemy.position))
            {
                validTargets.Add(enemy);
            }

            // 最大ロック数に達したら終了
            if (validTargets.Count >= _maxLockOnCount) break;
        }

        // ターゲットがいれば、溜まったゲージ分すべてを消費して発射
        if (validTargets.Count > 0)
        {
            // ターゲットを順番に割り当てて発射（敵よりゲージが多い場合はループで複数回当てる）
            int shotsToFire = _playerHealth._currentLaserStock;
            for (int i = 0; i < shotsToFire; i++)
            {
                Transform target = validTargets[i % validTargets.Count];

                if (_playerHealth.TryUseLaser())
                {
                    ExecuteHomingShoot(target);
                }
            }
        }
    }

    void ExecuteHomingShoot(Transform target)
    {
        if (_homingLaserPrefab == null || target == null) return;

        Transform muzzle = _muzzles[Random.Range(0, _muzzles.Length)];
        GameObject laser = Instantiate(_homingLaserPrefab, muzzle.position, muzzle.rotation);

        HomingLaser homing = laser.GetComponent<HomingLaser>();
        if (homing != null)
        {
            homing.SetTarget(target);
        }
    }

    void ManualShoot()
    {
        if (Time.time > _nextFireTime)
        {
            ExecuteShoot(_laserPrefab);
            _nextFireTime = Time.time + _fireRate;
        }
    }

    void ExecuteShoot(GameObject prefab)
    {
        if (prefab == null) return;
        GameObject reticle = _playerController._reticleInstance;
        foreach (var muzzle in _muzzles)
        {
            Vector3 spawnPos = muzzle.position;
            GameObject laser = Instantiate(prefab, spawnPos, transform.rotation);
            Vector3 dir = (reticle != null) ? (reticle.transform.position - spawnPos).normalized : transform.forward;

            Rigidbody rb = laser.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = dir * _laserSpeed;

            Destroy(laser, 2f);
        }
    }

    bool IsPointVisible(Vector3 point)
    {
        Vector3 viewportPoint = _mainCam.WorldToViewportPoint(point);
        return viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
    }
}