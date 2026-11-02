using System.Collections;
using UnityEngine;

public class Radio : Interaction
{
    [Header("Audio")]
    [SerializeField] private AudioClip primerClip;
    [SerializeField] private AudioClip segundoClip;

    private AudioSource radioSFX;

    private bool reproduciendo = false;
    private bool primerAudioReproducido = false;
    private bool radioTerminada = false;

    protected override void Start()
    {
        base.Start();

        radioSFX = GetComponent<AudioSource>();
        fullString = "Prender radio (E)";
    }

    protected override void Interactuar()
    {
        if (reproduciendo || radioTerminada)
            return;

        if (player.points >= PuntosRequeridos)
        {
            player.RemovePoints(PuntosRequeridos);

            AudioClip clipActual;

            if (!primerAudioReproducido)
            {
                clipActual = primerClip;
                primerAudioReproducido = true;
            }
            else
            {
                clipActual = segundoClip;
                radioTerminada = true;
            }

            StartCoroutine(ReproducirAudio(clipActual));
        }
    }

    private IEnumerator ReproducirAudio(AudioClip clip)
    {
        reproduciendo = true;

        textoInteraccion.gameObject.SetActive(false);
        jugadorDentro = false;

        radioSFX.PlayOneShot(clip);

        yield return new WaitForSeconds(clip.length);

        reproduciendo = false;
    }

    public override void PlayerEntered()
    {
        if (reproduciendo || primerAudioReproducido || radioTerminada)
            return;

        base.PlayerEntered();
    }
}