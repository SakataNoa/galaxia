using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;

    [Header("Settings")]
    public int _damage = 10;
    public float _lifeTime = 5f;

    public void Setup(Vector3 dir, float speed)
    {
        _direction = dir.normalized;
        _speed = speed;
        if (_direction != Vector3.zero) transform.forward = _direction;
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        transform.position += _direction * _speed * Time.deltaTime;
    }

}