using System;
using System.Linq;
using System.Text;
using Chaos.NaCl;
using IoTApiMock.DTO;
using IoTApiMock.Exceptions;

namespace IoTApiMock.Services
{
    public static class Token
    {
        private static readonly string _separator = ";";

        public static (byte[] privateKey, byte[] publicKey) CreateKeyPairs()
        {
            Random random = new Random();
            var byteArray = new byte[32];
            random.NextBytes(byteArray);
            return (Ed25519.ExpandedPrivateKeyFromSeed(byteArray), Ed25519.PublicKeyFromSeed(byteArray));
        }

        public static byte[] CreateToken(string identification, long number, byte[] privateKey)
        {
            if (identification.Contains(";"))
            {
                throw new InvalidTokenException("The provided token has not the expected format");
            }

            var encodedData = Encode(identification, number);

            var signature = Ed25519.Sign(encodedData, privateKey);
            var mergedArray = new byte[encodedData.Length + signature.Length];
            Buffer.BlockCopy(encodedData, 0, mergedArray, 0, encodedData.Length);
            Buffer.BlockCopy(signature, 0, mergedArray, encodedData.Length, signature.Length);
            return mergedArray;
        }

        public static TokenDataDto DecodeAndVerifyToken(string token, byte[] publicKey)
        {
            byte[] message;
            try
            {
                message = Convert.FromBase64String(token);
            }
            catch
            {
                throw new InvalidTokenException("The token is in a wrong format");
            }

            var tokenData = Decode(message);
            var signature = message.Reverse().Take(64).Reverse().ToArray();
            var datapayload = message.Take(message.Length - 64).ToArray();
            if (!Ed25519.Verify(signature, datapayload, publicKey))
            {
                throw new InvalidTokenException("The provided token has not a valid signature");
            }

            return tokenData;
        }

        private static byte[] Encode(string identification, long number, int? time = null)
        {

            if (time == null)
            {
                time = (int)new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds(); 
            }

            return Encoding.ASCII.GetBytes($"{identification}{_separator}{number}{_separator}{time}{_separator}");
        }

        private static TokenDataDto Decode(byte[] token)
        {
            var decodedString = Encoding.ASCII.GetString(token).Split(_separator);
            if (decodedString.Length < 4)
            {
                throw new InvalidTokenException("The provided token have an wrong format");
            }

            return new TokenDataDto()
            {
                Identification = decodedString[0],
                Count = Convert.ToInt64(decodedString[1]),
                UnixTimeStamp = Convert.ToInt32(decodedString[2])
            };
        }
    }
}
