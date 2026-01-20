using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraController : MonoBehaviour
{
    [SerializeField] Transform _PlayerTrget;
    private PlayerController _playerScript;
    private Vector3 _originalPos;

    [Header("Follow Settings")]
    public float smoothSpeed = 10.0f;

    [Header("Speed Settings")]
    public float boostInSpeed = 5.0f;  // ブースト開始時の変化速度
    public float returnSpeed = 2.0f;   // ノーマルに戻る時の変化速度（ここを調整！）

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

        bool isBoosting = (_playerScript != null && _playerScript.IsBoosting);

        // 1. ブースト中か戻り中かでスピードを使い分ける
        float currentChangeSpeed = isBoosting ? boostInSpeed : returnSpeed;

        // 2. オフセットとFOVを目標値に向けて補完
        Vector3 targetOffset = isBoosting ? _boostOffset : _normalOffset;
        float targetFOV = isBoosting ? _boostFOV : _normalFOV;

        _currentOffset = Vector3.Lerp(_currentOffset, targetOffset, Time.deltaTime * currentChangeSpeed);

        if (_cam != null)
        {
            _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFOV, Time.deltaTime * currentChangeSpeed);
        }

        // 3. 目標座標の計算
        Vector3 targetPosWithOffset = _PlayerTrget.position + _currentOffset;

        // Z座標は同期、X, YはLerp
        float finalZ = targetPosWithOffset.z;
        float finalX = Mathf.Lerp(transform.position.x, targetPosWithOffset.x, Time.deltaTime * smoothSpeed);
        float finalY = Mathf.Lerp(transform.position.y, targetPosWithOffset.y, Time.deltaTime * smoothSpeed);

        // 4. クランプ適用
        finalX = Mathf.Clamp(finalX, minPosX, maxPosX);
        finalY = Mathf.Clamp(finalY, minPosY, maxPosY);

        // 5. 反映
        transform.position = new Vector3(finalX, finalY, finalZ);
    }
    // ダメージ時に PlayerHealth から呼び出す関数
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        _originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // ランダムな方向にずらす
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(_originalPos.x + x, _originalPos.y + y, _originalPos.z);

            elapsed += Time.deltaTime;
            yield return null; // 1フレーム待機
        }

        // 揺れが終わったら元の位置に戻す
        transform.localPosition = _originalPos;
    }
}
