using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header ("Events")]
    public UnityEvent OnHealthChange;
    public UnityEvent OnPlayerDeath;

    [Header ("Health")]
    [SerializeField] private int _startingHealth = 6;
    [SerializeField] private int _currentHealth; // serialized for testing purposes
    private bool _isDead = false;

    [Header ("IFrames")]
    [SerializeField] private float _iFramesDuration = 1f;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteRenderer _grappleSprite;
    [SerializeField] private float _iFramesDeltaTime = 0.15f;
    private float _lastHit;
    private WaitForSeconds _iFramesTick;

    [Header ("Grapple Gun")]
    [SerializeField] private GameObject _grappleGunPrefab;
    [SerializeField] private float _pushValue = 1000f;
    private GameObject _grappleGun;

    [Header ("Shake parameters")]
    [SerializeField] private GameObject _healthBar;
    [SerializeField] private float _shakeIntensity = 10f;
    [SerializeField] private float _shakeTime = 0.5f;

    [Header ("Particles")]
    [SerializeField] private ParticleSystem _hitParticles;

    public int StartingHealth => _startingHealth;
    public int CurrentHealth => _currentHealth;

    private void Awake()
    {
        _currentHealth = _startingHealth;
    }

    private void Start()
    {
        _iFramesTick = new WaitForSeconds(_iFramesDeltaTime);
    }

    private void Update()
    {
        Testing();
    }

    private void Testing()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            _grappleGun = Instantiate(_grappleGunPrefab, GameManager.Instance.player.transform.position + new Vector3(0.32f, 0, 0), Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f)));
            _grappleGun.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-_pushValue , _pushValue), Random.Range(-_pushValue , _pushValue)));
        }
        if (Input.GetKeyDown(KeyCode.F))
            TakeDamage(1);
    }

    public void TakeDamage(int damage)
    {
        if (Time.time - _lastHit <= _iFramesDuration) return;

        Debug.Log(this.name + " took " + damage + " damage");

        _currentHealth -= damage;

        if (_currentHealth > 0)
        {
            _lastHit = Time.time;
            StartCoroutine(StartIFrames());
            OnHealthChange.Invoke();
            _hitParticles.Play();
            FunctionTimer.Create(() => _hitParticles.Stop(), _hitParticles.main.duration);
            iTween.ShakePosition(_healthBar, Vector3.one * _shakeIntensity, _shakeTime);
        }
        else if (!_isDead)
        {
            OnPlayerDeath.Invoke();
            LaunchGrappleGun();
        }
    }

    private IEnumerator StartIFrames()
    {
        while (Time.time - _lastHit <= _iFramesDuration)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            _grappleSprite.enabled = !_grappleSprite.enabled;
            yield return _iFramesTick;
        }
        _spriteRenderer.enabled = true;
        _grappleSprite.enabled = true;
    }
    
    // Disables and spawns a new grapple gun, which flies off in a random direction
    private void LaunchGrappleGun()
    {
        GameManager.Instance.grappleGun.SetActive(false);
        //GameManager.Instance.player.GetComponent<BoxCollider2D>().enabled = false;
        _grappleGun = Instantiate(_grappleGunPrefab, GameManager.Instance.player.transform.position + new Vector3(0.32f, 0, 0), Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f)));
        _grappleGun.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-_pushValue , _pushValue), Random.Range(-_pushValue , _pushValue)));
    }

    public void AddHealth(int value)
    {
        _currentHealth += value;
        if (_currentHealth > _startingHealth)
            _currentHealth = _startingHealth;
        OnHealthChange.Invoke();
    }

    public bool IsFullHealth()
    {
        return _currentHealth == _startingHealth;
    }

    public bool IsDead()
    {
        return _currentHealth <= 0;
    }
}