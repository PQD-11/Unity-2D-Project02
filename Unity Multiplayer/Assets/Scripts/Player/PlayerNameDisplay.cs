using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private MainPlayer player;
    [SerializeField] private TMP_Text nameText;

    private void Start()
    {
        OnPlayerNameValueChanged(String.Empty, player.PlayerName.Value);

        player.PlayerName.OnValueChanged += OnPlayerNameValueChanged;
    }

    private void OnDestroy()
    {
        player.PlayerName.OnValueChanged -= OnPlayerNameValueChanged;
    }

    private void OnPlayerNameValueChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
    {
        nameText.text = newValue.ToString();
    }
}
