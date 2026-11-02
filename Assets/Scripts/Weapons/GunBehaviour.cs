using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
public class GunBehaviour : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] protected float raycastDistance = 100f;
    [SerializeField] protected float gunDamage = 1f;
    protected Camera cameraPos;
    [SerializeField] protected LayerMask coll;
    [SerializeField] protected AudioClip shooteExplosiveClip;
    protected ParticleSystem ps;

    protected ParticleSystem[] psExplosiveAmmo = new ParticleSystem[3];
    [SerializeField] private float shootTime = 0.5f;
    protected ParticleSystemRenderer psR;

    [Header("Inventario")]
    [SerializeField] private Inventory.itemIndex weaponIndex = Inventory.itemIndex.None;
    [SerializeField] private PlayerShoot shootScript;

    [Header("Municion")]
    [SerializeField] private int ammoMax = 10;
    [SerializeField] private int ammoQuantity = 50;

    [SerializeField] private float delayRecharge = 3f;

    [Header("Materiales")]
    [SerializeField] protected Material materialGenerico;
    [SerializeField] protected Material materialZombieHit;

    [Header("Sonido")]
    [SerializeField] protected AudioClip shootClip;
    [SerializeField] private AudioClip reloadClip;

    [SerializeField] protected float minPitchShift = 0.85f;
    [SerializeField] protected float maxPitchShift = 1.15f;

    [Header("Animacion")]

    [SerializeField] protected float animIndex;

    public float AnimIndex => animIndex;
    protected AudioSource shootSfx;
    private AudioSource reloadSfx;

    private TextMeshProUGUI ammoText;

    private Inventory inventory;

    private string rechargeText = "Reloading...";

    [Header("Automatica")]

    [SerializeField] private bool isAutomatic = false;

    public Inventory.itemIndex WeaponIndex => weaponIndex;
    protected Inventory.itemIndex AmmoType => Inventory.GetAmmoFor(weaponIndex);

    [Header("Balas explosivas")]

    [SerializeField] protected float boomRadius = 0.5f;

    public bool IsAutomatic => isAutomatic;

    public int AmmoCurrent => ammoCurrent;

    public float ShootTime => shootTime;
    public int AmmoMax => ammoMax;

    private int ammoUsed;
    private int ammoCurrent;

    private bool isRecharging;

    public bool IsRecharging => isRecharging;

    public TextMeshProUGUI AmmoText => ammoText;

    public int AmmoQuantity => inventory != null ? inventory.GetAmount(AmmoType) : 0;

    private Coroutine rechargeCoroutine;
    protected Player player;

    private Animator anim;

    private SkinnedMeshRenderer render;

    public SkinnedMeshRenderer Render => render;
    private void Awake()
    {
        ammoCurrent = ammoMax;
        render = GetComponent<SkinnedMeshRenderer>();
    }

    public void Initialize(Camera playerCamera, TextMeshProUGUI playerAmmoText, ParticleSystem shotParticle, AudioSource shootSource, AudioSource reloadSource, Inventory playerInventory, Animator playerAnim)
    {
        cameraPos = playerCamera;
        ammoText = playerAmmoText;
        ps = shotParticle;
        shootSfx = shootSource;
        reloadSfx = reloadSource;
        anim = playerAnim;
        player = GetComponentInParent<Player>();
        if (inventory != null) inventory.OnInventoryChanged -= RefreshAmmoText;
        inventory = playerInventory;
        inventory.OnInventoryChanged += RefreshAmmoText;

        if (ps != null)
        {
            psR = ps.GetComponent<ParticleSystemRenderer>();
        }

        if (ammoText != null)
        {
            ammoText.text = ammoCurrent + " / " + ammoQuantity;
        }
        Transform explosionBulletGO = player.transform.Find("ExplosiveAmmo").transform;
        for(int i = 0; i < explosionBulletGO.childCount; i++)
        {
            psExplosiveAmmo[i] = explosionBulletGO.GetChild(i).gameObject.GetComponent<ParticleSystem>();
        }

        RefreshAmmoText();

    }

    public virtual void Unequip()
    {
        StopAllCoroutines();
        CancelReload();
        render.enabled = false;
        if (inventory != null) inventory.OnInventoryChanged -= RefreshAmmoText;
    }

    public void CancelReload()
    {
        if (rechargeCoroutine != null) StopCoroutine(rechargeCoroutine);
        rechargeCoroutine = null;
        isRecharging = false;
        RefreshAmmoText();
    }

    private void RefreshAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = isRecharging ? rechargeText : ammoCurrent + " / " + AmmoQuantity;
    }


    virtual public void Shoot()
    {
        AudioClip clipToPlay = shootClip;
        RaycastHit hit;
        Vector3 origin = cameraPos.transform.position;
        Vector3 direction = cameraPos.transform.forward;
        Material impactMaterial = materialGenerico;
        shootSfx.pitch = Random.Range(minPitchShift, maxPitchShift);
        
        FireAmmo();
        if (Physics.Raycast(origin, direction, out hit, raycastDistance, coll))
        {
            ps.transform.position = hit.point;

            if (player.activePerks[7])
            {
                clipToPlay = shooteExplosiveClip;
                float explosiveDamage = gunDamage * 2;
                Collider[] colliders = Physics.OverlapSphere(hit.point, boomRadius);
                List<ZombieBase> currentZombie = new List<ZombieBase>();

                foreach (Collider hitCollider in colliders)
                {
                    ZombieBase zombie = hitCollider.gameObject.GetComponentInParent<ZombieBase>();
                    if (zombie != null) 
                    {
                        if (currentZombie.Contains(zombie)) continue;
                        currentZombie.Add(zombie);
                        zombie.ReceiveDamage(false, explosiveDamage);
                    }
                }
            }
            
            if (hit.collider.CompareTag("ZombieBody"))
            {

                    ZombieBase zombie = hit.collider.GetComponentInParent<ZombieBase>();
                    zombie.ReceiveDamage(false, gunDamage);
                    impactMaterial = materialZombieHit;
            }

            if (hit.collider.CompareTag("ZombieHead"))
            {

                    ZombieBase zombie = hit.collider.GetComponentInParent<ZombieBase>();

                    zombie.ReceiveDamage(true, gunDamage);
                    impactMaterial = materialZombieHit;

            }

            if (player.activePerks[7])
            {
                foreach(ParticleSystem pse in psExplosiveAmmo)
                {
                    pse.transform.position = hit.point;
                    pse.Play();
                }
            }
            else{
                psR.material = impactMaterial;
                ps.Play();            
            }
            
        }
        shootSfx.PlayOneShot(clipToPlay);
    }


    protected void FireAmmo()
    {
        ammoCurrent--;
        if (ammoCurrent < 0)
        {
            ammoCurrent = 0;
        }

        RefreshAmmoText();

    }
    public void RechargeAmmo()
    {
        if(rechargeCoroutine != null)
        {
            StopCoroutine(rechargeCoroutine);
        }
        rechargeCoroutine = StartCoroutine(Recharge());
        
    }

    public void FillMagazine()
    {
        CancelReload();
        ammoCurrent = ammoMax;
        RefreshAmmoText();
    }

    public void GainAmmo(int gainedAmmo)
    {
        inventory.AddItem(Inventory.CreateItem(AmmoType, gainedAmmo));
        RefreshAmmoText();
    }

    private IEnumerator Recharge()
    {
        isRecharging = true;
        reloadSfx.PlayOneShot(reloadClip);
        anim.SetTrigger("Reload");
        RefreshAmmoText();

        if (player.activePerks[2]) yield return new WaitForSeconds(delayRecharge / 2);
        else yield return new WaitForSeconds(delayRecharge);

        isRecharging = false;
        rechargeCoroutine = null;
        int needed = ammoMax - ammoCurrent;
        ammoCurrent += inventory.TakeAmount(AmmoType, needed);

        RefreshAmmoText();
    }
}
