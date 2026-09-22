using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    private Camera mainCamera;
    private float nextCameraSearchTime;
    private InputAction lookAction;

    private void Awake()
    {
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput != null)
        {
            lookAction = playerInput.actions.FindAction("Look", throwIfNotFound: false);
        }

        FindCamera();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        // Если камера по какой-то причине пропала (например, уничтожена), пробуем найти её снова
        if (mainCamera == null)
        {
            if (Time.time < nextCameraSearchTime)
                return;

            nextCameraSearchTime = Time.time + 0.2f;
            FindCamera();
            if (mainCamera == null) return; // Если камеры всё еще нет, пропускаем кадр, чтобы не было ошибки
        }

        if (lookAction == null)
            return;

        Vector2 lookValue = lookAction.ReadValue<Vector2>();
        Vector2 direction;

        if (lookAction.activeControl?.device is Gamepad ||
            lookAction.activeControl?.device is Joystick)
        {
            direction = lookValue;
        }
        else
        {
            Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
                new Vector3(lookValue.x, lookValue.y, 0f));
            direction = mouseWorldPosition - transform.position;
        }

        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void FindCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // Используем FindAnyObjectByType вместо устаревшего FindFirstObjectByType
            mainCamera = FindAnyObjectByType<Camera>();
        }
    }
}
