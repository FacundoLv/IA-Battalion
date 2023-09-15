using UnityEngine;
using UnityEngine.UI;

public class ProtectionBar : MonoBehaviour
{
    [SerializeField] private Image _image;

    private BossUnit _owner;
    private Camera _camera;

    private void Awake()
    {
        _owner = GetComponentInParent<BossUnit>();
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
        _owner.OnMinionCountUpdated += UpdateProtection;
    }

    private void OnDisable()
    {
        _owner.OnMinionCountUpdated -= UpdateProtection;
    }

    private void UpdateProtection(int currentMinions, int maxMinions)
    {
        _image.fillAmount = (float) currentMinions / maxMinions;
    }
}
