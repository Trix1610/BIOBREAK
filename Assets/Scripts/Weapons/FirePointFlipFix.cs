using UnityEngine;

public class FirePointFlipFix : MonoBehaviour
{
    private void LateUpdate()
    {
        // Компенсируем переворот родителя, чтобы FirePoint всегда смотрел правильно
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
