using CustomUtils;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetwrokConnectHandler : SimulationBehaviour//, INetworkRunnerCallbacks
{
    //[SerializeField] private float clientReconnectTimeout = 30f; // Thời gian chờ Client kết nối lại

    //private Coroutine waitClientReconnectCoroutine;
    //private PlayerRef disconnectedClientRef;

    ////private void Start()
    ////{
    ////    if (Runner != null)
    ////        Runner.AddCallbacks(this);
    ////}

    ////private void OnDestroy()
    ////{
    ////    if (Runner != null)
    ////        Runner.RemoveCallbacks(this);
    ////}

    //public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    //{
    //    if (runner.IsServer)
    //    {
    //        Debug.Log($"Host phát hiện Player {player.PlayerId} đã rời phòng hoặc mất mạng.");

    //        disconnectedClientRef = player;

    //        if (waitClientReconnectCoroutine != null) StopCoroutine(waitClientReconnectCoroutine);
    //        waitClientReconnectCoroutine = StartCoroutine(Co_HostWaitForClientReconnect(runner, player));
    //    }
    //}

    //private IEnumerator Co_HostWaitForClientReconnect(NetworkRunner runner, PlayerRef clientPlayer)
    //{
    //    float timer = clientReconnectTimeout;

    //    // Thông báo cho chính Host biết Client đang mất kết nối (Hiện UI chờ trên máy Host)
    //    ChessManager.Instance.Local_ShowWaitingForClientUI(Mathf.CeilToInt(timer));

    //    // Gửi RPC thông báo cho Client (nếu Client chỉ bị lag nhẹ và vẫn bắt được RPC từ Host)
    //    ChessManager.Instance.RPC_NotifyWaitingForReconnect(Mathf.CeilToInt(timer));

    //    while (timer > 0)
    //    {
    //        yield return new WaitForSeconds(1f);
    //        timer -= 1f;

    //        // Cập nhật UI hiển thị số giây đếm ngược trên máy Host
    //        ChessManager.Instance.Local_ShowWaitingForClientUI(Mathf.CeilToInt(timer));
    //        ChessManager.Instance.RPC_NotifyWaitingForReconnect(Mathf.CeilToInt(timer));
    //    }

    //    Debug.Log($"Hết thời gian chờ kết nối lại. Xử thua Client {clientPlayer.PlayerId}");

    //    // Host xử lý logic kết thúc trận đấu (Client thua, Host thắng)
    //    ChessManager.Instance.Host_HandleClientReconnectTimeout(clientPlayer);

    //    waitClientReconnectCoroutine = null;
    //}

    //public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    //{
    //    if (runner.IsServer)
    //    {
    //        // Nếu người vừa vào đúng là Client vừa bị rớt mạng lúc nãy
    //        if (waitClientReconnectCoroutine != null && player == disconnectedClientRef)
    //        {
    //            Debug.Log($"Client {player.PlayerId} đã kết nối lại thành công trước khi hết giờ!");
    //            StopCoroutine(waitClientReconnectCoroutine);
    //            waitClientReconnectCoroutine = null;

    //            // Gửi RPC thông báo cho cả 2 bên tắt UI chờ đợi, tiếp tục trận đấu
    //            ChessManager.Instance.RPC_NotifyReconnectSuccess();
    //        }
    //    }
    //}

    //public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    //{
    //    //ấn quit button hoặc kết thúc game bình thường
    //    if (shutdownReason == ShutdownReason.Ok)
    //    {
    //        Debug.Log("Shutdown bình thường: Người chơi chủ động thoát hoặc game kết thúc hợp lệ.");
    //        return;
    //    }

    //    // Lỗi mạng
    //    if (!runner.IsServer)
    //    {
    //        Debug.Log($"[Client] Kết nối tới Host bị đứt đột ngột. Lý do: {shutdownReason}");
    //        ChessManager.Instance.Local_HandleHostDisconnected();
    //    }
    //    else
    //    {
    //        Debug.Log($"[Host] Chính bạn (Host) đã bị mất kết nối Internet/Cloud: {shutdownReason}");
    //        ChessManager.Instance.Local_HandleSelfHostDisconnected();
    //    }
    //}

    //public void OnConnectedToServer(NetworkRunner runner)
    //{
        
    //}

    //public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    //{
        
    //}

    //public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    //{
        
    //}

    //public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    //{
        
    //}

    //public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    //{
        
    //}

    //public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    //{
        
    //}

    //public void OnInput(NetworkRunner runner, NetworkInput input)
    //{
        
    //}

    //public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    //{
        
    //}

    //public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    //{
        
    //}

    //public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    //{
        
    //}

    //public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    //{
        
    //}

    //public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    //{
        
    //}

    //public void OnSceneLoadDone(NetworkRunner runner)
    //{
        
    //}

    //public void OnSceneLoadStart(NetworkRunner runner)
    //{
        
    //}

    //public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    //{
        
    //}

    //public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    //{
        
    //}
}
