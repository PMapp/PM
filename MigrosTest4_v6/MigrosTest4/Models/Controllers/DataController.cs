using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Web.Http;
using MigrosTest4.Models;
using MigrosTest4;

using System.IO.Ports;
using System.IO;

namespace MigrosTest4.Controllers
{
	/* Örnek amaçlı oluşturulmuş basit bir Controller. Bu sınıfın Web API'ye gelen çağrılara cevap
	verebilmesi için ApiController sınıfından türetilmesi gerekmektedir.*/ 
	public class DataController : ApiController
	{
		static SerialPort port;
		static List<byte> byteList = new List<byte>();

		public const byte STX = 0x02; //Start of the byte array
		public const byte ETX = 0x03; //End of the byte array
		public const byte DBoxServerAddr = 0xF0; //Server Address

		public static readonly ushort[] errorBytes = { 9999, 9999 };
		public static readonly ushort[] bccError = { 64999 };
		public static readonly ushort[] ackError = { 64995 };
		public static readonly ushort[] brokenDataError = { 64990 };
		public static readonly ushort[] dataSizeError = { 64985 };
		public static readonly ushort[] STXError = { 64980 };
		public static readonly ushort[] serverAddressError = { 64975 };
		public static readonly ushort[] packetTypeError = { 64970 };
		public static readonly ushort[] setDoneCorrectly = { 64965 };

		public static readonly byte[] bccBytes = new byte[16] { 0x30, 0x31, 0x32, 0x33 , 0x34,
		0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46 };	
		
		string clientAddress = "";	

		// Web API'ye yapılan Get() çağrısına verilen cevap
		public string Get()
		{
			return "asdf";
		}

		// Web API'ye yapılan Post() çağrısına verilen cevap
		public Response Post(Request value)
		{
			try
			{
				clientAddress = System.Web.HttpContext.Current.Request.UserHostAddress;
			}
			catch
			{
				clientAddress = "";
			}

			// Gelen requestin değerleri üzerinden kontrol
			if (value.requestHeader.userName == "Migros" && value.requestHeader.password == "Migros2015")
			{
				return IstekIsle(value);
			}
			else
			{
				return new Response() { responseHeader = new ResponseHeader() { status = "-1", resultCode = "ERR01", resultMessage = "AUTHENTICATION ERROR" }, responseBody = new ResponseBody() };
			}
		}

