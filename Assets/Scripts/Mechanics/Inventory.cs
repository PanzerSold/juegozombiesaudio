using System;
using UnityEngine;
public class Inventory : MonoBehaviour
{
    public enum itemType
    {
        Empty,
        Weapon,
        Ammo,
        Consumable
    }
    public enum itemIndex
    {
        None,
        M1911,
        M93R,
        M4,
        AK47,
        AWP,
        UZI,
        MP5,
        RPG,
        M800,
        Balas_Pistola,
        Balas_Rifle,
        Balas_Sniper,
        Balas_Subfusil,
        Balas_Escopeta,
        Balas_Lanzacohete,
        Granada,
        Espray_curativo
    }
    public struct LimitesInventario
    {
        public int Balas_Pistola;
        public int Balas_Rifle;
        public int Balas_Sniper;
        public int Balas_Subfusil;
        public int Balas_Escopeta;
        public int Balas_Lanzacohete;
        public int Granada;
        public int Espray_curativo;

        public static LimitesInventario Defecto => new LimitesInventario
        {
            Balas_Pistola = 48,
            Balas_Rifle = 90,
            Balas_Sniper = 30,
            Balas_Subfusil = 120,
            Balas_Escopeta = 24,
            Balas_Lanzacohete = 6,
            Granada = 5,
            Espray_curativo = 3
        };
    }

    public struct Item
    {
        public itemType item;
        public itemIndex index;
        public bool isStackable;
        public int amount;
        public static Item empty => new Item(itemType.Empty, itemIndex.None);
        public static Item M1911 => new Item(itemType.Weapon, itemIndex.M1911, true);
        public static Item startingAmmo => new Item(itemType.Ammo, itemIndex.Balas_Pistola, false, LimitesInventario.Defecto.Balas_Pistola / 2);
        public Item(itemType item, itemIndex index, bool isWeapon = false, int amount = 1)
        {
            this.item = item;
            this.index = index;
            this.isStackable = !isWeapon;
            this.amount = amount;
        }
    }
    private Item[] actualInventory;

    public bool CheckFull()
    {
        foreach(Item i in actualInventory)
        {
            if (i.item == itemType.Empty) return false;
        }
        return true;
    }

