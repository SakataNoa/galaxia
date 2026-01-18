using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject _laserPrefab;
    public GameObject _homingLaserPrefab;
    public GameObject _lockOnUIPrefab; // ロックオンUIのプレハブ

    [Header("Settings")]
    public float _laserSpeed = 80f;
    public float _fireRate = 0.15f;
    public float _barrelFireRate = 0.2f;
    public float _lockOnRadius = 3f;
    public int _maxLockOnCount = 5;

    [Header("Setup")]
    public Transform[] _muzzles;
    public Canvas _uiCanvas; // UIを表示するキャンバス

    // ロック中の敵と、そのUIのペアを管理
    private Dictionary<Transform, GameObject> _lockedTargetsUI = new Dictionary<Transform, GameObject>();

    private float _nextFireTime;
    private float _nextBarrelFireTime;
    private PlayerController _playerController;
    private Camera _mainCam;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _mainCam = Camera.main;

        if (_uiCanvas == null) _uiCanvas = FindFirstObjectByType<Canvas>();
    }

    void Update()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) OnAttackDetected();

        if (_playerController != null && _playerController.isRolling)
        {
            SearchNewTargets();
            ValidateTargets();

            if (Time.time > _nextBarrelFireTime && _lockedTargetsUI.Count > 0)
            {
                FireHomingAndRelease();
                _nextBarrelFireTime = Time.time + _barrelFireRate;
            }
        }
        else
        {
            ClearAllLockOn();
        }
    }

    void SearchNewTargets()
    {
        if (_playerController._reticleInstance == null) return;

        Vector3 startPoint = transform.position;
        Vector3 endPoint = _playerController._reticleInstance.transform.position;

        Collider[] colliders = Physics.OverlapCapsule(startPoint, endPoint, _lockOnRadius);

        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Transform enemy = col.transform;

                // まだロックしていなければUIを生成して登録
                if (!_lockedTargetsUI.ContainsKey(enemy) && _lockedTargetsUI.Count < _maxLockOnCount)
                {
                    CreateLockOnUI(enemy);
                }
            }
        }
    }

    void CreateLockOnUI(Transform target)
    {
        if (_lockOnUIPrefab == null || _uiCanvas == null) return;

        GameObject uiInstance = Instantiate(_lockOnUIPrefab, _uiCanvas.transform);
        LockOnUI lockUI = uiInstance.GetComponent<LockOnUI>();
        if (lockUI != null) lockUI.Setup(target);

        _lockedTargetsUI.Add(target, uiInstance);
        Debug.Log("ロックオンUI表示: " + target.name);
    }

    void ValidateTargets()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_mainCam);
        List<Transform> keysToRemove = new List<Transform>();

        foreach (var pair in _lockedTargetsUI)
        {
            Transform target = pair.Key;
            if (target == null) { keysToRemove.Add(target); continue; }

            Collider targetCol = target.GetComponent<Collider>();
            bool isVisible = (targetCol != null) ? GeometryUtility.TestPlanesAABB(planes, targetCol.bounds) : IsPointVisible(target.position);

            if (!isVisible) keysToRemove.Add(target);
        }

        foreach (var key in keysToRemove)
        {
            RemoveLockOn(key);
        }
    }

    void RemoveLockOn(Transform target)
    {
        if (_lockedTargetsUI.ContainsKey(target))
        {
            if (_lockedTargetsUI[target] != null) Destroy(_lockedTargetsUI[target]);
            _lockedTargetsUI.Remove(target);
        }
    }

    void ClearAllLockOn()
    {
        foreach (var ui in _lockedTargetsUI.Values) if (ui != null) Destroy(ui);
        _lockedTargetsUI.Clear();
    }

    bool IsPointVisible(Vector3 point)
    {
        Vector3 viewportPoint = _mainCam.WorldToViewportPoint(point);
        return viewportPoint.z > 0 && viewportPoint.x > 0 && viewportPoint.x < 1 && viewportPoint.y > 0 && viewportPoint.y < 1;
    }

    void FireHomingAndRelease()
    {
        List<Transform> keys = new List<Transform>(_lockedTargetsUI.Keys);

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            Transform target = keys[i];
            if (target == null) continue;

            Transform muzzle = _muzzles[i % _muzzles.Length];

            if (_homingLaserPrefab != null)
            {
                GameObject laser = Instantiate(_homingLaserPrefab, muzzle.position, muzzle.rotation);
                HomingLaser homing = laser.GetComponent<HomingLaser>();
                if (homing != null) homing.Initialize(target, _laserSpeed);

                // 発射したらUIを消してリストから除外
                RemoveLockOn(target);
            }
        }
    }

    private void OnAttackDetected()
    {
        if (_playerController != null && !_playerController.isRolling) ManualShoot();
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
            Vector3 spawnPos = muzzle.position + (transform.forward * 1.0f);
            GameObject laser = Instantiate(prefab, spawnPos, transform.rotation);
            Vector3 dir = (reticle != null) ? (reticle.transform.position - spawnPos).normalized : transform.forward;
            laser.transform.forward = dir;
            Rigidbody rb = laser.GetComponent<Rigidbody>();
            if (rb != null) rb.linearVelocity = dir * _laserSpeed;
            Destroy(laser, 2f);
        }
    }

    private void OnDrawGizmos()
    {
        if (_playerController != null && _playerController._reticleInstance != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 start = transform.position;
            Vector3 end = _playerController._reticleInstance.transform.position;
            Gizmos.DrawLine(start, end);
            Gizmos.DrawWireSphere(start, _lockOnRadius);
            Gizmos.DrawWireSphere(end, _lockOnRadius);
        }
    }
}