using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{


    public ulong clientId;
    public int noOfSpawnPoint;
    public FixedString64Bytes playerTeam;
    public FixedString64Bytes playerDriver;
    public FixedString64Bytes playerShip;
    public FixedString64Bytes playerName;
    public FixedString64Bytes playerEmail;
    public FixedString64Bytes playerId;


    public bool Equals(PlayerData other)
    {
        return
            clientId == other.clientId &&
            playerTeam == other.playerTeam &&
            playerDriver == other.playerDriver &&
            playerShip == other.playerShip &&
            playerName == other.playerName &&
            playerEmail == other.playerEmail &&
            playerId == other.playerId;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerTeam);
        serializer.SerializeValue(ref playerDriver);
        serializer.SerializeValue(ref playerShip);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerEmail);
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref noOfSpawnPoint);
    }

}