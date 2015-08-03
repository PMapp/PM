using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrosTest4.Models
{
	public class Sifre
	{
		public class SifreRequestBody
		{
			public string sifre { get; set; }
		}

		public class SifreResponseBody
		{
            public int kapakNo { get; set; }
			public ushort kilit { get; set; }
            public ushort icSicaklik { get; set; }
			public ushort disSicaklik { get; set; }
			public ushort hedefIcSicaklik { get; set; }
			public ushort islemciSicaklik { get; set; }
			public ushort kapi { get; set; }
			public ushort doluluk { get; set; }
            public ushort gerilim { get; set; }
            public ushort akim { get; set; }

		}

	}
}
