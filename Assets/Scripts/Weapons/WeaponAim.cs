using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponAim : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        FindCamera();
    }

    private void Update()
    {
        // Если камера по какой-то причине пропала (например, уничтожена), пробуем найти её снова
        if (mainCamera == null)
        {
            FindCamera();
            if (mainCamera == null) return; // Если камеры всё еще нет, пропускаем кадр, чтобы не было ошибки
        }

        if (Mouse.current == null) return;

        // 1. Получаем позицию мыши через новый Input System
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        
        // 2. Переводим в мировые координаты игры
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 0f));
        mouseWorldPosition.z = 0f; // Обнуляем Z для 2D

        // 3. Находим вектор направления и вычисляем угол
        Vector3 direction = mouseWorldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 4. Поворачиваем объект в сторону мыши
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