		private Response IstekIsle(Request istek)
		{
			Response cevap = new Response();

			switch (istek.requestHeader.serviceName)
			{
				case "sifreKontrol":

					cevap.responseHeader = new ResponseHeader() { status = "1", resultCode = "BASARILI", resultMessage = "" };

					// Web API'nin muhtemel olarak döndüreceği değişkenler
                    Sifre.SifreResponseBody basicData = new Sifre.SifreResponseBody() { kapakNo = 57, kilit = 1, icSicaklik = 6, disSicaklik = 27, hedefIcSicaklik = 4, islemciSicaklik = 23, kapi = 0, doluluk = 0, 
                    gerilim = 0, akim = 0};
					List<Sifre.SifreResponseBody> tempData = new List<Sifre.SifreResponseBody>();
					tempData.Add(basicData);
					cevap.responseBody = new ResponseBody() { kapakListesi = tempData };					

					break;

				case "cihazDurum":

					List<Sifre.SifreResponseBody> kapakData = new List<Sifre.SifreResponseBody>();

					// Burada yoruma alınan satırlar kontrol amaçlıdır. Aşağıdaki try catch blokları
					//yoruma alınıp bu satırlar yorumdan çıkarılırsa uygulama seri port bağlantıları
					//olmadan çalışmaktadır. 

                    /*byte[] tempBoxAddresses = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
                    ushort[] writeData = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 , 110 ,120};

                    for (short i = 0; i < tempBoxAddresses.Length; i++)
                    {
                        Sifre.SifreResponseBody incomingData = new Sifre.SifreResponseBody() { kapakNo = tempBoxAddresses[i], intFanSt = writeData[0], extFanSt = writeData[1], kilit = writeData[2], lightSt = writeData[3], icSicaklik = writeData[4], disSicaklik = writeData[5], hedefIcSicaklik = writeData[6], islemciSicaklik = writeData[7], kapi = writeData[8], doluluk = writeData[9], gerilim = writeData[10], akim = writeData[11] };
                        kapakData.Add(incomingData);	
                    }*/

                    try
					{
						using( port = new SerialPort("COM2", 57600, Parity.None, 8, StopBits.One) )
						{
							if (!port.IsOpen)
							{
								port.Open();
								if (port.IsOpen)
								{
									//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
									port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

									byte[] tempBoxAddresses = { 56, 57, 58, 61, 5, 6, 7, 8, 9, 10, 11, 12 };

									for (short i = 0; i < tempBoxAddresses.Length; i++)
									{
										byte[] getCommandFromAddresses = prepareGetPacket(tempBoxAddresses[i]);
                                        byteList.Clear();
										getCommand(getCommandFromAddresses);

										System.Threading.Thread.Sleep(100);

										using (TextWriter tw = new StreamWriter("ErrorInformation.txt", true))
										{
											tw.WriteLine();
											tw.Write("Bytes Read from Port: ");
											for (int j = 0; j < byteList.Count; j++)
											{
												tw.Write(byteList[j] + ", ");
											}

											tw.WriteLine();
											tw.WriteLine();
										}

										ushort[] writeData = new ushort[14];
										writeData = parseList(byteList);

										// Eğer okunan değerlerde bir hata varsa
										if (writeData.SequenceEqual(errorBytes) || writeData.SequenceEqual(bccError) || writeData.SequenceEqual(ackError) || writeData.SequenceEqual(brokenDataError) ||
										writeData.SequenceEqual(dataSizeError) || writeData.SequenceEqual(STXError) || writeData.SequenceEqual(serverAddressError) || writeData.SequenceEqual(packetTypeError))
										{
											Console.WriteLine("Error Bytes returned! BROKEN DATA!!!");
										}
										else
										{
                                            //Sifre.SifreResponseBody incomingData = new Sifre.SifreResponseBody() { boxAddress = tempBoxAddresses[i], intFanSt = writeData[0], extFanSt = writeData[1], lockSt = writeData[2], lightSt = writeData[3], intTemp = writeData[4], extTemp = writeData[5], hedefIcSicaklik = writeData[6], cpuTemp = writeData[7], kapi = writeData[8], presenceSt = writeData[9], gerilim = writeData[12], akim = writeData[13] };	
                                            Sifre.SifreResponseBody incomingData = new Sifre.SifreResponseBody() { kapakNo = i, kilit = writeData[2], icSicaklik = writeData[4], disSicaklik = writeData[5], hedefIcSicaklik = writeData[6], islemciSicaklik = writeData[7], kapi = writeData[8], doluluk = writeData[9], gerilim = writeData[12], akim = writeData[13] };	
											kapakData.Add(incomingData);
										}

									}

									//port.Close();
									//port.Dispose();
								}

								else
								{
									cevap.responseHeader = new ResponseHeader() { status = "-1", resultCode = "PORT ERROR", resultMessage = "" };
								}
							}	
						}						
					}
					catch (Exception ex)
					{
						//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
						using (TextWriter tw = new StreamWriter("GeneralErrors.txt", true))
						{
							tw.WriteLine();
							tw.Write(ex.ToString());							
							tw.WriteLine();
						}
						cevap.responseHeader = new ResponseHeader() { status = "-1", resultCode = "PORT ERROR: " + ex.ToString(), resultMessage = "" };
					}

					if(kapakData.Count==12) // Burada if'in içine daha mantıklı kontroller yazılabilir
					{
						cevap.responseHeader = new ResponseHeader() { status = "1", resultCode = "BASARILI", resultMessage = "" };	
						cevap.responseBody = new ResponseBody() { kapakListesi = kapakData };
					}
					else
					{
						cevap.responseHeader = new ResponseHeader() { status = "-1", resultCode = "BASARISIZ", resultMessage = "DOLAP BİLGİSİ OKUMA HATASI" };
                        cevap.responseBody = new ResponseBody() { kapakListesi = kapakData };
						//ERROR. Kontrol edilmeli
					}				

					break;
				default:
					break;
			}
			return cevap;
		}

