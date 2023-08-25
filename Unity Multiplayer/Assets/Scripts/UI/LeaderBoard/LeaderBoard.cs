using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntityHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int entitiesToDisplay = 8;

    private NetworkList<LeaderBoardEntityState> leaderBoardEntities;
    private List<LeaderBoardEntityDisplay> entityDisplays = new List<LeaderBoardEntityDisplay>();

    private void Awake()
    {
        leaderBoardEntities = new NetworkList<LeaderBoardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderBoardEntities.OnListChanged += HandleLeaderBoardEntitiesChanged;
            foreach (var entity in leaderBoardEntities)
            {
                HandleLeaderBoardEntitiesChanged(new NetworkListEvent<LeaderBoardEntityState>
                {
                    Type = NetworkListEvent<LeaderBoardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            MainPlayer[] players = FindObjectsByType<MainPlayer>(FindObjectsSortMode.None);
            foreach (var p in players)
            {
                HandlePlayerSpawned(p);
            }

            MainPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            MainPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderBoardEntities.OnListChanged -= HandleLeaderBoardEntitiesChanged;
        }

        if (IsServer)
        {
            MainPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            MainPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
    }

    private void HandleLeaderBoardEntitiesChanged(NetworkListEvent<LeaderBoardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Add:
                if (!entityDisplays.Any(x => x.ClientId == changeEvent.Value.ClientId))
                {
                    LeaderBoardEntityDisplay leaderBoardEntityDisplay = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                    leaderBoardEntityDisplay.Initialise(changeEvent.Value.ClientId, changeEvent.Value.PlayerName, changeEvent.Value.Coins);
                    entityDisplays.Add(leaderBoardEntityDisplay);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Remove:
                LeaderBoardEntityDisplay leaderBoardEntityDisplayToRemove =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (leaderBoardEntityDisplayToRemove != null)
                {
                    leaderBoardEntityDisplayToRemove.transform.SetParent(null);
                    Destroy(leaderBoardEntityDisplayToRemove.gameObject);
                    entityDisplays.Remove(leaderBoardEntityDisplayToRemove);
                }
                break;
            case NetworkListEvent<LeaderBoardEntityState>.EventType.Value:
                LeaderBoardEntityDisplay leaderBoardEntityDisplayToUpdate =
                    entityDisplays.FirstOrDefault(x => x.ClientId == changeEvent.Value.ClientId);
                if (leaderBoardEntityDisplayToUpdate != null)
                {
                    leaderBoardEntityDisplayToUpdate.UpdateCoin(changeEvent.Value.Coins);
                }
                break;
        }
        entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
        for (int i = 0; i < entityDisplays.Count; i++)
        {
            entityDisplays[i].transform.SetSiblingIndex(i);
            entityDisplays[i].UpdateText();
            entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
        }

        // LeaderBoardEntityDisplay myDisplay = 
        //     entityDisplays.FirstOrDefault(x => x.ClientId == NetworkManager.Singleton.LocalClientId);
        
        // if (myDisplay != null)
        // {
        //     if (myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
        //     {
        //         leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
        //         myDisplay.gameObject.SetActive(true);
        //     }
        // }
    }

    private void HandlePlayerSpawned(MainPlayer player)
    {
        leaderBoardEntities.Add(new LeaderBoardEntityState
        {
            ClientId = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Coins = 0
        });
        player.Wallet.TotalCoins.OnValueChanged += (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
    }

    private void HandleCoinsChanged(ulong ClientId, int newCoins)
    {
        for (int i = 0; i < leaderBoardEntities.Count; i++)
        {
            if (leaderBoardEntities[i].ClientId == ClientId)
            {
                leaderBoardEntities[i] = new LeaderBoardEntityState
                {
                    ClientId = leaderBoardEntities[i].ClientId,
                    PlayerName = leaderBoardEntities[i].PlayerName,
                    Coins = newCoins
                };
                return;
            }
        }
    }

    private void HandlePlayerDespawned(MainPlayer player)
    {
        foreach (var entity in leaderBoardEntities)
        {
            if (entity.ClientId == player.OwnerClientId)
            {
                leaderBoardEntities.Remove(entity);
            }
        }
        player.Wallet.TotalCoins.OnValueChanged -= (oldCoins, newCoins) =>
            HandleCoinsChanged(player.OwnerClientId, newCoins);
    }
}
