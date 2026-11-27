using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class soda_script : MonoBehaviour
{
    [SerializeField] public string perkName = "PowerPop"; // Name of the perk
    [SerializeField] public int perkID = 1; // Unique ID for the perk
    [SerializeField] public int sodaCost = 1500; // Cost of the PowerPop perk
    Mod_Weapon_Script modWeaponScript = new Mod_Weapon_Script(); // Create an instance of Mod_Weapon_Script
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Spaceman spaceman = collision.GetComponent<Spaceman>();
            if (spaceman != null)
            {
                spaceman.RegisterPerks(this);
                Debug.Log("In range of consumable. Press interact.");
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
                Debug.Log("Left Consumable range.");
            }
        }
    }

    // This gets called by the Spaceman script when OnInteract runs
    // The unique effect of this machine (Juggernog style max health upgrade)
    public void ApplyEffect(Spaceman spaceman)
    {
        if (perkID == 1) 
        { 
        spaceman.lives = spaceman.maxLives; // Fill health to the new max
        Debug.Log("Reach for PowerPop Tonight! Max lives increased.");
        }
        else if (perkID==2)
        {
            spaceman.prime_fireRate = spaceman.prime_fireRate / 2;
            spaceman.wp1_fireRate = spaceman.wp1_fireRate / 2;
            spaceman.wp2_fireRate = spaceman.wp2_fireRate / 2;
            spaceman.wp3_fireRate = spaceman.wp3_fireRate / 2;
        }
        else if (perkID == 5)
        {
            if (spaceman.prime_wpn_id != -1 && spaceman.wpn_prime_upgraded==false)
            {
                spaceman.scrap -= sodaCost; // Deduct the cost of the perk from the player's scrap
                modWeaponScript.ApplyWeaponMod(spaceman);
                Debug.Log($"Successfully purchased your weapon using {perkName}!");
            }
        }
    }
}