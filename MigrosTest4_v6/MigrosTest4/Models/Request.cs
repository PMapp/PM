using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrosTest4.Models
{
	public class Request
	{
		public RequestHeader requestHeader { get; set; }
		public object requestBody { get; set; }
	}

	public class RequestHeader
	{
		public string userName { get; set; }
		public string password { get; set; }
		public string serviceName { get; set; }
	}
}
