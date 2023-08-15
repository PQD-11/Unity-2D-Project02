using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ProjectileLauncher : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private InputReader inputReader;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private GameObject serverProjectilePrefab;
    [SerializeField] private GameObject clientProjectilePrefab;
    [SerializeField] private GameObject spark;
    [SerializeField] private Collider2D playerCollider;
    [SerializeField] private CoinWallet coinWallet;


    [Header("Settings")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private float sparkDuration;
    [SerializeField] private int costToFire;

    private bool shouldFire;
    private float previousFireTime;
    private float sparkTimer;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent += HandlePrimaryFire;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) { return; }

        inputReader.PrimaryFireEvent -= HandlePrimaryFire;
    }

    private void HandlePrimaryFire(bool obj)
    {
        shouldFire = obj;
    }

    void Update()
    {
        if (sparkTimer > 0f)
        {
            sparkTimer -= Time.deltaTime;
        }
        else
        {
            spark.SetActive(false);
        }

        if (!IsOwner) { return; }

        if (!shouldFire) { return; }

        if (Time.time < (1 / fireRate) + previousFireTime) { return; }

        if (coinWallet.TotalCoins.Value < costToFire) { return; }

        PrimaryFireServerRpc(projectileSpawnPoint.position, projectileSpawnPoint.up);

        SpawnProjectile(projectileSpawnPoint.position, projectileSpawnPoint.up);

        previousFireTime = Time.time;
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        // Debug.Log("Server");

        if (coinWallet.TotalCoins.Value < costToFire) { return; }

        coinWallet.SpendCoins(costToFire);

        GameObject projectileInstance = Instantiate(
            serverProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<DealDamageOnContact>(out DealDamageOnContact dealDame))
        {
            dealDame.SetOwner(OwnerClientId);
        }

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }

        SpawnProjectileClientRpc(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) { return; }

        SpawnProjectile(spawnPos, direction);
        // Debug.Log("SpawnProjectileClientRpc");

    }
    private void SpawnProjectile(Vector3 spawnPos, Vector3 direction)
    {
        spark.SetActive(true);
        sparkTimer = sparkDuration;

        // Debug.Log("SpawnProjectile");
        GameObject projectileInstance = Instantiate(
            clientProjectilePrefab,
            spawnPos,
            Quaternion.identity);

        projectileInstance.transform.up = direction;

        Physics2D.IgnoreCollision(playerCollider, projectileInstance.GetComponent<Collider2D>());

        if (projectileInstance.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
        {
            rb.velocity = rb.transform.up * projectileSpeed;
        }
    }
}
