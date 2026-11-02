public class Door : Interaction
{
    protected override int PuntosRequeridos { get => base.PuntosRequeridos; set => base.PuntosRequeridos = value; }
    protected override void Start()
    {
        base.Start();
        fullString = "Open door (" + PuntosRequeridos + ") (E)";
    }

    protected override void Interactuar()
    {
        if (player.points >= PuntosRequeridos)
        {
            SFX_Comprar.Play();
            player.RemovePoints(PuntosRequeridos);
            textoInteraccion.gameObject.SetActive(false);
            FindAnyObjectByType<RoundManager>().IncreaseMapIndex();
            Destroy(gameObject);
        }
        else
        {
            SFX_FaltaPlata.Play();
            StartCoroutine(VisualPurchase("Not enough money"));
        }
    }
}