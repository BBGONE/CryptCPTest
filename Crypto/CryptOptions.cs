namespace CryptCPTest.Crypto
{
    public class CryptOptions
    {
        public bool IsMachineStore { get; set; }

        /// <summary>
        /// Путь к исполняемому файлу CryptCp.exe
        /// </summary>
        public string CryptCPpath { get; set; }

        public string SignCertificate { get; set; }

        public string UnSignCertificate { get; set; }

        public string EncryptCertificate { get; set; }

        public string DecryptCertificate { get; set; }
    }
}
