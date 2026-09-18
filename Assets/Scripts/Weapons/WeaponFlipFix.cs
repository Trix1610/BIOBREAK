using UnityEngine;

public class WeaponFlipFix : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null) return;

        // Получаем угол поворота объекта (в градусах)
        float angle = transform.eulerAngles.z;

        // Нормализуем угол в диапазон -180...180
        if (angle > 180f) angle -= 360f;

        // Если угол между -90 и 90 градусами - оружие смотрит вправо
        // Если угол между 90 и 180 или -180 и -90 - оружие смотрит влево
        bool isAimingLeft = angle > 90f || angle < -90f;

        // Переворачиваем спрайт если оружие смотрит влево
        spriteRenderer.flipY = isAimingLeft;
    }
}
