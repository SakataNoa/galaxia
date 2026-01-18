using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;

    public void Setup(Vector3 dir, float speed)
    {
        _direction = dir;
        _speed = speed;
        // 5秒後に自動消滅
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たったか判定
        if (other.CompareTag("Player"))
        {
            Debug.Log("プレイヤーが被弾！");
            // ここにプレイヤーのダメージ処理を追加する
            Destroy(gameObject);
        }
    }
}