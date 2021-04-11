using CryptCPTest.Crypto;
using CryptCPTest.Test;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptCPTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            CryptOptions cryptInfo = new CryptOptions();
            cryptInfo.CryptCPpath = @"c:\Users\tsapov.m\Documents\work\35411\CryptoPro\cryptcp.win32.exe";
            cryptInfo.EncryptCertificate = "413ddcd06e9c4aebe2c2ae5e76b077318639f855";    // THUMBPRINT сертификата для шифрования (сертификат партнера для шифрования TLS - без закрытого ключа)
            cryptInfo.SignCertificate = "11734753f99fc664380c5413741c80f96d801589";       // THUMBPRINT сертификата для подписи (собственный сертификат для ЭЦП - с закрытым ключом)
            cryptInfo.DecryptCertificate = "11734753f99fc664380c5413741c80f96d801589";    // THUMBPRINT сертификата для расшифровки (собственный сертификат с закрытым ключом - TLS сертификат)
            cryptInfo.UnSignCertificate = "‎413ddcd06e9c4aebe2c2ae5e76b077318639f855";     // THUMBPRINT сертификата для проверки подписи (сертификат партнера - который прислал данные, без закрытого ключа)

            /*
            string inputFilePath = @"c:\Users\tsapov.m\TEST\E1J_SC_1_20210407_0002.csv";
            await ExecuteSign(inputFilePath, cryptInfo);
            */

            
            string inputFilePath = @"c:\Users\tsapov.m\TEST\outbox_E1J_SC_1_20210407_0002_out.zip.sgn.enc";
            await ExecuteUnSign(inputFilePath, cryptInfo);
            
            Console.WriteLine("Press any key to continue");

            Console.ReadKey();
        }

        private static async Task ExecuteSign(string inputFilePath, CryptOptions cryptInfo, CancellationToken? token = null)
        {
            string logicalFilename = Path.GetFileName(inputFilePath);

            byte[] inputData = await File.ReadAllBytesAsync(inputFilePath);
            byte[] packedData = await TestCryptoProcessor.ExecuteOutput(inputData, cryptInfo, logicalFilename);
            await File.WriteAllBytesAsync(@$"c:\Users\tsapov.m\TEST\{Path.GetFileNameWithoutExtension(logicalFilename)}.zip.sgn.enc", packedData);
          }

        private static async Task ExecuteUnSign(string inputFilePath, CryptOptions cryptInfo, CancellationToken? token = null)
        {
            byte[] inputData = await File.ReadAllBytesAsync(inputFilePath);
            (byte[] unpackedData, string fileName) = await TestCryptoProcessor.ExecuteInput(inputData, cryptInfo);
            await File.WriteAllBytesAsync(@$"c:\Users\tsapov.m\TEST\{fileName}", unpackedData);
        }
    }
}
