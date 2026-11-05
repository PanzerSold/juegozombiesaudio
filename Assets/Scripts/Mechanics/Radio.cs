using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio : Interaction
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource estaticaSource;
    [SerializeField] private AudioSource reporteroSource;
    [SerializeField] private AudioSource publicidadSource;
    [SerializeField] private AudioSource presentadorSource;
    [SerializeField] private AudioSource musicaSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip estatica;
    [SerializeField] private AudioClip reportero;
    [SerializeField] private AudioClip publicidad;
    [SerializeField] private AudioClip presentador;
    [SerializeField] private AudioClip musica;

    private bool radioEncendida = true;
    private bool reproduciendoClip = false;

    private bool reporteroReproducido = false;
    private bool publicidadReproducida = false;
    private bool presentadorReproducido = false;

    private bool musicaPermanente = false;

    private int clipsReproducidos = 0;

    private Coroutine reproduccionActual;

    protected override void Start()
    {
        base.Start();

        // Configuración inicial de los AudioSources.
        ConfigurarAudioSources();

        // La música debe continuar durante la pausa.
        musicaSource.ignoreListenerPause = true;

        // Comenzar con la estática.
        EstablecerSonidoPredeterminado();

        ActualizarTexto();
    }

    private void ConfigurarAudioSources()
    {
        AudioSource[] fuentes =
        {
            estaticaSource,
            reporteroSource,
            publicidadSource,
            presentadorSource,
            musicaSource
        };

        foreach (AudioSource fuente in fuentes)
        {
            fuente.mute = false;
            fuente.playOnAwake = false;
            fuente.loop = false;
            fuente.Stop();
        }

        // Prioridades de reproducción.
        estaticaSource.priority = 180;

        reporteroSource.priority = 0;
        publicidadSource.priority = 0;
        presentadorSource.priority = 0;

        musicaSource.priority = 128;
    }

    protected override void Update()
    {
        // R tiene prioridad sobre la recarga.
        if (jugadorDentro && pi.reloadAction.WasPressedThisFrame())
        {
            AlternarRadio();
        }

        base.Update();
    }

    protected override void Interactuar()
    {
        if (!radioEncendida || reproduciendoClip || musicaPermanente)
            return;

        // Todavía quedan clips por reproducir.
        if (clipsReproducidos < 3)
        {
            if (player.points < PuntosRequeridos)
                return;

            player.RemovePoints(PuntosRequeridos);

            SintonizarClip();
            return;
        }

        // Los tres clips ya terminaron.
        // Si el presentador apareció antes, E activa la música permanente.
        if (presentadorReproducido)
        {
            if (player.points < PuntosRequeridos)
                return;

            player.RemovePoints(PuntosRequeridos);

            ActivarMusicaPermanente();
        }
    }

    private void SintonizarClip()
    {
        List<int> clipsDisponibles = new List<int>();

        if (!reporteroReproducido)
            clipsDisponibles.Add(0);

        if (!publicidadReproducida)
            clipsDisponibles.Add(1);

        if (!presentadorReproducido)
            clipsDisponibles.Add(2);

        if (clipsDisponibles.Count == 0)
            return;

        int elegido = clipsDisponibles[
            Random.Range(0, clipsDisponibles.Count)
        ];

        AudioClip clipElegido;
        AudioSource fuenteElegida;

        bool esPresentador = false;

        switch (elegido)
        {
            case 0:
                reporteroReproducido = true;
                clipElegido = reportero;
                fuenteElegida = reporteroSource;
                break;

            case 1:
                publicidadReproducida = true;
                clipElegido = publicidad;
                fuenteElegida = publicidadSource;
                break;

            default:
                presentadorReproducido = true;
                clipElegido = presentador;
                fuenteElegida = presentadorSource;
                esPresentador = true;
                break;
        }

        clipsReproducidos++;

        reproduccionActual = StartCoroutine(
            ReproducirClip(clipElegido, fuenteElegida, esPresentador)
        );
    }

    private IEnumerator ReproducirClip(AudioClip clip, AudioSource fuente, bool esPresentador)
    {
        reproduciendoClip = true;

        // Detener la estática y cualquier otro audio de la radio.
        DetenerTodosLosAudios();

        fuente.clip = clip;
        fuente.loop = false;
        fuente.Play();

        ActualizarTexto();

        // Esperar a que termine el audio principal.
        // Esta espera utiliza el reloj de audio para respetar la pausa del juego.
        double tiempoFinal = AudioSettings.dspTime +
            clip.length / Mathf.Max(0.01f, fuente.pitch);

        while (AudioSettings.dspTime < tiempoFinal)
        {
            yield return null;
        }

        fuente.Stop();

        // Si el presentador fue el último clip, activar música permanente.
        if (esPresentador && clipsReproducidos == 3)
        {
            reproduciendoClip = false;
            reproduccionActual = null;

            ActivarMusicaPermanente();
            yield break;
        }

        // Si el presentador apareció antes del último clip,
        // reproducir la música una sola vez.
        if (esPresentador && clipsReproducidos < 3)
        {
            musicaSource.clip = musica;
            musicaSource.loop = false;
            musicaSource.Play();

            // El tiempo sigue avanzando incluso durante la pausa.
            yield return new WaitForSecondsRealtime(
                musica.length / Mathf.Max(0.01f, musicaSource.pitch)
            );

            musicaSource.Stop();
        }

        reproduciendoClip = false;
        reproduccionActual = null;

        // Volver a la estática.
        EstablecerSonidoPredeterminado();

        ActualizarTexto();
    }
    private void DetenerTodosLosAudios()
    {
        estaticaSource.Stop();
        reporteroSource.Stop();
        publicidadSource.Stop();
        presentadorSource.Stop();
        musicaSource.Stop();
    }

    private void EstablecerSonidoPredeterminado()
    {
        DetenerTodosLosAudios();

        if (musicaPermanente)
        {
            musicaSource.clip = musica;
            musicaSource.loop = true;
            musicaSource.Play();
        }
        else
        {
            estaticaSource.clip = estatica;
            estaticaSource.loop = true;
            estaticaSource.Play();
        }
    }

    private void ActivarMusicaPermanente()
    {
        if (musicaPermanente)
            return;

        musicaPermanente = true;

        EstablecerSonidoPredeterminado();

        ActualizarTexto();
    }

    private void AlternarRadio()
    {
        radioEncendida = !radioEncendida;

        // Silenciar todos los canales sin interrumpir su reproducción.
        estaticaSource.mute = !radioEncendida;
        reporteroSource.mute = !radioEncendida;
        publicidadSource.mute = !radioEncendida;
        presentadorSource.mute = !radioEncendida;
        musicaSource.mute = !radioEncendida;

        ActualizarTexto();
    }

    private void ActualizarTexto()
    {
        if (!radioEncendida)
        {
            fullString = "Prender radio (R)";
        }
        else if (musicaPermanente)
        {
            fullString = "Música permanente (R apagar)";
        }
        else if (reproduciendoClip)
        {
            fullString = "Radio en reproducción (R apagar)";
        }
        else if (clipsReproducidos == 3)
        {
            fullString = "Sintonizar música (E) / Apagar radio (R)";
        }
        else
        {
            fullString = "Sintonizar radio (E) / Apagar radio (R)";
        }

        if (jugadorDentro && textoInteraccion != null)
            textoInteraccion.text = fullString;
    }

    public override void PlayerEntered()
    {
        base.PlayerEntered();

        pi.blockReload = true;

        ActualizarTexto();
    }

    public override void PlayerExit()
    {
        base.PlayerExit();

        pi.blockReload = false;
    }
}