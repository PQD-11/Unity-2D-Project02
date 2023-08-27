using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CoinWallet : NetworkBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private BountyCoin bountyCoinPrefabs;

    [SerializeField] private float coinSpread = 3f;
    [SerializeField] private float bountyPercentage = 50;
    [SerializeField] private int bountyCoinCount = 10;
    [SerializeField] private int minBountyCoinValue = 5;
    [SerializeField] private LayerMask layerMask;

    private Collider2D[] coinBuffer = new Collider2D[1];
    private float coinRadius;
    public NetworkVariable<int> TotalCoins = new NetworkVariable<int>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            coinRadius = bountyCoinPrefabs.GetComponent<CircleCollider2D>().radius;

            health.OnDie += HendleDie;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            health.OnDie -= HendleDie;
        }
    }

    private void HendleDie(Health health)
    {
        int bountyValue = (int)(TotalCoins.Value * (bountyPercentage / 100));

        int bountyCoinValue = bountyValue / bountyCoinCount;

        if (bountyCoinValue >= minBountyCoinValue)
        {
            for (int i = 0; i < bountyCoinCount; i++)
            {
                BountyCoin cointInstance = Instantiate(bountyCoinPrefabs, GetSpawnPoint(), Quaternion.identity);
                cointInstance.SetValue(bountyCoinValue);
                cointInstance.GetComponent<NetworkObject>().Spawn(); 
            }
        }
    }

    private Vector2 GetSpawnPoint()
    {
        while (true)
        {
            Vector2 spawnPoint = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * coinSpread;
            int numColliders = Physics2D.OverlapCircleNonAlloc(spawnPoint, coinRadius, coinBuffer, layerMask);
            if (numColliders == 0)
            {
                return spawnPoint;
            }
        }
    }

    public void SpendCoins(int costToFire)
    {
        TotalCoins.Value -= costToFire;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Coin>(out Coin coin)) { return; }

        int coinValue = coin.Collect();

        if (!IsServer) { return; }

        TotalCoins.Value += coinValue;
    }
}
