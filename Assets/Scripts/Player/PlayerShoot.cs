using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.Analytics.IAnalytic;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    [SerializeField] private TextMeshProUGUI playerAmmoText;

    [SerializeField] private ParticleSystem shotParticle;
    [SerializeField] private GameObject inventoryPanel;


    [Header("Sonido")]
    [SerializeField] protected AudioSource shootSfx;
    [SerializeField] private AudioSource emptyMagSfx;
    [SerializeField] private AudioSource reloadSfx;

    private PlayerInputs pi;
    private Inventory inventory;

    private GunBehaviour gun;

    public GunBehaviour Gun => gun;

    private readonly Dictionary<Inventory.itemIndex, GunBehaviour> guns = new Dictionary<Inventory.itemIndex, GunBehaviour>();

    private float shootTimer = 0;

    private bool inventoryOpen = false;

    public bool InventoryOpen => inventoryOpen;
    
    //----- NUEVO CODIGO ----- Índice del arma que sostenías antes de la actual
    private Inventory.itemIndex lastWeapon = Inventory.itemIndex.None;

    private PlayerHealth ph;
    private Animator anim;

    //----- NUEVO CODIGO -----
    private void Awake()
    {
        inventory = GetComponent<Inventory>();

        foreach (GunBehaviour g in GetComponentsInChildren<GunBehaviour>())
        {
            if (g.WeaponIndex == Inventory.itemIndex.None)
            {
                Debug.LogWarning("El arma '" + g.name + "' no tiene Weapon Index asignado.", g);
                continue;
            }
            if (!guns.TryAdd(g.WeaponIndex, g))
            {
                Debug.LogWarning("Dos armas comparten el Weapon Index " + g.WeaponIndex + " ('" + g.name + "').", g);
            }
        }
    }

    private void Start()
    {
        ph = GetComponent<PlayerHealth>();
        anim = GetComponentInChildren<Animator>();
        pi = gameObject.GetComponent<PlayerInputs>();
        EquipWeapon(inventory.EquippedWeapon);
    }


    private void EquipWeapon(Inventory.itemIndex weapon)
    {
        if (!guns.TryGetValue(weapon, out GunBehaviour newGun))
        {
            Debug.LogWarning("No hay ningun arma en el Player con Weapon Index " + weapon);
            return;
        }
        EquipGun(newGun);
    }

    public void EquipGun(GunBehaviour newGun)
    {
        if (newGun == gun) return;
        //----- NUEVO CODIGO ----- Antes de soltar el arma actual, guarda cuál era
        if (gun != null) lastWeapon = gun.WeaponIndex;
        //----- NUEVO CODIGO -----
        if (gun != null) gun.Unequip();

        gun = newGun;
        gun.Initialize(playerCamera, playerAmmoText, shotParticle, shootSfx, reloadSfx, inventory, anim);
        gun.Render.enabled = true;
        float idArma = gun.AnimIndex;
        anim.SetFloat("Weapon", idArma);
        shootTimer = 0;
    }
    //----- NUEVO CODIGO ----- Cambia al arma anterior si existe en la escena y todavía está en el inventario
    private void EquipLastWeapon()
    {
        if (!guns.ContainsKey(lastWeapon)) return;
        if (!inventory.TrySelectWeapon(lastWeapon)) return;
        EquipWeapon(lastWeapon);
    }
    //----- NUEVO CODIGO -----

    public void EquipFromSlot(int slot)
    {
        if (inventory.TrySelectWeaponSlot(slot, out Inventory.itemIndex weapon))
        {
            EquipWeapon(weapon);
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        inventoryOpen = !inventoryOpen;
        if (inventoryPanel != null) inventoryPanel.SetActive(inventoryOpen);

        Cursor.lockState = inventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = inventoryOpen;

        if (inventoryOpen && gun != null) gun.CancelReload();
    }

    private void ThrowGrenade()
    {
        if (inventory.TakeAmount(Inventory.itemIndex.Granada, 1) == 0) return;
    }


    private void Update()
    {
        if (pi.inventoryAction.WasPressedThisFrame() && !ph.IsDying)
        {
            ToggleInventory();
        }

        if (inventoryOpen) return;
        //----- NUEVO CODIGO ----- Al apretar Q (LastWeapon) vuelve al arma anterior
        if (pi.lastWeaponAction.WasPressedThisFrame() && !ph.IsDying)
        {
            EquipLastWeapon();
        }
        //----- NUEVO CODIGO -----
        if (gun != null && !ph.IsDying)
         {
            bool inputShoot;
            shootTimer += Time.deltaTime;
            if (gun.IsAutomatic)
            {
                inputShoot = pi.shootAction.IsPressed();
            } else
            {
                inputShoot = pi.shootAction.WasPressedThisFrame();
            }

            if (!gun.IsRecharging)
            {
                if (shootTimer >= gun.ShootTime)
                {
                    if (inputShoot)
                    {
                        if (gun.AmmoCurrent > 0)
                        {
                            gun.Shoot();
                        }
                        else if (!emptyMagSfx.isPlaying)
                        {
                            emptyMagSfx.Play();
                        }
                        shootTimer = 0;
                    }
                }

                if (gun.AmmoCurrent != gun.AmmoMax && gun.AmmoQuantity > 0)
                {
                    if (pi.reloadAction.WasPressedThisFrame())
                    {
                        gun.RechargeAmmo();
                    }

                }
            }
        }
    }
}
