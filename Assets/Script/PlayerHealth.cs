using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI References")]
    public Slider _healthSlider;
    public Image _damageFlashImage;

    [Header("Camera Reference")]
    public CamaraController _cameraController; // ここにメインカメラのCameraControllerをセット

    [Header("Settings")]
    public float _maxHealth = 100f;
    public float _flashSpeed = 5f;
    public Color _flashColor = new Color(1f, 0f, 0f, 0.4f);

    private float _currentHealth;

    void Start()
    {
        _currentHealth = _maxHealth;

        if (_healthSlider != null)
        {
            _healthSlider.maxValue = _maxHealth;
            _healthSlider.value = _maxHealth;
        }

        // 自動でCameraControllerを探す（もしセットし忘れても大丈夫なように）
        if (_cameraController == null)
        {
            _cameraController = Camera.main.GetComponent<CamaraController>();
        }
    }

    void Update()
    {
        if (_damageFlashImage != null && _damageFlashImage.color.a > 0)
        {
            _damageFlashImage.color = Color.Lerp(_damageFlashImage.color, Color.clear, _flashSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;

        // 1. 画面フラッシュ
        if (_damageFlashImage != null) _damageFlashImage.color = _flashColor;

        // 2. カメラシェイク（0.2秒間、強さ0.3で揺らす）
        if (_cameraController != null)
        {
            _cameraController.TriggerShake(0.2f, 0.3f);
        }

        if (_healthSlider != null) _healthSlider.value = _currentHealth;

        if (_currentHealth <= 0) Die();
    }

    void Die()
    {
        Debug.Log("Game Over");
        Time.timeScale = 0;
    }
}