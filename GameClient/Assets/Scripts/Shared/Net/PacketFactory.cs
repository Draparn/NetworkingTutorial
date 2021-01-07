using System.Collections.Generic;

namespace NetworkTutorial.Shared.Net
{
	class PacketFactory
	{
		private static Dictionary<ServerPackets, Packet> serverPacketTypes = new Dictionary<ServerPackets, Packet>()
		{
			{ServerPackets.welcome, new Packet((int)ServerPackets.welcome) },
			{ServerPackets.serverFull, new Packet((int)ServerPackets.serverFull) },
			{ServerPackets.spawnPlayer, new Packet((int)ServerPackets.spawnPlayer) },
			{ServerPackets.playerRotation, new Packet((int)ServerPackets.playerRotation) },
			{ServerPackets.playerDisconnected, new Packet((int)ServerPackets.playerDisconnected) },
			{ServerPackets.playerRespawn, new Packet((int)ServerPackets.playerRespawn) },
			{ServerPackets.playerHealth, new Packet((int)ServerPackets.playerHealth) },
			{ServerPackets.projectileSpawn, new Packet((int)ServerPackets.projectileSpawn) },
			{ServerPackets.projectileExplosion, new Packet((int)ServerPackets.projectileExplosion) },
			{ServerPackets.healthpackSpawn, new Packet((int)ServerPackets.healthpackSpawn) },
			{ServerPackets.healthpackActivate, new Packet((int)ServerPackets.healthpackActivate) },
			{ServerPackets.healthpackDeactivate, new Packet((int)ServerPackets.healthpackDeactivate) },
			{ServerPackets.serverSnapshot, new Packet((int)ServerPackets.serverSnapshot) },
		};

		private static Dictionary<ClientPackets, Packet> clientPacketTypes = new Dictionary<ClientPackets, Packet>()
		{
			{ClientPackets.connectRequest, new Packet((int)ClientPackets.connectRequest) },
			{ClientPackets.welcomeReceived, new Packet((int)ClientPackets.welcomeReceived) },
			{ClientPackets.disconnect, new Packet((int)ClientPackets.disconnect) },
			{ClientPackets.playerMovement, new Packet((int)ClientPackets.playerMovement) },
			{ClientPackets.playerPrimaryFire, new Packet((int)ClientPackets.playerPrimaryFire) }
		};

		public static Packet GetClientPacketType(ClientPackets packetType)
		{
			return clientPacketTypes[packetType];
		}

		public static Packet GetServerPacketType(ServerPackets packetType)
		{
			return serverPacketTypes[packetType];
		}

	}
}
