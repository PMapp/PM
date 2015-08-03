using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrosTest4.Models
{
	public class Response
	{
		public ResponseHeader responseHeader { get; set; }
		public ResponseBody responseBody { get; set; }
	}

	public class ResponseBody
	{
		public List<Sifre.SifreResponseBody> kapakListesi { get; set; }
	}

	public class ResponseHeader
	{
		public string status { get; set; }
		public string resultCode { get; set; }
		public string resultMessage { get; set; }
	}
}
