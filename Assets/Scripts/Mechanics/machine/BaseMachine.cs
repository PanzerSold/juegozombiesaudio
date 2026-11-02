public abstract class BaseMachine : Interaction
{
    protected Inventory inventory;
    protected override int PuntosRequeridos { get => base.PuntosRequeridos; set => base.PuntosRequeridos = value; }

    protected abstract void Buy();

    protected virtual int CostoActual => PuntosRequeridos;
    protected bool compraFallida = false;

    protected override void Start()
    {
        base.Start();
        if (player != null)
        {
            inventory = player.GetComponent<Inventory>();
        }

    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Interactuar()
    {
        int costo = CostoActual;
        if (player.points >= costo)
        {
            compraFallida = false;
            Buy();
            if (!compraFallida) player.RemovePoints(costo);
        }
        else
        {
            SFX_FaltaPlata.Play();
            StartCoroutine(VisualPurchase("Not enough money"));
        }
    }
}
