using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    [Header("Settings")]
    public GameObject _bulletPrefab;   // 敵の弾プレハブ
    public float _fireRate = 2.0f;     // 何秒ごとに撃つか
    public float _bulletSpeed = 40f;   // 弾の速さ
    public float _shootingDistance = 100f; // プレイヤーとの距離がこれ以内なら撃つ

    private Transform _player;
    private float _nextFireTime;

    void Start()
    {
        // プレイヤーを探す（Playerタグがついている前提）
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
    }

    void Update()
    {
        if (_player == null) return;

        // プレイヤーとの距離を計算
        float distance = Vector3.Distance(transform.position, _player.position);

        // 距離が近く、かつ発射タイミングが来たら撃つ
        if (distance < _shootingDistance && Time.time > _nextFireTime)
        {
            Shoot();
            _nextFireTime = Time.time + _fireRate;
        }
    }

    void Shoot()
    {
        if (_bulletPrefab == null) return;

        // 弾を生成
        GameObject bullet = Instantiate(_bulletPrefab, transform.position, transform.rotation);

        // プレイヤーの方へ飛ばす方向を計算
        Vector3 direction = (_player.position - transform.position).normalized;

        // 弾に速度を与える（Rigidbodyを使わないシンプルな移動）
        EnemyBullet bulletScript = bullet.AddComponent<EnemyBullet>();
        bulletScript.Setup(direction, _bulletSpeed);
    }
}