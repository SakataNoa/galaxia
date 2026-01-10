using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs (Project Window)")]
    public GameObject _laserPrefab;
    public GameObject _barrelLaserPrefab;

    [Header("Setup (Hierarchy Objects)")]
    public Transform[] _muzzles;

    [Header("Firing Settings")]
    public float _laserSpeed = 150f;
    public float _fireRate = 0.15f;
    public float _barrelFireRate = 0.05f;
    public float _spawnOffset = 1.0f;

    [Header("Visual Adjustment")]
    [Tooltip("弾の見た目がズレている場合に回転を補正(例: Xに90など)")]
    public Vector3 _modelRotationOffset = Vector3.zero;

    private float _nextFireTime;
    private float _nextBarrelFireTime;
    private PlayerController _playerController;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        // 直接入力の監視
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) OnAttackDetected();

        // バレルロール中の自動連射
        if (_playerController != null && _playerController.isRolling)
        {
            if (Time.time > _nextBarrelFireTime)
            {
                ExecuteShoot(_barrelLaserPrefab != null ? _barrelLaserPrefab : _laserPrefab, true);
                _nextBarrelFireTime = Time.time + _barrelFireRate;
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
            ExecuteShoot(_laserPrefab, false);
            _nextFireTime = Time.time + _fireRate;
        }
    }

    void ExecuteShoot(GameObject prefab, bool isBarrelShot)
    {
        if (prefab == null) return;
        GameObject reticle = _playerController != null ? _playerController._reticleInstance : null;

        foreach (var muzzle in _muzzles)
        {
            // バレル中は回転している銃口の向き(muzzle.forward)ではなく、
            // 機体本体の向き(transform.forward)を基準にする
            Vector3 spawnPos = muzzle.position + (transform.forward * _spawnOffset);
            SpawnLaser(prefab, spawnPos, muzzle.rotation, reticle, isBarrelShot);
        }
    }

    void SpawnLaser(GameObject prefab, Vector3 position, Quaternion rotation, GameObject reticle, bool isBarrelShot)
    {
        GameObject laser = Instantiate(prefab, position, rotation);

        Vector3 shotDirection;
        if (isBarrelShot)
        {
            // 【重要】機体本体の正面方向に飛ばすことで、横に飛ばなくなります
            shotDirection = transform.forward;
        }
        else
        {
            if (reticle != null)
                shotDirection = (reticle.transform.position - position).normalized;
            else
                shotDirection = transform.forward;
        }

        // 弾の向きを進行方向に向ける
        laser.transform.forward = shotDirection;

        // 見た目の補正（縦向き対策）
        laser.transform.Rotate(_modelRotationOffset);

        Rigidbody rb = laser.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = shotDirection * _laserSpeed;
        }

        Destroy(laser, 2f);
    }
}