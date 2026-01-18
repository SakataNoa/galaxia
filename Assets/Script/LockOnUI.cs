using UnityEngine;
using UnityEngine.UI;

public class LockOnUI : MonoBehaviour
{
    private Transform _target;
    private Camera _cam;

    public void Setup(Transform target)
    {
        _target = target;
        _cam = Camera.main;
    }

    void Update()
    {
        // ターゲットが破壊されたら自分も消える
        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 敵の3D位置を2Dの画面座標に変換
        Vector3 screenPos = _cam.WorldToScreenPoint(_target.position);

        // 画面外（カメラの後ろ）なら表示しない
        if (screenPos.z < 0)
        {
            GetComponent<Image>().enabled = false;
        }
        else
        {
            GetComponent<Image>().enabled = true;
            transform.position = screenPos;
        }
    }
}