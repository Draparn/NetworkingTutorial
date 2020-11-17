using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace GameServer
{
	public class Client
	{
		public int Id;

		public TCP tcp;

		public Client(int id)
		{
			Id = id;
			tcp = new TCP(id);
		}
	}
}
