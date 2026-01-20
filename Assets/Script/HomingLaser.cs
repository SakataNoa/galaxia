using UnityEngine;

public class HomingLaser : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private float _rotateSpeed = 800f; // 回転性能を大幅アップ
    private float _homingDuration = 2.0f; // 追尾する時間
    private float _timer = 0;

    public void Initialize(Transform target, float speed)
    {
        _target = target;
        _speed = speed;
        _timer = 0;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_target != null && _timer < _homingDuration)
        {
            // ターゲット方向への回転を計算
            Vector3 targetDir = (_target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(targetDir);

            // 非常に強くターゲットを向くように調整
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, _rotateSpeed * Time.deltaTime);
        }

        // 常に前進
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth health = other.GetComponent<EnemyHealth>();
            if (health != null) health.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}