using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameCreate : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameField;
    [SerializeField] private Button createButton;
    [SerializeField] private int minNameLenght = 1;
    [SerializeField] private int maxNameLenght = 15;

    private void Start()
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            return;
        }

        nameField.text = PlayerPrefs.GetString("PlayerName", string.Empty);
        HandleChangeName();
    }

    public void HandleChangeName()
    {
        createButton.interactable = nameField.text.Length <= maxNameLenght && nameField.text.Length >= minNameLenght;
    }

    public void Create()
    {
        PlayerPrefs.SetString("PlayerName", nameField.text);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

}
