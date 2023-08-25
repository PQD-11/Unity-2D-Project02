using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class LeaderBoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text displayText;
    [SerializeField] private Color myColor;
    private FixedString32Bytes playerName;

    public ulong ClientId {get; private set;}
    public int Coins {get; private set;}
    public void Initialise(ulong clientId, FixedString32Bytes playerName, int coins)
    {
        ClientId = clientId;
        this.playerName = playerName;  

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            displayText.color = myColor; 
        }
        
        UpdateCoin(coins);
    }

    public void UpdateCoin(int coins)
    {
        Coins = coins;
        
        UpdateText();
    }

    public void UpdateText()
    { 
        displayText.text = $"[{transform.GetSiblingIndex() + 1}] {playerName} - {Coins}";
    }
}
