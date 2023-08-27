using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class RespawnHandler : NetworkBehaviour
{
    [SerializeField] private MainPlayer playerPrefabs;
    [SerializeField] private float keptCoinPercentage;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        MainPlayer[] players = FindObjectsByType<MainPlayer>(FindObjectsSortMode.None);
        foreach (MainPlayer p in players) 
        {
            HandlePlayerSpawned(p);
        }

        MainPlayer.OnPlayerSpawned += HandlePlayerSpawned;
        MainPlayer.OnPlayerDespawned += HandlePlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }

        MainPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
        MainPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
    }

    private void HandlePlayerSpawned(MainPlayer player)
    {
        player.Health.OnDie += (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDespawned(MainPlayer player)
    {
        player.Health.OnDie -= (health) => HandlePlayerDie(player);
    }

    private void HandlePlayerDie(MainPlayer player)
    {
        int keptCoin = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage/ 100));

        Destroy(player.gameObject);

        StartCoroutine(ReSpawnPlayer(player.OwnerClientId, keptCoin));
    }

    private IEnumerator ReSpawnPlayer(ulong ownerClientId, int keptCoin)
    {
        yield return null;

        MainPlayer playerInstance = Instantiate(playerPrefabs, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);

        playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
        playerInstance.Wallet.TotalCoins.Value += keptCoin;
    }

}
