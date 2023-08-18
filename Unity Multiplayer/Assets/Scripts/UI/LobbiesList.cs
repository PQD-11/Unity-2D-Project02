using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;
    private bool isJoining;
    private bool isRefreshing;

    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        if (isRefreshing) { return; }

        isRefreshing = true;

        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions();
            queryLobbiesOptions.Count = 20;
            queryLobbiesOptions.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"
                    ),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0"
                    ),
            };

            var lobbies = await Lobbies.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }

            foreach (Lobby lobby in lobbies.Results)
            {
                var lobbyItem = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyItem.Intialise(this, lobby);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            return;
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby lobby)
    {
        if (isJoining) { return; }

        isJoining = true;
        try
        {
            Lobby joiningLobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;

            await ClientSingleton.Instance.GameManager.StartClientAsync(joinCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);

        }

        isJoining = false;
    }
}

