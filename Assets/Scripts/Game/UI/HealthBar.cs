using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _image;

    private IUnit _owner;
    private Camera _camera;

    private void Awake()
    {
        _owner = GetComponentInParent<IUnit>();
    }

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
    }

    private void OnEnable()
    {
        _owner.OnHealthChanged += UpdateHealth;
    }

    private void OnDisable()
    {
        _owner.OnHealthChanged -= UpdateHealth;
    }

    private void UpdateHealth(float currentHealth, float maxHealth)
    {
        _image.fillAmount = currentHealth / maxHealth;
    }
}