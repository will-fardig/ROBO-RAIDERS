using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class power_pop_script : MonoBehaviour
{
    public int powerPopCost = 1500; // Cost of the PowerPop perk
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Spaceman spaceman = collision.GetComponent<Spaceman>();
            if (spaceman != null)
            {
                spaceman.RegisterPerks(this);
                Debug.Log("In range of PowerPop! Press interact.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Spaceman spaceman = collision.GetComponent<Spaceman>();
            if (spaceman != null)
            {
                spaceman.UnregisterPerks(this);
                Debug.Log("Left PowerPop range.");
            }
        }
    }

    // This gets called by the Spaceman script when OnInteract runs
    // The unique effect of this machine (Juggernog style max health upgrade)
    public void ApplyEffect(Spaceman spaceman)
    {
        spaceman.lives = spaceman.maxLives; // Fill health to the new max
        Debug.Log("Reach for PowerPop Tonight! Max lives increased.");
    }
}