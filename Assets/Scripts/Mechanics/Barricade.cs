using System.Collections;
using UnityEngine;

public class Barricade : Interaction
{
    [Header("Arrastra las barricadas en orden de aparición:")]
    [SerializeField] private GameObject[] barricades;
    [SerializeField] private Collider ColliderPrincipal;
    [SerializeField] private float delayBarricada = 3f;

    [SerializeField] private ParticleSystem woodParticle;
    public bool isOpen { get; private set; } = false;
    private bool cycle = false;

    [SerializeField] private AudioSource SFX_GettingHit;
    [SerializeField] private AudioSource SFX_GettingDestroyed;


    private int barricadasRestantes;
    
    public int zombiesClose = 0;

    private Animator animZombie;
    protected override void Start()
    {
        base.Start();
        fullString = "Repair barricade (E)";

        barricadasRestantes = 0;
        for (int i = 0; i < barricades.Length; i++)
        {
            if (barricades[i] != null && barricades[i].activeSelf) barricadasRestantes++;
        }
    }
    protected override void Update()
    {
        if (jugadorDentro)
        {
            if (barricadasRestantes < barricades.Length)
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

        if (zombiesClose > 0)
        {
            SacarBarricada();
        }
    }
    protected override void Interactuar()
    {
        if (!cycle && barricadasRestantes < barricades.Length)
        {
            player.AwardPoints(10);

            StartCoroutine(BarricadeCorrutine());
        }
    }

    private void SacarBarricada()
    {
        if (!cycle && barricadasRestantes > 0)
        {

            StartCoroutine(ZombieCorrutine());
        }
    }

    IEnumerator BarricadeCorrutine()
    {
        cycle = true;
        player.PlayBarrierSound();
        for (int i = 0; i < barricades.Length; i++)
        {
            if (barricades[i] != null && !barricades[i].activeSelf)
            {
                barricades[i].SetActive(true);
                barricadasRestantes++;
                break;
            }
        }

        ColliderPrincipal.enabled = true;
        isOpen = false;

        yield return new WaitForSeconds(delayBarricada);
        cycle = false;
    }

    IEnumerator ZombieCorrutine()
    {
        cycle = true;
        for (int i = barricades.Length - 1; i >= 0; i--)
        {
            if (barricades[i] != null && barricades[i].activeSelf)
            {
                if(woodParticle != null) woodParticle.Play();
                barricades[i].SetActive(false);
                SFX_GettingHit.Play();
                barricadasRestantes--;
                break;
            }
        }
        if (barricadasRestantes <= 0 && ColliderPrincipal != null)
        {
            ColliderPrincipal.enabled = false;
            SFX_GettingDestroyed.Play();
            isOpen = true;
        }
        if (animZombie != null)
        {
            animZombie.Play("attack");
        }
        yield return new WaitForSeconds(delayBarricada / 2f);


        cycle = false;
    }

    public void GetZombieAnim(Animator zombie)
    {

        animZombie = zombie;

    }

    override public void PlayerEntered()
    {
        jugadorDentro = true;
        if (barricadasRestantes < barricades.Length)
        {
            textoInteraccion.text = fullString;
            textoInteraccion.gameObject.SetActive(true);
        }
    }

    override public void PlayerStay()
    {
        if (barricadasRestantes < barricades.Length)
        {
            textoInteraccion.text = fullString;
            textoInteraccion.gameObject.SetActive(true);
        } else
        {
            textoInteraccion.gameObject.SetActive(false);
        }
    }
}