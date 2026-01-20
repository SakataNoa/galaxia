using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("HP Settings")]
    public int _maxHealth = 100;
    public int _currentHealth;
    public Slider _hpSlider;

    [Header("Laser Stock Settings")]
    public int _maxLaserStock = 10;
    public int _currentLaserStock = 0;
    public Slider _laserSlider;

    [Header("Visual Effects")]
    public GameObject _hitEffect;
    public GameObject _fullChargeEffect;
    public Image _damageFlashImage; // 【追加】画面を赤くするUIパネル (Image)
    public Color _normalGaugeColor = Color.cyan;
    public Color _fullChargeColor = Color.yellow;
    private Image _laserFillImage;

    [Header("Audio")]
    public AudioSource _audioSource;
    public AudioClip _fullChargeSE;

    [HideInInspector] public bool _isRolling = false;
    private bool _wasFullLastFrame = false;

    void Start()
    {
        _currentHealth = _maxHealth;
        if (_hpSlider != null)
        {
            _hpSlider.maxValue = _maxHealth;
            _hpSlider.value = _currentHealth;
        }

        if (_laserSlider != null)
        {
            _laserSlider.maxValue = _maxLaserStock;
            _laserSlider.value = _currentLaserStock;
            _laserFillImage = _laserSlider.fillRect.GetComponent<Image>();
        }

        if (_fullChargeEffect != null) _fullChargeEffect.SetActive(false);
        if (_damageFlashImage != null) _damageFlashImage.color = Color.clear; // 初期状態は透明
    }

    void Update()
    {
        HandleFullChargeEffect();
    }

    public void AddLaserStock(int amount)
    {
        if (_currentLaserStock < _maxLaserStock)
        {
            _currentLaserStock += amount;
            _currentLaserStock = Mathf.Clamp(_currentLaserStock, 0, _maxLaserStock);
            UpdateLaserUI();
        }
    }

    public bool TryUseLaser()
    {
        if (_currentLaserStock > 0)
        {
            _currentLaserStock--;
            UpdateLaserUI();
            return true;
        }
        return false;
    }

    void UpdateLaserUI()
    {
        if (_laserSlider != null) _laserSlider.value = _currentLaserStock;
    }

    private void HandleFullChargeEffect()
    {
        bool isFull = _currentLaserStock >= _maxLaserStock;
        if (_laserFillImage != null)
        {
            if (isFull)
            {
                float flash = Mathf.PingPong(Time.time * 5f, 1f);
                _laserFillImage.color = Color.Lerp(_fullChargeColor, Color.white, flash);
                if (!_wasFullLastFrame) OnFullChargeReach();
            }
            else
            {
                _laserFillImage.color = _normalGaugeColor;
                if (_fullChargeEffect != null) _fullChargeEffect.SetActive(false);
            }
        }
        _wasFullLastFrame = isFull;
    }

    private void OnFullChargeReach()
    {
        if (_audioSource != null && _fullChargeSE != null) _audioSource.PlayOneShot(_fullChargeSE);
        if (_fullChargeEffect != null) _fullChargeEffect.SetActive(true);
    }

    public void TakeDamage(int damage)
    {
        if (_isRolling) return;

        _currentHealth -= damage;
        if (_hpSlider != null) _hpSlider.value = _currentHealth;

        // 1. 画面フラッシュ演出
        if (_damageFlashImage != null) StartCoroutine(FlashDamageUI());

        // 2. エフェクト生成
        if (_hitEffect != null) Instantiate(_hitEffect, transform.position, Quaternion.identity);

        // 3. カメラの揺れ（マイルドに設定）
        CameraController cam = Camera.main.GetComponent<CameraController>();
        if (cam != null) cam.TriggerShake(0.15f, 0.2f);

        if (_currentHealth <= 0) Debug.Log("Player Destroyed");
    }

    private IEnumerator FlashDamageUI()
    {
        _damageFlashImage.color = new Color(1, 0, 0, 0.4f); // 赤色を0.4の透明度で表示
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            _damageFlashImage.color = Color.Lerp(new Color(1, 0, 0, 0.4f), Color.clear, elapsed / 0.2f);
            yield return null;
        }
        _damageFlashImage.color = Color.clear;
    }

    // --- ここで全ての判定を行う ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            // 弾側のスクリプトからダメージ値を取得
            EnemyBullet bullet = other.GetComponent<EnemyBullet>();
            int damageValue = (bullet != null) ? bullet._damage : 10;

            if (_isRolling)
            {
                AddLaserStock(1);
            }
            else
            {
                TakeDamage(damageValue);
            }

            // 弾をここで消す（二重判定防止）
            Destroy(other.gameObject);
        }
    }
}