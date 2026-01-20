using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform _PlayerTrget;
    private PlayerController _playerScript;
    private Vector3 _shakeOffset; // 揺れ専用のオフセット

    [Header("Follow Settings")]
    public float smoothSpeed = 10.0f;

    [Header("Speed Settings")]
    public float boostInSpeed = 5.0f;
    public float returnSpeed = 2.0f;

    [Header("Offset Settings")]
    public Vector3 _normalOffset = new Vector3(0, 3, -10);
    public Vector3 _boostOffset = new Vector3(0, 2.5f, -14);
    private Vector3 _currentOffset;

    [Header("FOV Settings")]
    public float _normalFOV = 60f;
    public float _boostFOV = 75f;
    private Camera _cam;

    [Header("Bounds")]
    [SerializeField] float minPosX = -100f, maxPosX = 100f;
    [SerializeField] float minPosY = -100f, maxPosY = 100f;

    void Start()
    {
        _cam = GetComponent<Camera>();
        if (_PlayerTrget != null)
        {
            _playerScript = _PlayerTrget.GetComponent<PlayerController>();
        }
        _currentOffset = _normalOffset;
    }

    private void LateUpdate()
    {
        if (_PlayerTrget == null) return;

        bool isBoosting = (_playerScript != null && _playerScript._isBoosting);
        float currentChangeSpeed = isBoosting ? boostInSpeed : returnSpeed;

        Vector3 targetOffset = isBoosting ? _boostOffset : _normalOffset;
        float targetFOV = isBoosting ? _boostFOV : _normalFOV;

        _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, Time.deltaTime * currentChangeSpeed);

        if (_cam != null)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * currentChangeSpeed);
        }

        // 1. 本来あるべきカメラの目標座標を計算
        Vector3 targetPosWithOffset = _PlayerTrget.position + _currentOffset;

        float finalZ = targetPosWithOffset.z;
        float finalX = Mathf.Lerp(transform.position.x - _shakeOffset.x, targetPosWithOffset.x, Time.deltaTime * smoothSpeed);
        float finalY = Mathf.Lerp(transform.position.y - _shakeOffset.y, targetPosWithOffset.y, Time.deltaTime * smoothSpeed);

        finalX = Mathf.Clamp(finalX, minPosX, maxPosX);
        finalY = Mathf.Clamp(finalY, minPosY, maxPosY);

        // 2. 基本の追従位置に「揺れ」だけを足す
        transform.position = new Vector3(finalX, finalY, finalZ) + _shakeOffset;
    }

    // ダメージ時に呼ばれる。magnitudeを小さく（0.1〜0.2程度）すると動かしやすくなります
    public void TriggerShake(float duration, float magnitude)
    {
        StopAllCoroutines(); // 連続被弾時に揺れが重なりすぎないようにリセット
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // 徐々に揺れを弱くしていく（減衰処理）
            float currentMagnitude = Mathf.Lerp(magnitude, 0, elapsed / duration);

            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;

            _shakeOffset = new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _shakeOffset = Vector3.zero;
    }
}