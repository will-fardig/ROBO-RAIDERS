using UnityEngine;
using UnityEngine.InputSystem;

public class HealthBarDemo : MonoBehaviour
{
    [SerializeField] private Spaceman player;

    private void Update()
    {
        if (player == null || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            player.TakeDamage(
                5,
                player.transform.position + Vector3.right
            );
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            player.Heal(5);
        }
    }
}