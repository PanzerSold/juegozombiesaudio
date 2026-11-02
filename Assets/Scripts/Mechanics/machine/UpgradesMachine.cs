using UnityEngine;
using System.Collections.Generic;

public class UpgradesMachine : BaseMachine
{
    protected override void Start()
    {
        base.Start();
        fullString = "Buy perk (" + PuntosRequeridos + ") (E)";
    }

    protected override void Buy()
    {
        List<int> missingPerks = new List<int>();

        for (int i = 0; i < player.activePerks.Length; i++)
        {
            if (!player.activePerks[i])
            {
                missingPerks.Add(i);
            }
        }

        if (missingPerks.Count > 0)
        {
            GivePerk(missingPerks);
        }
        else
        {
            player.AwardPoints(PuntosRequeridos);
            StartCoroutine(VisualPurchase("Every perk collected"));
            return;      
        }
        SFX_Comprar.Play();
    }

    public override void PlayerStay()
    {
        List<int> missingPerks = new List<int>();

        for (int i = 0; i < player.activePerks.Length; i++)
        {
            if (!player.activePerks[i])
            {
                missingPerks.Add(i);
            }
        }

        if (missingPerks.Count > 0)
        {
            fullString = "Buy perk (" + PuntosRequeridos + ") (E)";
            textoInteraccion.text = fullString;
        }
        else
        {
            fullString = "You bought every perk";
            textoInteraccion.text = fullString;
        }
    }


    private void GivePerk(List<int> missingPerks)
    {
        int randomIndex = Random.Range(0, missingPerks.Count);
        int perkIndex = missingPerks[randomIndex];

        player.activePerks[perkIndex] = true;

        Player.PlayerPerks perk = (Player.PlayerPerks)perkIndex;

        if (perk == Player.PlayerPerks.Shielded) UnityEvents.shielded.Invoke(true);
        StartCoroutine(VisualPurchase("You got: " + perk));
        UnityEvents.onPerkActive.Invoke();
    }
}