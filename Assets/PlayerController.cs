using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float _moveSpeed;
    public float _tilt;
    public float _tiltSpeed = 0.1f;
    public float _leanSpeed = 0.01f; // リーン（Q/E）の戻る速さ

    private float _horiInput;
    private float _verInput;
    private float _leanInput; // Q/Eの入力を保持（-1, 0, 1）

    // 移動入力（WASD / スティック）
    public void OnMove(InputValue value)
    {
        Vector2 input = value.Get<Vector2>();
        _horiInput = input.x;
        _verInput = input.y;
    }

    // リーン入力（Q/E）
    // Input Actionで "Lean" という名前の1D Axis（Q:-1, E:1）を設定してください
    public void OnLean(InputValue value)
    {
        _leanInput = value.Get<float>();
    }

    void Update()
    {
        HandleRotation();
        ClampToScreen();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Movement()
    {
        // 前進
        transform.Translate(Vector3.forward * _moveSpeed * Time.fixedDeltaTime);

        // 上下左右移動
        Vector3 movement = new Vector3(_horiInput, _verInput, 0);
        transform.localPosition += movement * _moveSpeed * Time.fixedDeltaTime;
    }

    void HandleRotation()
    {
        Vector3 currentRot = transform.localEulerAngles;

        // 1. 上下移動によるX軸の傾き（これは常に適用）
        float targetX = Mathf.LerpAngle(currentRot.x, -_verInput * _tilt, _tiltSpeed);

        // 2. 左右の回転（Y軸）
        float targetY = Mathf.LerpAngle(currentRot.y, _horiInput * _tilt, _tiltSpeed);

        // 3. Z軸（ロール）の計算
        float targetZ = 0;
        float currentLeanSpeed = _tiltSpeed; // 基本の速さ

        if (_leanInput != 0)
        {
            // Q/Eが押されている時は、移動の傾きを無視して指定角度(95度)へ
            targetZ = -_leanInput * 95f;
            currentLeanSpeed = 0.2f; // Q/E時は少し素早く傾くように設定（お好みで）
        }
        else
        {
            // Q/Eが離されている時は、左右移動に合わせて少し傾ける
            targetZ = -_horiInput * _tilt;
            currentLeanSpeed = _leanSpeed; // 戻る時の速さ
        }

        // 最終的な角度を適用
        float lerpedZ = Mathf.LerpAngle(currentRot.z, targetZ, currentLeanSpeed);
        transform.localEulerAngles = new Vector3(targetX, targetY, lerpedZ);
    }

    void ClampToScreen()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp(pos.x, 0.05f, 0.95f);
        pos.y = Mathf.Clamp(pos.y, 0.05f, 0.95f);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }
}