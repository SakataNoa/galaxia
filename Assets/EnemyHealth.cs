using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    public float _health = 1f;
    public GameObject _explosionPrefab; // 爆発エフェクト（あれば）
    public int _scoreValue = 100;       // スコア（後で使います）

    // 弾（Trigger設定）が当たった時に呼ばれる
    private void OnTriggerEnter(Collider other)
    {
        // 当たった相手のタグが "Bullet"（または弾のプレハブ名）かチェック
        // 今回はシンプルに、何かが当たったらダメージを受ける設定にします
        if (other.CompareTag("PlayerBullet") || other.name.Contains("Laser"))
        {
            TakeDamage(1);

            // 弾の方を消す（貫通させない場合）
            Destroy(other.gameObject);
        }
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;

        if (_health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 爆発エフェクトを生成
        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, transform.rotation);
        }

        Debug.Log("敵を撃破！スコア: " + _scoreValue);

        // 自分自身を消去
        Destroy(gameObject);
    }
}