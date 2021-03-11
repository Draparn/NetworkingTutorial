using SmallMultiplayerGame.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SmallMultiplayerGame.Shared.Net
{
	/// <summary>Sent from server to client.</summary>
	public enum ServerPackets
	{
		welcome = 1,
		serverFull,
		nameTaken,
		newClientInfo,
		playerSpawn,
		playerDisconnected,
		playerRespawn,
		playerHealth,
		playerWeaponSwitch,
		playerFiredWeapon,
		weaponSpawn,
		weaponPickupStatus,
		weaponPickup,
		weaponAmmoUpdate,
		projectileSpawn,
		projectileExplosion,
		healthpackSpawn,
		healthpackStatusUpdate,
		serverSnapshot
	}

	/// <summary>Sent from client to server.</summary>
	public enum ClientPackets
	{
		connectRequest = 1,
		welcomeReceived,
		disconnect,
		playerMovement,
		playerPrimaryFire,
		playerWeaponSwitch
	}

	public class Packet : IDisposable
	{
		private List<byte> buffer;

		private byte[] readableBuffer;
		private byte id;
		private int readPos;

		public Packet()
		{
			buffer = new List<byte>();
			readPos = 0;
		}
		public Packet(byte _id)
		{
			id = _id;
			buffer = new List<byte>();
			readPos = 0;

			Write(_id);
		}
		public Packet(byte[] _data)
		{
			buffer = new List<byte>();
			readPos = 0;

			SetBytes(_data);
		}

		#region Functions
		/// <summary>Sets the packet's content and prepares it to be read.</summary>
		public void SetBytes(byte[] _data)
		{
			Write(_data);
			readableBuffer = buffer.ToArray();
		}
		/// <summary>Inserts the length of the packet's content at the start of the buffer as a ushort.</summary>
		public void WriteLength()
		{
			buffer.InsertRange(0, BitConverter.GetBytes((ushort)buffer.Count));
		}
		/// <summary>Inserts the given byte at the start of the buffer.</summary>
		public void InsertByte(byte _value)
		{
			buffer.Insert(0, _value);
		}
		/// <summary>Returns the packet's contents in array form.</summary>
		public byte[] ToArray()
		{
			readableBuffer = buffer.ToArray();
			return readableBuffer;
		}
		/// <summary>Returns the length of the packet's content.</summary>
		public int GetLength()
		{
			return buffer.Count;
		}
		/// <summary>Returns the length of the unread data contained in the packet.</summary>
		public int UnreadLength()
		{
			return GetLength() - readPos;
		}
		/// <summary>Resets the packet instance to allow it to be reused.</summary>
		public void Reset()
		{
			buffer.Clear();
			readableBuffer = null;
			readPos = 0;

			Write(id);
		}
		#endregion

		#region Write Data
		/// <summary>Adds a Byte to the packet.</summary>
		public void Write(byte _value)
		{
			buffer.Add(_value);
		}
		/// <summary>Adds an array of bytes to the packet.</summary>
		public void Write(byte[] _value)
		{
			buffer.AddRange(_value);
		}
		/// <summary>Adds a short to the packet.</summary>
		public void Write(short _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a ushort to the packet.</summary>
		public void Write(ushort _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds an int to the packet.</summary>
		public void Write(int _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a uint to the packet.</summary>
		public void Write(uint _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a long to the packet.</summary>
		public void Write(long _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a ulong to the packet.</summary>
		public void Write(ulong _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a float to the packet.</summary>
		public void Write(float _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a bool to the packet.</summary>
		public void Write(bool _value)
		{
			buffer.AddRange(BitConverter.GetBytes(_value));
		}
		/// <summary>Adds a string to the packet.</summary>
		public void Write(string _value)
		{
			Write(_value.Length);
			buffer.AddRange(Encoding.ASCII.GetBytes(_value));
		}
		///<summary>Adds adds a Quaternion to the packet.</summary>
		public void Write(Quaternion quat)
		{
			//Finds the largest value and writes that. Needed for numerical precision.
			if (Mathf.Abs(quat.y) > Mathf.Abs(quat.w))
			{
				Write(false);
				Write(quat.y < 0 ? (short)-ValueTypeConversions.ReturnDecimalsAsShort(quat.w) : ValueTypeConversions.ReturnDecimalsAsShort(quat.w));
			}
			else
			{
				Write(true);
				Write(quat.w < 0 ? (short)-(ValueTypeConversions.ReturnDecimalsAsShort(quat.y)) : ValueTypeConversions.ReturnDecimalsAsShort(quat.y));
			}
		}
		///<summary>Adds adds a Vector3 to the packet.</summary>
		public void Write(Vector3 vector)
		{
			Write((byte)vector.x);
			Write((byte)vector.y);
			Write((byte)vector.z);
			Write(ValueTypeConversions.ReturnDecimalsAsShort(vector.x));
			Write(ValueTypeConversions.ReturnDecimalsAsShort(vector.y));
			Write(ValueTypeConversions.ReturnDecimalsAsShort(vector.z));
		}
		#endregion

		#region Read Data
		/// <summary>Reads a byte from the packet.</summary>
		public byte ReadByte(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				byte _value = readableBuffer[readPos];
				if (_moveReadPos)
				{
					readPos += 1;
				}
				return _value;
			}
			else
				throw new Exception("Could not read value of type 'byte'!");
		}
		/// <summary>Reads an array of bytes from the packet.</summary>
		public byte[] ReadBytes(int _length, bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				byte[] _value = buffer.GetRange(readPos, _length).ToArray();
				if (_moveReadPos)
					readPos += _length;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'byte[]'!");
		}
		/// <summary>Reads a short from the packet.</summary>
		public short ReadShort(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				short _value = BitConverter.ToInt16(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 2;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'short'!");
		}
		/// <summary>Reads a ushort from the packet.</summary>
		public ushort ReadUShort(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				ushort _value = BitConverter.ToUInt16(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 2;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'ushort'!");
		}
		/// <summary>Reads an int from the packet.</summary>
		public int ReadInt(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				int _value = BitConverter.ToInt32(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 4;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'int'!");
		}
		/// <summary>Reads a uint from the packet.</summary>
		public uint ReadUInt(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				uint _value = BitConverter.ToUInt32(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 4;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'uint'!");
		}
		/// <summary>Reads a long from the packet.</summary>
		public long ReadLong(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				long _value = BitConverter.ToInt64(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 8;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'long'!");
		}
		/// <summary>Reads a ulong from the packet.</summary>
		public ulong ReadULong(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				ulong _value = BitConverter.ToUInt64(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 8;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'ulong'!");
		}
		/// <summary>Reads a float from the packet.</summary>
		public float ReadFloat(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				float _value = BitConverter.ToSingle(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 4;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'float'!");
		}
		/// <summary>Reads a bool from the packet.</summary>
		public bool ReadBool(bool _moveReadPos = true)
		{
			if (buffer.Count > readPos)
			{
				bool _value = BitConverter.ToBoolean(readableBuffer, readPos);
				if (_moveReadPos)
					readPos += 1;

				return _value;
			}
			else
				throw new Exception("Could not read value of type 'bool'!");
		}
		/// <summary>Reads a string from the packet.</summary>
		public string ReadString(bool _moveReadPos = true)
		{
			try
			{
				int _length = ReadInt();
				string _value = Encoding.ASCII.GetString(readableBuffer, readPos, _length);
				if (_moveReadPos && _value.Length > 0)
					readPos += _length;

				return _value;
			}
			catch
			{
				throw new Exception("Could not read value of type 'string'!");
			}
		}
		/// <summary>Reads a Quaternion from the packet.</summary>
		public Quaternion ReadQuaternion()
		{
			var floatIsY = ReadBool();
			var sentFloat = ValueTypeConversions.ReturnShortAsFloat(ReadShort());
			var omittedFloat = Mathf.Sqrt(1.0f - (sentFloat * sentFloat));

			if (floatIsY)
				return new Quaternion(0, sentFloat, 0, omittedFloat);

			return new Quaternion(0, omittedFloat, 0, sentFloat);
		}
		/// <summary>Reads a Vector3 from the packet.</summary>
		public Vector3 ReadVector3()
		{
			var wholeX = ReadByte();
			var wholeY = ReadByte();
			var wholeZ = ReadByte();
			var decimalsX = ValueTypeConversions.ReturnShortAsFloat(ReadShort());
			var decimalsY = ValueTypeConversions.ReturnShortAsFloat(ReadShort());
			var decimalsZ = ValueTypeConversions.ReturnShortAsFloat(ReadShort());

			return new Vector3(wholeX + decimalsX, wholeY + decimalsY, wholeZ + decimalsZ);
		}
		#endregion

		private bool disposed = false;

		protected virtual void Dispose(bool _disposing)
		{
			if (!disposed)
			{
				if (_disposing)
				{
					buffer = null;
					readableBuffer = null;
					readPos = 0;
				}

				disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}