		private static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{				
				int bytes = port.BytesToRead;
				byte[] RcvdBytes = new byte[bytes];

				int offset = 0, toRead = bytes;

				int read;
		
				while (toRead > 0 && (read = port.Read(RcvdBytes, offset, toRead)) > 0)
				{
					offset += read;
					toRead -= read;
				}

				if (toRead > 0) throw new System.IO.EndOfStreamException();

				byteList.AddRange(RcvdBytes);

			}
			catch (Exception ex)
			{
				
				Console.WriteLine(ex.ToString());
			}
		}

		static byte[] prepareGetPacket(byte address)
		{
			// Total get command is 7 bytes. This may change later
			byte[] getCommandArray = new byte[7];
			byte tmpBCC;

			getCommandArray[0] = STX;
			tmpBCC = STX;

			getCommandArray[1] = address;
			tmpBCC ^= address;

			getCommandArray[2] = 0x01;
			tmpBCC ^= 0x01;

			getCommandArray[3] = 0x03;
			tmpBCC ^= 0x03;

			// Calculates the BCC using the values in static bccBytes[]
			getCommandArray[4] = bccBytes[(tmpBCC >> 4) & 0x0F];
			getCommandArray[5] = bccBytes[tmpBCC & 0x0F];

			getCommandArray[6] = ETX;

			return getCommandArray;
		}

		static byte[] prepareSetPacket(byte address, byte[] setData)
		{
			// Total set command is 13 bytes. This may change later
			byte[] setCommandArray = new byte[13];
			byte tmpBCC;

			setCommandArray[0] = STX;
			tmpBCC = STX;

			setCommandArray[1] = address;
			tmpBCC ^= address;

			setCommandArray[2] = 0x07;
			tmpBCC ^= 0x07;

			setCommandArray[3] = 0x04;
			tmpBCC ^= 0x04;

			// Data we want to send as set values
			for (int i = 4; i < 4 + setData.Length; i++)
			{
				setCommandArray[i] = setData[i - 4];
				tmpBCC ^= setData[i - 4];
			}

			// Calculates the BCC using the values in static bccBytes[]
			setCommandArray[10] = bccBytes[(tmpBCC >> 4) & 0x0F];
			setCommandArray[11] = bccBytes[tmpBCC & 0x0F];

			setCommandArray[12] = ETX;

			return setCommandArray;
		}

		static void getCommand(byte[] commands)
		{
			// Writes the get command to the port
			port.Write(commands, 0, commands.Length);

			// Writes the get command to the .txt file
			using (TextWriter tw = new StreamWriter("ErrorInformation.txt", true))
			{
				DateTime now = DateTime.Now;

				tw.WriteLine();
				tw.WriteLine(now.ToString("G") + ": ");
				tw.Write("Written Bytes to GET: ");
				for (int i = 0; i < commands.Length; i++)
				{
					tw.Write(commands[i] + ", ");
				}
			}
		}

		static void setCommand(byte[] commands)
		{
			// Writes the set command to the port
			port.Write(commands, 0, commands.Length);

			// Writes the set command to the .txt file
			using (TextWriter tw = new StreamWriter("ErrorInformation.txt", true))
			{
				DateTime now = DateTime.Now;

				tw.WriteLine();
				tw.WriteLine(now.ToString("G") + ": ");
				tw.Write("Written Bytes to SET: ");
				for (int i = 0; i < commands.Length; i++)
				{
					tw.Write(commands[i] + ", ");
				}
			}
		}

