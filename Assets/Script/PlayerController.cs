using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float _moveSpeed = 15f;
    public float _tilt = 30f;
    public float _tiltSpeed = 0.1f;
    public float _leanSpeed = 0.05f;

    private float _horiInput;
    private float _verInput;
    private float _leanInput;

    [Header("Barrel Roll Settings")]
    [SerializeField] float _timeBetTaps = 0.25f;
    [SerializeField] float _rollDuration = 0.5f;
    public bool isRolling = false;
    public GameObject barrelDeflect;

    private float _lastLeftTapTime;
    private float _lastRightTapTime;

    [Header("Boost Settings")]
    public float _boostSpeed = 30f;
    public float _speedSmoothTime = 8f;
    public bool _isBoosting = false; // publicÇ…ïœçXÇµÇƒCameraControllerÇ©ÇÁå©Ç¶ÇÈÇÊÇ§Ç…ÇµÇ‹ÇµÇΩ
    private float _currentSpeed;

    [Header("Reticle (Aiming)")]
    public GameObject _reticlePrefab;
    [HideInInspector] public GameObject _reticleInstance;
    public float _reticleDistance = 30f;
    public float _reticleSmoothTime = 20f;

    private Rigidbody _rb;
    private PlayerShooting _playerShooting;

    private void Start()
    {
        _rb = GetComponent(typeof(Rigidbody)) as Rigidbody;
        _rb.useGravity = false;
        _rb.isKinematic = true;

        _playerShooting = GetComponent<PlayerShooting>();

        isRolling = false;
        if (barrelDeflect != null) barrelDeflect.SetActive(false);
        _currentSpeed = _moveSpeed;

        if (_reticlePrefab != null)
        {
            _reticleInstance = Instantiate(_reticlePrefab);
        }
    }

    public void OnMove(InputValue value) { Vector2 input = value.Get<Vector2>(); _horiInput = input.x; _verInput = input.y; }

    public void OnLean(InputValue value)
    {
        float input = value.Get<float>();
        _leanInput = input;
        if (isRolling || Mathf.Abs(input) < 0.1f) return;

        if (input < -0.1f)
        {
            if (Time.time - _lastLeftTapTime < _timeBetTaps) StartCoroutine(BarrelRoll(1));
            _lastLeftTapTime = Time.time;
        }
        else if (input > 0.1f)
        {
            if (Time.time - _lastRightTapTime < _timeBetTaps) StartCoroutine(BarrelRoll(-1));
            _lastRightTapTime = Time.time;
        }
    }

    public void OnBoost(InputValue value) { _isBoosting = value.isPressed; }

    void Update()
    {
        float targetSpeed = _isBoosting ? _boostSpeed : _moveSpeed;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * _speedSmoothTime);

        HandleReticle();

        if (!isRolling) HandleRotation();
        ClampToScreen();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        transform.Translate(Vector3.forward * _currentSpeed * Time.fixedDeltaTime, Space.World);
        Vector3 movement = new Vector3(_horiInput, _verInput, 0);
        transform.localPosition += movement * _moveSpeed * Time.fixedDeltaTime;
    }

    void HandleReticle()
    {
        if (_reticleInstance == null) return;

        Vector3 targetReticlePos = transform.position + (transform.forward * _reticleDistance);
        _reticleInstance.transform.position = Vector3.Lerp(_reticleInstance.transform.position, targetReticlePos, Time.deltaTime * _reticleSmoothTime);
        _reticleInstance.transform.LookAt(Camera.main.transform);
        _reticleInstance.transform.Rotate(0, 180, 0);
    }

    void HandleRotation()
    {
        Vector3 currentRot = transform.localEulerAngles;
        float targetX = Mathf.LerpAngle(currentRot.x, -_verInput * _tilt, _tiltSpeed);
        float targetY = Mathf.LerpAngle(currentRot.y, _horiInput * _tilt, _tiltSpeed);
        float targetZ = (_leanInput != 0) ? -_leanInput * 95f : -_horiInput * _tilt;
        float lerpedZ = Mathf.LerpAngle(currentRot.z, targetZ, _leanSpeed);
        transform.localEulerAngles = new Vector3(targetX, targetY, lerpedZ);
    }

    IEnumerator BarrelRoll(int direction)
    {
        isRolling = true;
        if (_playerShooting != null) _playerShooting.OnBarrelRollStart();

        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null) health._isRolling = true;

        if (barrelDeflect != null) barrelDeflect.SetActive(true);
        float elapsed = 0f;
        float startZ = transform.localEulerAngles.z;
        while (elapsed < _rollDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _rollDuration;
            float curve = Mathf.SmoothStep(0, 1, percent);
            float zRotation = startZ + (direction * 360f * curve);
            float currentX = Mathf.LerpAngle(transform.localEulerAngles.x, -_verInput * _tilt, _tiltSpeed);
            float currentY = Mathf.LerpAngle(transform.localEulerAngles.y, _horiInput * _tilt, _tiltSpeed);
            transform.localRotation = Quaternion.Euler(currentX, currentY, zRotation);
            yield return null;
        }
        if (barrelDeflect != null) barrelDeflect.SetActive(false);
        if (health != null) health._isRolling = false;
        isRolling = false;
    }

    void ClampToScreen()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, 0.05f, 0.95f);
        pos.y = Mathf.Clamp(pos.y, 0.05f, 0.95f);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }
}