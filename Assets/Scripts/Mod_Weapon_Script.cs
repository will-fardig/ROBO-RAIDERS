using UnityEngine;

public class Mod_Weapon_Script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void ApplyWeaponMod(Spaceman spaceman)
    {
        if (spaceman.prime_wpn_id != -1  && spaceman.wpn_prime_upgraded==false)
        {
            spaceman.wpn_prime_upgraded = true;
            spaceman.prime_wpn_damage *= 2; // Double the damage of the primary weapon
            spaceman.prime_wpn_mag_max *= 2; // Double the magazine size of the primary weapon
            spaceman.prime_wpn_reserve_max *= 2; // Double the reserve ammo of the primary weapon
            spaceman.prime_wpn_mag = spaceman.prime_wpn_mag_max; // Refill magazine to new max
            spaceman.prime_wpn_reserve = spaceman.prime_wpn_reserve_max; // Refill reserve to new max
            spaceman.prime_wpn_automatic= true; // Make the primary weapon automatic

            spaceman.prime_pellets = spaceman.prime_pellets * 2; // Double the pellets of the primary weapon

            Debug.Log("Primary weapon upgraded! Damage doubled.");
            if (spaceman.prime_wpn_id == spaceman.wpn1_id) //if it is equal to weapon 1 slot
            {
                spaceman.wpn1_mag_max = spaceman.prime_wpn_mag_max;
                spaceman.wpn1_mag = spaceman.prime_wpn_mag_max;
                spaceman.wpn1_reserve_max = spaceman.prime_wpn_reserve_max;
                spaceman.wpn1_reserve = spaceman.prime_wpn_reserve_max;
                spaceman.wpn1_damage = spaceman.prime_wpn_damage;
                spaceman.wp1_upgraded = spaceman.wpn_prime_upgraded;
                spaceman.wp1_automatic = spaceman.prime_wpn_automatic;
                spaceman.wp1_pellets = spaceman.prime_pellets;
            }
            if (spaceman.prime_wpn_id == spaceman.wpn2_id) //if it is equal to weapon 2 slot
            {
                spaceman.wpn2_mag_max = spaceman.prime_wpn_mag_max;
                spaceman.wpn2_mag = spaceman.prime_wpn_mag_max;
                spaceman.wpn2_reserve_max = spaceman.prime_wpn_reserve_max;
                spaceman.wpn2_reserve = spaceman.prime_wpn_reserve_max;
                spaceman.wpn2_damage = spaceman.prime_wpn_damage;
                spaceman.wp2_upgraded = spaceman.wpn_prime_upgraded;
                spaceman.wp2_automatic = spaceman.prime_wpn_automatic;
                spaceman.wp2_pellets = spaceman.prime_pellets;
            }
            if (spaceman.prime_wpn_id == spaceman.wpn3_id) //if it is equal to weapon 3 slot
            {
                spaceman.wpn3_mag_max = spaceman.prime_wpn_mag_max;
                spaceman.wpn3_mag = spaceman.prime_wpn_mag_max;
                spaceman.wpn3_reserve_max = spaceman.prime_wpn_reserve_max;
                spaceman.wpn3_reserve = spaceman.prime_wpn_reserve_max;
                spaceman.wpn3_damage = spaceman.prime_wpn_damage;
                spaceman.wp3_upgraded = spaceman.wpn_prime_upgraded;
                spaceman.wp3_automatic = spaceman.prime_wpn_automatic;
                spaceman.wp3_pellets = spaceman.prime_pellets;
            }
        }
    }

    
}