		static ushort[] parseList(List<byte> list_in)
		{
			try
			{
				// Stores the return data. If parsing is successful return this array
				ushort[] parsedData = new ushort[14];

				// Temporarily stores the meaningful data (sensor values) as bytes.
				byte[] tempData = new byte[22];

				// 1st condition for a successful parsing
				if (list_in.Count == 29)
				{
					// BCC, later will be used to check if incoming bytes are valid
					ushort calculatedBCC = 0;

					// [STX]-[Address]-[Length]-[CommandType]-[.....Data.....]-[BCC1]-[BCC2]-[ETX]

					// Packet Length. Sensor data length. Represents how many bytes are in the data
					byte IcPacketLength = 0;

					// SET, GET or ACK. There are more types in the QT Code.
					byte commandType = 0;

					// 1st incoming byte should be 0x02
					if (list_in[0] == STX)
					{
						// BCC changes as we read new bytes
						calculatedBCC = STX;

						// 2nd incoming byte should be 0xF0
						if (list_in[1] == DBoxServerAddr)
						{
							// BCC changes as we read new bytes
							calculatedBCC ^= DBoxServerAddr;

							// 3rd byte is Packet Length
							IcPacketLength = list_in[2];
							calculatedBCC ^= IcPacketLength; // Change BCC

							// 4th byte is Command Type
							commandType = list_in[3];
							calculatedBCC ^= commandType; // Change BCC

							// If command type is GET
							if (commandType == 0x03)
							{
								int cnt;
								for (cnt = 1; cnt < IcPacketLength; cnt++)
								{
									// Stores the data in a temporary array
									tempData[cnt - 1] = list_in[3 + cnt];
									calculatedBCC ^= list_in[3 + cnt]; // Change BCC
								}

								calculatedBCC &= 0xFF; // Change BCC

								// If calculated BCC is correct
								if (calculatedBCC == (16 * Ascii2Hex(list_in[3 + cnt]) + Ascii2Hex(list_in[4 + cnt])))
								{
									// Parse the datas as ushort
									parsedData[0] = (ushort)tempData[0];
									parsedData[1] = (ushort)tempData[1];
									parsedData[2] = (ushort)tempData[2];
									parsedData[3] = (ushort)tempData[3];
									parsedData[4] = BitConverter.ToUInt16(tempData, 4);
									parsedData[5] = BitConverter.ToUInt16(tempData, 6);
									parsedData[6] = BitConverter.ToUInt16(tempData, 8);
									parsedData[7] = BitConverter.ToUInt16(tempData, 10);
									parsedData[8] = (ushort)tempData[12];
									parsedData[9] = (ushort)tempData[13];
                                    parsedData[10] = BitConverter.ToUInt16(tempData, 14);
                                    parsedData[11] = BitConverter.ToUInt16(tempData, 16);
                                    parsedData[12] = BitConverter.ToUInt16(tempData, 18);
                                    parsedData[13] = BitConverter.ToUInt16(tempData, 20);


									list_in.Clear(); // Clear the list to use again later

									return parsedData; // If everything is okay return the parsed data.
								}

								else // If calculated BCC is wrong
								{
									Console.WriteLine("BCC Error!");
									list_in.Clear();
									return bccError;
								}
							}

							// If commandType is 0x02 while size is 21
							// BE CAREFUL HERE. This is not wrong for all the cases
							// commandType will be 0x02 when the box is responding SET command 
							else if (commandType == 0x02)
							{
								Console.WriteLine("ACK");
								list_in.Clear();
								return ackError;
							}

							else
							{
								list_in.Clear();
								return packetTypeError;
							}
						}

						else
						{
							list_in.Clear();
							return serverAddressError;
						}
					}

					else
					{
						list_in.Clear();
						return STXError;
					}
				}

				// This is for SET commands
				// Response to a SET command is always same
				// Check the incoming bytes representation at the beginning of the document 
				else if (list_in.Count == 7 && list_in[0] == 0x02 && list_in[1] == 240 &&
						 list_in[2] == 0x01 && list_in[3] == 0x02 && list_in[4] == 70 &&
						 list_in[5] == 49 && list_in[6] == 0x03)
				{
					Console.WriteLine("Set Equal");
					list_in.Clear();
					return setDoneCorrectly;
				}

				else
				{
					list_in.Clear();
					return dataSizeError;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return errorBytes;
			}
		}

		static byte Ascii2Hex(byte ascii)
		{
			if ((ascii > 0x2F) && (ascii < 0x3A))
			{
				return (byte)(ascii - 0x30);
			}

			else
			{
				if ((ascii >= 'A') && (ascii <= 'F'))
				{
					return (byte)(ascii - 0x37);
				}

				else
				{
					if ((ascii >= 'a') && (ascii <= 'f'))
					{
						return (byte)(ascii - 0x57);
					}
				}
			}
			return 0;
		}
	}
}
