using UnityEngine;
using System.Linq;

public class RandomWeaponMachine : BaseMachine
{
    private static Inventory.itemIndex[] armasPosibles = 
    {
        Inventory.itemIndex.M1911,
        Inventory.itemIndex.M93R,
        Inventory.itemIndex.MP5,
        Inventory.itemIndex.UZI,
        Inventory.itemIndex.RPG,
        Inventory.itemIndex.M800,
        Inventory.itemIndex.AK47,
        Inventory.itemIndex.M4,
        Inventory.itemIndex.AWP,
    };

    protected override void Start()
    {
        base.Start();
        fullString = "Random weapon machine (" + PuntosRequeridos + ") (E)";
    }

    protected override void Buy()
    {
        GiveRandomWeapon();
    }

    private void GiveRandomWeapon()
    {
        if (inventory.CheckFull())
        {
            player.AwardPoints(PuntosRequeridos);
            StartCoroutine(VisualPurchase("Your inventory is full"));
            return;
        }
        
        Inventory.itemIndex[] weaponPool = armasPosibles;
        Inventory.itemIndex[] currentWeapons = inventory.getAllWeapons(); 
        Inventory.itemIndex[] result = weaponPool.Except(currentWeapons).ToArray();
        Inventory.itemIndex chosenWeapon = result[Random.Range(0,result.Length)];

        inventory.AddItem(new Inventory.Item(Inventory.itemType.Weapon, chosenWeapon, isWeapon: true, amount: 1));

        SFX_Comprar.Play();
        StartCoroutine(VisualPurchase("You got: " + chosenWeapon));

        if (inventory.CheckFull()) return;

        Inventory.itemIndex ammo = Inventory.GetAmmoFor(chosenWeapon);
        int ammoAmount = Inventory.GetLimit(ammo)/2;
        inventory.AddItem(new Inventory.Item(Inventory.itemType.Ammo, ammo, amount: ammoAmount));
    }

    public override void PlayerStay()
    {
        if (inventory.CheckFull())
        {
            fullString = "Your inventory is full";
            textoInteraccion.text = fullString;
        }
        else
        {
            fullString = "Random weapon machine (" + PuntosRequeridos + ") (E)";
            textoInteraccion.text = fullString;
        }
    }
}

