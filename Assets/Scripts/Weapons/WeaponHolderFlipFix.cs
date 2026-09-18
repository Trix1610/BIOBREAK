using UnityEngine;

public class WeaponHolderFlipFix : MonoBehaviour
{
    private void LateUpdate()
    {
        // Компенсируем переворот родителя (Player), чтобы WeaponHolder не переворачивался
        if (transform.parent != null)
        {
            Vector3 parentScale = transform.parent.localScale;
            if (parentScale.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }
}
