using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Settings")]
    public float _maxHealth = 100f;
    public Slider _healthSlider;

    private float _currentHealth;

    void Start()
    {
        _currentHealth = _maxHealth;

        if (_healthSlider != null)
        {
            _healthSlider.maxValue = _maxHealth;
            _healthSlider.minValue = 0f;
            _healthSlider.value = _maxHealth;
        }
        else
        {
            Debug.LogError("Health Sliderがアサインされていません！");
        }
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        Debug.Log("ダメージを受けました！ 残りHP: " + _currentHealth);

        if (_healthSlider != null)
        {
            _healthSlider.value = _currentHealth;
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("プレイヤーが破壊されました");
        // ゲームオーバー処理
    }
}