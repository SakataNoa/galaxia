using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Settings")]
    public float _health = 1f;
    public GameObject _explosionPrefab;
    public int _scoreValue = 100;

    private void OnTriggerEnter(Collider other)
    {
        // ’e‚ª“–‚½‚Á‚½”»’è
        if (other.CompareTag("PlayerBullet") || other.name.Contains("Laser"))
        {
            TakeDamage(1);
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
        if (_explosionPrefab != null)
        {
            Instantiate(_explosionPrefab, transform.position, transform.rotation);
        }

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddScore(_scoreValue);
        }

        Destroy(gameObject);
    }
}