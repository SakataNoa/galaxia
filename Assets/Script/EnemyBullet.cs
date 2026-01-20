using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    public float _damage = 10f; // 弾の威力

    public void Setup(Vector3 dir, float speed)
    {
        _direction = dir;
        _speed = speed;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // プレイヤーに当たった場合
        if (other.CompareTag("Player"))
        {
            // PlayerHealthコンポーネントを取得してダメージを与える
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(_damage);
            }

            Destroy(gameObject);
        }
    }
}