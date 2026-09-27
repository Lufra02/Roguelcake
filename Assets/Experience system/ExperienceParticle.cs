using System;
using UnityEngine;

public class ExperienceParticle : MonoBehaviour
{
    public int amountOfExperience;
    private void OnTriggerEnter(Collider other)
    {
        PlayerManager player = PlayerManager.Instance;
        if (player == null) return;

        player.playerStats.AddXP(amountOfExperience);
    }
}
