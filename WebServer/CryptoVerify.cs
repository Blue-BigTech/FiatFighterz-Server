using System;
using Nethereum.Signer;
using Utils.NET.Crypto;

namespace WebServer
{
	public class CryptoVerify
	{
		public int experationTime;
		public int largePrime;

		

		public static string GenerateMessage(AES aes)
		{
            var randomGenerator = new Random();
			string randomstring = (randomGenerator.Next(1, 999) * aes.randomNum).ToString() + "_FIAT_" + DateTime.UtcNow;
			Console.WriteLine(randomstring);
			return aes.Encrypt( randomstring );
		}
	
		public static string VerifySignedMessage(string msg, string signedMessage)
		{
			var signer = new EthereumMessageSigner();
			var address = signer.EncodeUTF8AndEcRecover(msg, signedMessage);
			return address;
		}

		public static bool ValidateMessageTime(AES aes, string message)
        {
			string decryptedMessage = "";
			if (string.IsNullOrWhiteSpace(message)){ return false; }
			try
            {
				decryptedMessage = aes.Decrypt(message);
				if (decryptedMessage.Contains("_FIAT_"))
                {
					float randomNum = float.Parse(decryptedMessage.Split("_FIAT_")[0]);
					if (randomNum % aes.randomNum == 0 && randomNum / aes.randomNum <= 999)
                    {
						Console.WriteLine(randomNum % aes.randomNum);
						Console.WriteLine(randomNum / aes.randomNum);
						DateTime dt = DateTime.Parse(decryptedMessage.Split("_FIAT_")[1]);
						Console.WriteLine(DateTime.UtcNow + " datetime " + dt );
					}
				}


			}catch
            {
				return false;
			}
			return false;
		}

	}
}

