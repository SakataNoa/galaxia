using UnityEngine;
using UnityEngine.InputSystem; // 1. 新しいInput Systemの名前空間を追加

public class PlayerController : MonoBehaviour
{
    float _horiInput;
    float _verInput;
    public float _moveSpeed;
    public float _tilt;
    public float _tiltSpeed;

    // 2. 入力値を保持する変数（Vector2型が便利です）
    private Vector2 _inputMovement;

    // 3. インスペクターからではなく、メッセージ経由で入力を受け取る
    public void OnMove(InputValue value)
    {
        _inputMovement = value.Get<Vector2>();
        _horiInput = _inputMovement.x;
        _verInput = _inputMovement.y;
    }

    void Update()
    {
        // GetAxisは不要になったので削除し、傾き処理だけ残す
        HandleTilting();
        ClampToScreen();
    }

    private void FixedUpdate() // 4. Unityの仕様上、Fは大文字(FixedUpdate)にする必要があります
    {
        Movement();
    }

    void Movement()
    {
        transform.Translate(Vector3.forward * _moveSpeed * Time.fixedDeltaTime);

        Vector3 movement = new Vector3(_horiInput, _verInput, 0);
        transform.localPosition += movement * _moveSpeed * Time.fixedDeltaTime;
    }

    void HandleTilting()
    {
        TiltZ(_horiInput);
        TiltX(_verInput);
    }

    void TiltZ(float axis)
    {
        Vector3 targetEuAng = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(targetEuAng.x, Mathf.LerpAngle(targetEuAng.y, axis * _tilt, _tiltSpeed), Mathf.LerpAngle(targetEuAng.z, -axis * _tilt, _tiltSpeed));
    }

    void TiltX(float axis)
    {
        Vector3 targetEuAng = transform.localEulerAngles;
        transform.localEulerAngles = new Vector3(Mathf.LerpAngle(targetEuAng.x, -axis * _tilt, _tiltSpeed), targetEuAng.y, Mathf.LerpAngle(targetEuAng.z, axis * _tilt, _tiltSpeed));
    }

    void ClampToScreen()
    {
        Vector3 pos = Camera.main.WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = Camera.main.ViewportToWorldPoint(pos);
    }
}