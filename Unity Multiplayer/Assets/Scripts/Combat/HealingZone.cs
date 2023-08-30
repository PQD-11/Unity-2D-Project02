using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealingZone : NetworkBehaviour
{
    [SerializeField] private Image healPowerBar;
    [SerializeField] private int maxHealPower = 30;
    [SerializeField] private float healCoolDown = 15f;
    [SerializeField] private float healTickRate = 1f;
    [SerializeField] private int coinPerTick = 10;
    [SerializeField] private int healPerTick = 10;

    private List<MainPlayer> playersInZone = new List<MainPlayer>();
    private NetworkVariable<int> HealPower = new NetworkVariable<int>();

    private float remainingCooldown;
    private float tickTimer;

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged += HandleHealPowerChanged;
            HandleHealPowerChanged(0, HealPower.Value);
        }

        if (IsServer)
        {
            HealPower.Value = 0;
            remainingCooldown = healCoolDown;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealPower.OnValueChanged -= HandleHealPowerChanged;
        }
    }

    private void Update()
    {
        if (!IsServer) { return; }

        if (remainingCooldown > 0f)
        {
            remainingCooldown -= Time.deltaTime;

            if (remainingCooldown <= 0f)
            {
                HealPower.Value = maxHealPower;
            }
            else
            {
                return;
            }
        }

        tickTimer += Time.deltaTime;
        if (tickTimer >= 1 / healTickRate)
        {
            foreach (MainPlayer player in playersInZone)
            {
                if (HealPower.Value == 0) { break; }

                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) { continue; }

                if (player.Wallet.TotalCoins.Value < coinPerTick) { continue; }

                player.Wallet.SpendCoins(coinPerTick);
                player.Health.RestoreHealth(healPerTick);

                HealPower.Value -= 1;

                if (HealPower.Value == 0)
                {
                    remainingCooldown = healCoolDown;
                }
            }
            tickTimer = tickTimer % (1 / healTickRate);
        }
    }

    private void HandleHealPowerChanged(int previousValue, int newValue)
    {
        healPowerBar.fillAmount = (float)newValue / maxHealPower;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.attachedRigidbody.TryGetComponent<MainPlayer>(out MainPlayer player))
            {
                playersInZone.Add(player);
            }

            Debug.Log($"Entered: {player.PlayerName.Value}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsServer)
        {
            if (other.attachedRigidbody.TryGetComponent<MainPlayer>(out MainPlayer player))
            {
                playersInZone.Remove(player);
            }

            Debug.Log($"Left: {player.PlayerName.Value}");
        }
    }
}
