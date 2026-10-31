using UnityEngine;

public class WeaponMachine : BaseMachine
{
    [SerializeField] private Inventory.itemIndex givenWeapon;

    //----- NUEVO CODIGO ----- Balas = mitad, arma nueva = precio completo, inventario lleno sin esas balas = 0
    protected override int CostoActual
    {
        get
        {
            if (inventory == null) return PuntosRequeridos;
            if (inventory.CheckFull())
            {
                bool tieneBalas = inventory.HasItem(Inventory.itemType.Ammo, Inventory.GetAmmoFor(givenWeapon));
                return tieneBalas ? PuntosRequeridos / 2 : 0;
            }
            return inventory.GetAmount(givenWeapon) > 0 ? PuntosRequeridos / 2 : PuntosRequeridos;
        }
    }
    //----- NUEVO CODIGO -----

    protected override void Buy()
    {
        if (inventory.CheckFull())
        {
            if (inventory.HasItem(Inventory.itemType.Ammo, Inventory.GetAmmoFor(givenWeapon)))
            {
                if (inventory.FindAndFillFirst(Inventory.GetAmmoFor(givenWeapon)))
                {
                    SFX_Comprar.Play();
                }
                else
                {
                    compraFallida = true;
                    StartCoroutine(VisualPurchase("Inventory is full"));
                }
            }
            else
            {
                compraFallida = true;
                StartCoroutine(VisualPurchase("Inventory is full"));
            }
            return;
        }
        if (inventory.GetAmount(givenWeapon) > 0)
        {
            StartCoroutine(VisualPurchase("Bought ammo"));
            EntregarBalas();
        }
    
        else
        {
            EntregarArma();
            StartCoroutine(VisualPurchase("You got: " + givenWeapon));
        }
        SFX_Comprar.Play();
    }

    public override void PlayerStay()
    {
        if (inventory.CheckFull())
        {
            fullString = "Your inventory is full";
            textoInteraccion.text = fullString;
            return;
        }
        if (inventory.GetAmount(givenWeapon) > 0)
        {
            fullString = "Buy ammo for " + givenWeapon + " (" + PuntosRequeridos / 2 + ") (E)";
            textoInteraccion.text = fullString;
        }
        else
        {
            fullString = "Buy " + givenWeapon + " (" + PuntosRequeridos + ") (E)";
            textoInteraccion.text = fullString;
        }
    }

    private void EntregarArma()
    {
        Inventory.Item nuevaArma = Inventory.CreateItem(givenWeapon);
        inventory.AddItem(nuevaArma);
        Inventory.itemIndex tipoDeBalas = Inventory.GetAmmoFor(givenWeapon);
        Inventory.Item nuevasBalas = Inventory.CreateItem(tipoDeBalas, Inventory.GetLimit(tipoDeBalas) / 2);
        inventory.AddItem(nuevasBalas);
    }
    private void EntregarBalas()
    {
        Inventory.itemIndex tipoDeBalas = Inventory.GetAmmoFor(givenWeapon);
        Inventory.Item nuevasBalas = Inventory.CreateItem(tipoDeBalas, Inventory.GetLimit(tipoDeBalas) / 2);
        inventory.AddItem(nuevasBalas);
    }
}