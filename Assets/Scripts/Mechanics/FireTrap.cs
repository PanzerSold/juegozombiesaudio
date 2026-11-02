using System.Collections;
using UnityEngine;

public class FireTrap : Interaction
{
    protected override int PuntosRequeridos { get => base.PuntosRequeridos; set => base.PuntosRequeridos = value; }
    [SerializeField] private float tiempoDeEspera = 10f;
    [SerializeField] private Transform FuegosParent;
    [SerializeField] private BoxCollider FuegoTrigger;
    [SerializeField] private ParticleSystem[] losfueguitos;
    [SerializeField] protected AudioSource SFX_Fuego;
    private bool isFireActive = false;

    protected override void Start()
    {
        base.Start();
        fullString = "Activate trap (" + PuntosRequeridos + ") (E)";
    }

    protected override void Update()
    {
        if(jugadorDentro)
        {
            if (!isFireActive)
            {
                fullString = "Activate trap (" + PuntosRequeridos + ") (E)";
                textoInteraccion.text = fullString;
                textoInteraccion.gameObject.SetActive(true);

                if (pi.interactAction.IsPressed() && !ph.IsDying && !playerAmmo.InventoryOpen) 
                {
                    Interactuar();
                }
                return;
            }
            else
            {
                fullString = "Wait a while";
                textoInteraccion.text = fullString;
                textoInteraccion.gameObject.SetActive(true);

                if (pi.interactAction.IsPressed() && !ph.IsDying && !playerAmmo.InventoryOpen)
                {
                    Interactuar();
                }
            }
        }
    }

    protected override void Interactuar()
    {
        if (!isFireActive && player.points >= PuntosRequeridos)
        {
            StartCoroutine(ActivarFuego());
            SFX_Comprar.Play();
            SFX_Fuego.Play();
            return;
        }
        else if (isFireActive)
        {
            StartCoroutine(VisualPurchase("The trap is already active"));
            SFX_FaltaPlata.Play();
        }
        else
        {
            StartCoroutine(VisualPurchase("Not enough money"));
            SFX_FaltaPlata.Play();
        }
    }

    public override void PlayerStay()
    {
        if (isFireActive)
        {
            fullString = "Wait a while";
            textoInteraccion.text = fullString;
            textoInteraccion.gameObject.SetActive(true);
        }
        else
        {
            fullString = "Activate trap (" + PuntosRequeridos + ") (E)";
            textoInteraccion.text = fullString;
            textoInteraccion.gameObject.SetActive(true);
        }
    }

    private IEnumerator ActivarFuego()
    {
        isFireActive = true;
        FuegoTrigger.gameObject.SetActive(true);
        foreach (var i in losfueguitos)
        {
            i.Play();
        }
        player.RemovePoints(PuntosRequeridos);
        yield return new WaitForSeconds(tiempoDeEspera);
        FuegoTrigger.gameObject.SetActive(false);
        isFireActive = false;
    }
}
