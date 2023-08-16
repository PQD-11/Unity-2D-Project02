using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ClientGameManager
{
    private string MenuSceneName = "Menu";

    public async Task<bool> InitAsync()
    {
        //Authentiate player
        await UnityServices.InitializeAsync();

        AuthState authState = await AuthenticationWrapper.DoAuth();

        if (authState == AuthState.Authenticated)
        {
            return true;
        }

        return false;
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(MenuSceneName);
    }
}
