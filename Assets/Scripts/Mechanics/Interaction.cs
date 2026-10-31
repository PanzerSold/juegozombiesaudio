using System;
using TMPro;
using UnityEngine;
using System.Collections;

public abstract class Interaction : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] protected int puntosRequeridos;
    [SerializeField] protected bool usoUnico = false;
    protected PlayerInputs pi;
    protected bool jugadorDentro = false;
    protected Player player;
    [SerializeField] protected TextMeshProUGUI textoInteraccion;
    [SerializeField] protected TextMeshProUGUI textPurchase;
    [SerializeField] protected AudioSource SFX_FaltaPlata;
    [SerializeField] protected AudioSource SFX_Comprar;

    protected String fullString = "";

    protected PlayerHealth ph;
    protected PlayerShoot playerAmmo;


    protected virtual void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        ph = GameObject.FindWithTag("Player").GetComponent<PlayerHealth>();
        playerAmmo = GameObject.FindWithTag("Player").GetComponent<PlayerShoot>();
        pi = player.gameObject.GetComponent<PlayerInputs>();
    }

    protected virtual void Update()
    {
        if (jugadorDentro)
        {
            if (pi.interactAction.WasPressedThisFrame() && !ph.IsDying && !playerAmmo.InventoryOpen) Interactuar();
        }
    }

    protected abstract void Interactuar();

    protected virtual int PuntosRequeridos
    {
        get
        {
            if (player.HasPowerUp(Player.PlayerBonus.Discount)) return puntosRequeridos / 2;
            return puntosRequeridos;
        }

        set
        {
            puntosRequeridos = value;
        }
    }

    protected virtual void UpdatePriceText()
    {
        textoInteraccion.text = fullString.Replace(puntosRequeridos.ToString(), PuntosRequeridos.ToString());
    }
    public virtual void PlayerEntered()
    {
        jugadorDentro = true;
        UpdatePriceText();
        textoInteraccion.gameObject.SetActive(true);
    }

    public virtual void PlayerStay()
    {

    }
    public virtual void PlayerExit()
    {
        jugadorDentro = false;
        textoInteraccion.gameObject.SetActive(false);
    }

    protected IEnumerator VisualPurchase(string purchase)
    {
        textPurchase.text = "" + purchase;
        textPurchase.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        textPurchase.gameObject.SetActive(false);
    }
}
