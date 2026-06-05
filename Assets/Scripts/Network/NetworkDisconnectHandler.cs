using Fusion;
using System;
using System.Collections;
using UnityEngine;
using static Unity.Collections.Unicode;

public class NetworkDisconnectHandler : MonoBehaviour
{
    private void Start()
    {
        NetworkRunner.CloudConnectionLost += OnCloudConnectionLost;
    }

    private void OnCloudConnectionLost(NetworkRunner runner, ShutdownReason reason, bool reconnecting)
    {
        if (reconnecting)
        {
            // e.g. notify player the game is reconnecting
            Debug.Log($"Cloud Connection Lost: {reason}, reconnecting...");
            // then, wait for automatic reconnection to complete
            StartCoroutine(WaitForReconnection(runner));
        }
        else
        {
            Debug.Log($"Cloud Connection Lost: {reason}.");
            // Handle scenarios where reconnection is not possible
            // e.g. notify the user, fully shutdown the NetworkRunner, etc.
        }
    }

    private IEnumerator WaitForReconnection(NetworkRunner runner)
    {
        // e.g. notify player the game has reconnected
        yield return new WaitUntil(() => runner.IsInSession);
        Debug.Log("Reconnected to the Cloud!");
    }
}