    public itemIndex[] getAllWeapons()
    {
        itemIndex[] allWeapons = new itemIndex[9];
        for(int i = 0; i < SlotCount; i++)
        {
            if (actualInventory[i].item == itemType.Weapon) allWeapons[i] = actualInventory[i].index;
        }
        return allWeapons;
    }
    private const int SlotCount = 9;
    private int equippedSlot = 0;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        ResetInventory();
    }

    public static int GetLimit(itemIndex index)
    {
        LimitesInventario limites = LimitesInventario.Defecto;
        switch (index)
        {
            case itemIndex.Balas_Pistola: return limites.Balas_Pistola;
            case itemIndex.Balas_Rifle: return limites.Balas_Rifle;
            case itemIndex.Balas_Sniper: return limites.Balas_Sniper;
            case itemIndex.Balas_Subfusil: return limites.Balas_Subfusil;
            case itemIndex.Balas_Escopeta: return limites.Balas_Escopeta;
            case itemIndex.Balas_Lanzacohete: return limites.Balas_Lanzacohete;
            case itemIndex.Granada: return limites.Granada;
            case itemIndex.Espray_curativo: return limites.Espray_curativo;
            default: return 0;
        }
    }

    public static itemIndex GetAmmoFor(itemIndex weapon)
    {
        switch (weapon)
        {
            case itemIndex.M1911:
            case itemIndex.M93R:
                return itemIndex.Balas_Pistola;
            case itemIndex.M4:
            case itemIndex.AK47:
                return itemIndex.Balas_Rifle;
            case itemIndex.AWP:
                return itemIndex.Balas_Sniper;
            case itemIndex.UZI:
            case itemIndex.MP5:
                return itemIndex.Balas_Subfusil;
            case itemIndex.M800:
                return itemIndex.Balas_Escopeta;
            case itemIndex.RPG:
                return itemIndex.Balas_Lanzacohete;
            default:
                return itemIndex.None;
        }
    }

    public static Item CreateItem(itemIndex index, int amount = 1)
    {
        switch (index)
        {
            case itemIndex.None:
                return Item.empty;
            case itemIndex.Balas_Pistola:
            case itemIndex.Balas_Rifle:
            case itemIndex.Balas_Sniper:
            case itemIndex.Balas_Subfusil:
            case itemIndex.Balas_Escopeta:
            case itemIndex.Balas_Lanzacohete:
                return new Item(itemType.Ammo, index, false, amount);
            case itemIndex.Granada:
            case itemIndex.Espray_curativo:
                return new Item(itemType.Consumable, index, false, amount);
            default:
                return new Item(itemType.Weapon, index, true, 1);
        }
    }

    public bool HasItem(itemType type, itemIndex index)
    {
        for (int i = 0; i < actualInventory.Length; i++)
        {
            if (actualInventory[i].item == type && actualInventory[i].index == index) return true;
        }
        return false;
    }

    public int GetAmount(itemIndex index)
    {
        int total = 0;
        for (int i = 0; i < actualInventory.Length; i++)
        {
            if (actualInventory[i].item != itemType.Empty && actualInventory[i].index == index)
            {
                total += actualInventory[i].amount;
            }
        }
        return total;
    }

    public itemIndex EquippedWeapon
    {
        get
        {
            Item slot = actualInventory[equippedSlot];
            return slot.item == itemType.Weapon ? slot.index : itemIndex.None;
        }
    }

    public bool AddItem(Item newItem)
    {
        if (newItem.item == itemType.Empty || newItem.amount <= 0) return false;

        if (!newItem.isStackable)
        {
            if (HasItem(newItem.item, newItem.index) || !PlaceInEmptySlot(newItem)) return false;
            OnInventoryChanged?.Invoke();
            return true;
        }

        int limit = GetLimit(newItem.index);
        int remaining = newItem.amount;

        for (int i = 0; i < actualInventory.Length && remaining > 0; i++)
        {
            if (actualInventory[i].item == newItem.item && actualInventory[i].index == newItem.index)
            {
                int added = Mathf.Min(limit - actualInventory[i].amount, remaining);
                if (added > 0)
                {
                    actualInventory[i].amount += added;
                    remaining -= added;
                }
            }
        }

        while (remaining > 0)
        {
            Item chunk = newItem;
            chunk.amount = Mathf.Min(remaining, limit);
            if (!PlaceInEmptySlot(chunk)) break;
            remaining -= chunk.amount;
        }

        if (remaining == newItem.amount) return false;
        OnInventoryChanged?.Invoke();
        return true;
    }

    private bool PlaceInEmptySlot(Item item)
    {
        for (int i = 0; i < actualInventory.Length; i++)
        {
            if (actualInventory[i].item == itemType.Empty)
            {
                actualInventory[i] = item;
                return true;
            }
        }
        return false;
    }
    //----- NUEVO CODIGO ----- Busca el slot donde está esa arma y lo marca como equipado. Devuelve false si ya no está en el inventario
    public bool TrySelectWeapon(itemIndex weapon)
    {
        for (int i = 0; i < actualInventory.Length; i++)
        {
            if (actualInventory[i].item == itemType.Weapon && actualInventory[i].index == weapon)
            {
                equippedSlot = i;
                return true;
            }
        }
        return false;
    }
    //----- NUEVO CODIGO -----

    public int TakeAmount(itemIndex index, int requested)
    {
        int taken = 0;
        for (int i = 0; i < actualInventory.Length && taken < requested; i++)
        {
            if (actualInventory[i].item == itemType.Empty || actualInventory[i].index != index) continue;

            int fromSlot = Mathf.Min(requested - taken, actualInventory[i].amount);
            actualInventory[i].amount -= fromSlot;
            taken += fromSlot;
            if (actualInventory[i].amount <= 0) actualInventory[i] = Item.empty;
        }
        if (taken > 0) OnInventoryChanged?.Invoke();
        return taken;
    }

    public bool TrySelectWeaponSlot(int slot, out itemIndex weapon)
    {
        weapon = itemIndex.None;
        if (slot < 0 || slot >= actualInventory.Length || actualInventory[slot].item != itemType.Weapon) return false;

        equippedSlot = slot;
        weapon = actualInventory[slot].index;
        return true;
    }

    public Item GetSlot(int slot)
    {
        if (slot < 0 || slot >= actualInventory.Length) return Item.empty;
        return actualInventory[slot];
    }

    //----- NUEVO CODIGO ----- Devuelve false si el slot esta vacio o es el arma equipada. Si descarta, vacia el slot, devuelve lo que habia en discarded y avisa con OnInventoryChanged
    public bool DiscardSlot(int slot, out Item discarded)
    {
        discarded = Item.empty;
        if (slot < 0 || slot >= actualInventory.Length) return false;
        if (actualInventory[slot].item == itemType.Empty) return false;
        if (slot == equippedSlot) return false;

        discarded = actualInventory[slot];
        actualInventory[slot] = Item.empty;
        OnInventoryChanged?.Invoke();
        return true;
    }
    //----- NUEVO CODIGO ----- “Borrar” sobre el arma equipada no hace nada, porque DiscardSlot lo bloquea a propósito.

    private void ResetInventory()
    {
        equippedSlot = 0;
        actualInventory = new Item[SlotCount];
        actualInventory[0] = Item.M1911;
        actualInventory[1] = Item.startingAmmo;
        for (int i = 2; i < SlotCount; i++)
        {
            actualInventory[i] = Item.empty;
        }

       

        OnInventoryChanged?.Invoke();
    }


    public bool FindAndFillFirst(itemIndex ammo)
    {
        int limit = GetLimit(ammo);
        if (limit <= 0) return false;

        for (int i = 0; i < SlotCount; i++)
        {
            if (actualInventory[i].item == itemType.Ammo &&
                actualInventory[i].index == ammo)
            {
                actualInventory[i].amount = limit;
                OnInventoryChanged?.Invoke();
                return true;
            }
        }

        if (PlaceInEmptySlot(CreateItem(ammo, limit)))
        {
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
}