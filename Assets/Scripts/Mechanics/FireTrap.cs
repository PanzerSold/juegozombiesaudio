using System.Collections;
using UnityEngine;

public class FireTrap : Interaction
{
    protected override int PuntosRequeridos { get => base.PuntosRequeridos; set => base.PuntosRequeridos = value; }
    [SerializeField] private float tiempoDeEspera = 10f; // AVISO, SI LO CAMBIAS, CAMBIAR DESPUES LOS PARTICLE SYSTEM
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
                textoInteraccion.gameObject.SetActive(true);

                if (pi.interactAction.IsPressed() && !ph.IsDying && !playerAmmo.InventoryOpen) 
                {
                    Interactuar();
                }
            }
            else
            {
                textoInteraccion.gameObject.SetActive(false);
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
        }
        else
        {
            StartCoroutine(VisualPurchase("Not enough money"));
            SFX_FaltaPlata.Play();
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
