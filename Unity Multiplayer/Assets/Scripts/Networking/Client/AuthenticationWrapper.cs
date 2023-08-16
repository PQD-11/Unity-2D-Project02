using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public static class AuthenticationWrapper
{
    public static AuthState AuthState { get; private set; } = AuthState.NotAuthenticated;
    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (AuthState == AuthState.Authenticated)
        {
            return AuthState;
        }

        int tries = 0;
        AuthState = AuthState.Authenticating;
        while (AuthState == AuthState.Authenticating && tries < maxTries)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthState = AuthState.Authenticated;
                break;
            }
            tries++;

            await Task.Delay(1000);
        }

        return AuthState;
    }

    private static async Task SignInAnonymouslyAsync(int maxRetries)
    {
        int tries = 0;
        AuthState = AuthState.Authenticating;
        while (AuthState == AuthState.Authenticating && tries < maxRetries)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            if (AuthenticationService.Instance.IsSignedIn && AuthenticationService.Instance.IsAuthorized)
            {
                AuthState = AuthState.Authenticated;
                break;
            }
            tries++;

            await Task.Delay(1000);
        }
    }
}

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}
