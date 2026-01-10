using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject _laserPrefab;
    public GameObject _barrelLaserPrefab;

    [Header("Setup")]
    public Transform[] _muzzles;

    [Header("Firing Settings")]
    public float _laserSpeed = 120f;
    public float _fireRate = 0.2f;
    public float _barrelFireRate = 0.05f;

    private float _nextFireTime;
    private float _nextBarrelFireTime;
    private PlayerController _playerController;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
    }

    public void OnFire(InputValue value)
    {
        if (_playerController != null && !_playerController.isRolling)
        {
            if (value.isPressed) ManualShoot();
        }
    }

    void Update()
    {
        if (_playerController != null && _playerController.isRolling)
        {
            if (Time.time > _nextBarrelFireTime)
            {
                ExecuteShoot(_barrelLaserPrefab != null ? _barrelLaserPrefab : _laserPrefab, true);
                _nextBarrelFireTime = Time.time + _barrelFireRate;
            }
        }
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
        if (prefab == null || _muzzles.Length == 0) return;

        // Controllerが生成したインスタンスを取得
        GameObject reticle = _playerController._reticleInstance;

        foreach (var muzzle in _muzzles)
        {
            GameObject laser = Instantiate(prefab, muzzle.position, muzzle.rotation);

            Vector3 shotDirection;
            if (isBarrelShot)
            {
                shotDirection = muzzle.forward;
            }
            else
            {
                if (reticle != null)
                    shotDirection = (reticle.transform.position - muzzle.position).normalized;
                else
                    shotDirection = muzzle.forward;
            }

            laser.transform.forward = shotDirection;

            Rigidbody rb = laser.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shotDirection * _laserSpeed;
            }

            Destroy(laser, 2f);
        }
    }
}