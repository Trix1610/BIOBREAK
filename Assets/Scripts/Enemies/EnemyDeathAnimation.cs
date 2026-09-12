using System.Collections;
using UnityEngine;

namespace Enemies
{
    public class EnemyDeathAnimation
    {
        private readonly EnemyDeathConfig config;

        public EnemyDeathAnimation(EnemyDeathConfig config)
        {
            this.config = config;
        }

        public IEnumerator PlayDeathAnimation()
        {
            if (config.Collider != null) config.Collider.enabled = false;
            
            if (config.Rb != null) 
            {
                config.Rb.linearVelocity = Vector2.zero;
                config.Rb.simulated = false;
            }

            if (config.HealthCanvasObject != null)
                config.HealthCanvasObject.SetActive(false);

            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 initialScale = config.Transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                config.Transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);

                if (config.SpriteRenderer != null)
                {
                    Color color = config.SpriteRenderer.color;
                    color.a = Mathf.Lerp(1f, 0f, t);
                    config.SpriteRenderer.color = color;
                }

                yield return null;
            }
        }
    }
}
