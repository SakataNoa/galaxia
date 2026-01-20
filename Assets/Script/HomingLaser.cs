using UnityEngine;

[RequireComponent(typeof(TrailRenderer))] // TrailRendererを必須にする
public class HomingLaser : MonoBehaviour
{
    private Transform _target;

    [Header("Movement Settings")]
    public float _speed = 60f;
    public float _rotateSpeed = 15f;
    public float _lifeTime = 3f;

    [Header("Damage Settings")]
    public float _damage = 50f;

    private TrailRenderer _trail;

    private void Awake()
    {
        // TrailRendererの取得と初期設定
        _trail = GetComponent<TrailRenderer>();
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        // 生成された瞬間にターゲットの方を向かせる（急旋回を防ぐため）
        if (_target != null)
        {
            transform.LookAt(_target);
        }

        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        // ターゲットが存在し、かつアクティブな場合のみ追跡
        if (_target != null && _target.gameObject.activeInHierarchy)
        {
            // ターゲットへの方向を計算
            Vector3 direction = _target.position - transform.position;

            if (direction != Vector3.zero)
            {
                // ターゲットの方向へ徐々に向きを変える
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
            }
        }

        // 常に前方に移動（ホーミング中も直進ベクトルを加えることで曲線を描く）
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 敵に当たった場合の判定
        if (other.CompareTag("Enemy"))
        {
            // ダメージ処理の例（敵側にスクリプトがある場合）
            /*
            var enemy = other.GetComponent<EnemyHealth>();
            if (enemy != null) enemy.TakeDamage(_damage);
            */

            // 当たった瞬間に弾を消す
            // TrailRendererを即座に消すと不自然なので、親子関係を切り離して消滅させる手法もありますが、
            // 今回はシンプルに弾ごと消去します。
            Destroy(gameObject);
        }
    }
}