using UnityEngine;

public class wallbuy_script : MonoBehaviour
{
    [Header("Wall Buy Settings")]
    [SerializeField] private int wall_buy_cost;
    [SerializeField] private string weaponName;

    [Header("Weapon Stats")]
    [SerializeField] public int wpn_buy_id; // 0 is pistol, 1+ for wall weapons
    [SerializeField] public int wpn_buy_mag_max;
    [SerializeField] public int wpn_buy_reserve_max;
    [SerializeField] public int wpn_buy_damage;
    [SerializeField] private float wpn_buy_reloadTime;
    [SerializeField] public float wpn_buy_fireRate;
    [SerializeField] public bool wpn_buy_automatic;   // hold to keep firing

    [Header("Bullet Feel")]
    [SerializeField] public float wpn_buy_bulletSpeed = 8f;   // units per second
    [SerializeField] public float wpn_buy_bulletSize = 1f;    // 1 = bullet prefab's normal size
    [SerializeField] public float wpn_buy_spread = 0f;        // total cone in degrees (0 = laser accurate)
    [SerializeField] public int wpn_buy_pellets = 1;          // bullets per shot (shotguns > 1)
    [SerializeField] public int wpn_buy_pierce = 0;           // extra enemies each bullet passes through
    [SerializeField] public bool wpn_buy_upgraded;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Spaceman spaceman = collision.GetComponent<Spaceman>();
            if (spaceman != null)
            {
                spaceman.RegisterWallBuy(this);
                Debug.Log($"In range of {weaponName}! Press interact to buy for {wall_buy_cost} Scrap.");
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
                spaceman.UnregisterWallBuy(this);
                Debug.Log($"Left {weaponName} range.");
            }
        }
    }

    public void Interact()
    {
        Spaceman spaceman = FindAnyObjectByType<Spaceman>();
        if (spaceman != null)
        {
            spaceman.TryBuyWallWeapon(this, wall_buy_cost, wpn_buy_id);
        }
    }
    // Applied to the player when the purchase succeeds

    public void ApplyWeapon(Spaceman spaceman)
    {
        Debug.Log($"Buying Weapon ID: {wpn_buy_id}");
        Debug.Log($"Weapon 1: {spaceman.weapon_ids[0]}");
        Debug.Log($"Weapon 2: {spaceman.weapon_ids[1]}");
        Debug.Log($"Maximum Weapons: {spaceman.GetMaxWeapons()}");
        int maxWeapons = spaceman.GetMaxWeapons();

        // 1. Check if player already owns this weapon.
        int existingSlot = -1;

        for (int i = 0; i < maxWeapons; i++)
        {
            if (i < spaceman.weapon_ids.Count &&
                (int)spaceman.weapon_ids[i] == wpn_buy_id)
            {
                existingSlot = i;
                break;
            }
        }

        // 2. Already owned: refill ammunition.
        if (existingSlot != -1)
        {
            if (existingSlot == 0)
            {
                spaceman.wpn1_mag = wpn_buy_mag_max;
                spaceman.wpn1_reserve = wpn_buy_reserve_max;
            }
            else if (existingSlot == 1)
            {
                spaceman.wpn2_mag = wpn_buy_mag_max;
                spaceman.wpn2_reserve = wpn_buy_reserve_max;
            }
            else if (existingSlot == 2)
            {
                spaceman.wpn3_mag = wpn_buy_mag_max;
                spaceman.wpn3_reserve = wpn_buy_reserve_max;
            }

            if (spaceman.prime_wpn_id == wpn_buy_id)
            {
                spaceman.prime_wpn_mag = wpn_buy_mag_max;
                spaceman.prime_wpn_reserve = wpn_buy_reserve_max;
            }

            Debug.Log($"Ammo refilled for {weaponName}!");
            return;
        }

        // 3. Make sure the inventory list has enough entries.
        while (spaceman.weapon_ids.Count < maxWeapons)
        {
            spaceman.weapon_ids.Add(-1);
        }

        // 4. Find an empty slot first.
        int targetSlot = -1;

        for (int i = 0; i < maxWeapons; i++)
        {
            if ((int)spaceman.weapon_ids[i] == -1)
            {
                targetSlot = i;
                break;
            }
        }

        // 5. All slots full: replace a weapon that isn't equipped.
        if (targetSlot == -1)
        {
            for (int i = 0; i < maxWeapons; i++)
            {
                if ((int)spaceman.weapon_ids[i] != spaceman.prime_wpn_id)
                {
                    targetSlot = i;
                    break;
                }
            }
        }

        if (targetSlot == -1)
        {
            Debug.LogWarning("Could not find a weapon slot to replace!");
            return;
        }

        // 6. Assign the new weapon ID.
        spaceman.weapon_ids[targetSlot] = wpn_buy_id;

        // 7. Apply weapon stats to the selected slot.
        switch (targetSlot)
        {
            case 0:
                spaceman.wpn1_id = wpn_buy_id;
                spaceman.wpn1_mag_max = wpn_buy_mag_max;
                spaceman.wpn1_mag = wpn_buy_mag_max;
                spaceman.wpn1_reserve_max = wpn_buy_reserve_max;
                spaceman.wpn1_reserve = wpn_buy_reserve_max;
                spaceman.wpn1_damage = wpn_buy_damage;
                spaceman.wp1_reloadTime = wpn_buy_reloadTime;
                spaceman.wp1_fireRate = wpn_buy_fireRate;
                spaceman.wp1_upgraded = wpn_buy_upgraded;
                spaceman.wp1_automatic = wpn_buy_automatic;
                spaceman.wp1_bulletSpeed = wpn_buy_bulletSpeed;
                spaceman.wp1_bulletSize = wpn_buy_bulletSize;
                spaceman.wp1_spread = wpn_buy_spread;
                spaceman.wp1_pellets = wpn_buy_pellets;
                spaceman.wp1_pierce = wpn_buy_pierce;
                if (spaceman.PERKS_ORDER.ContainsValue("Double Trouble Tonic"))
                {
                    spaceman.wp1_fireRate = spaceman.wp1_fireRate / 2; // Double the fire rate of the primary weapon
                    spaceman.wp1_pellets *=2; // Double the pellets of the primary weapon
                }
                if (spaceman.PERKS_ORDER.ContainsValue("Fizzy Fastness"))
                {
                    spaceman.wp1_reloadTime = spaceman.wp1_reloadTime / 2; // Double the fire rate of the primary weapon
                }
                break;

            case 1:
                spaceman.wpn2_id = wpn_buy_id;
                spaceman.wpn2_mag_max = wpn_buy_mag_max;
                spaceman.wpn2_mag = wpn_buy_mag_max;
                spaceman.wpn2_reserve_max = wpn_buy_reserve_max;
                spaceman.wpn2_reserve = wpn_buy_reserve_max;
                spaceman.wpn2_damage = wpn_buy_damage;
                spaceman.wp2_reloadTime = wpn_buy_reloadTime;
                spaceman.wp2_fireRate = wpn_buy_fireRate;
                spaceman.wp2_upgraded = wpn_buy_upgraded;
                spaceman.wp2_automatic = wpn_buy_automatic;
                spaceman.wp2_bulletSpeed = wpn_buy_bulletSpeed;
                spaceman.wp2_bulletSize = wpn_buy_bulletSize;
                spaceman.wp2_spread = wpn_buy_spread;
                spaceman.wp2_pellets = wpn_buy_pellets;
                spaceman.wp2_pierce = wpn_buy_pierce;
                if (spaceman.PERKS_ORDER.ContainsValue("Double Trouble Tonic"))
                {
                    spaceman.wp2_fireRate = spaceman.wp2_fireRate / 2; // Double the fire rate of the primary weapon
                    spaceman.wp2_pellets *= 2; // Double the pellets of the primary weapon
                }
                if (spaceman.PERKS_ORDER.ContainsValue("Fizzy Fastness"))
                {
                    spaceman.wp2_reloadTime = spaceman.wp2_reloadTime / 2; // Double the fire rate of the primary weapon
                }
                break;

            case 2:
                spaceman.wpn3_id = wpn_buy_id;
                spaceman.wpn3_mag_max = wpn_buy_mag_max;
                spaceman.wpn3_mag = wpn_buy_mag_max;
                spaceman.wpn3_reserve_max = wpn_buy_reserve_max;
                spaceman.wpn3_reserve = wpn_buy_reserve_max;
                spaceman.wpn3_damage = wpn_buy_damage;
                spaceman.wp3_reloadTime = wpn_buy_reloadTime;
                spaceman.wp3_fireRate = wpn_buy_fireRate;
                spaceman.wp3_upgraded = wpn_buy_upgraded;
                spaceman.wp3_automatic = wpn_buy_automatic;
                spaceman.wp3_bulletSpeed = wpn_buy_bulletSpeed;
                spaceman.wp3_bulletSize = wpn_buy_bulletSize;
                spaceman.wp3_spread = wpn_buy_spread;
                spaceman.wp3_pellets = wpn_buy_pellets;
                spaceman.wp3_pierce = wpn_buy_pierce;
                if (spaceman.PERKS_ORDER.ContainsValue("Double Trouble Tonic"))
                {
                    spaceman.wp3_fireRate = spaceman.wp3_fireRate / 2; // Double the fire rate of the primary weapon
                    spaceman.wp3_pellets *= 2; // Double the pellets of the primary weapon
                }
                if (spaceman.PERKS_ORDER.ContainsValue("Fizzy Fastness"))
                {
                    spaceman.wp3_reloadTime = spaceman.wp3_reloadTime / 2; // Double the fire rate of the primary weapon
                }
                break;
        }

        Debug.Log($"Purchased {weaponName} in slot {targetSlot + 1}!");
        Debug.Log($"Inventory: {string.Join(", ", spaceman.weapon_ids.ToArray())}");
        Debug.Log($"Active weapon: {spaceman.prime_wpn_id}");

        // Switch to it right away so the purchase is felt immediately
        spaceman.EquipWeapon(wpn_buy_id);
    }


}