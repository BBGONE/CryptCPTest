using CryptCPTest.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CryptCPTest.Crypto
{
    public sealed class CryptoContext:IDisposable, IAsyncDisposable
    {
        private readonly List<string> tempFiles = new List<string>();

        public bool IsMachineStore { get; private set; }

        /// <summary>
        /// Имя файла на диске (физическое имя файла)
        /// </summary>
        public string PhysicalFileName { get; private set; }

        /// <summary>
        /// Имя файла которое должно быть в архиве (логическое имя файла)
        /// </summary>
        public string LogicalFileName { get; private set; }

        /// <summary>
        /// Рабочая директория
        /// </summary>
        public string WorkDirectory { get; private set; }

        /// <summary>
        /// Направление обработки файла (входящий или исходящий)
        /// </summary>
        public Direction Direction { get; private set; }

        /// <summary>
        /// Имя последнего обработанного файла (физическое имя файла)
        /// как правило присваивается на каждом этапе обработке (внутри обработчика)
        /// </summary>
        public string LastFileName { get; set; }

        /// <summary>
        /// Путь к исполняемому файлу CryptCp.exe
        /// </summary>
        public string CryptCPpath { get; private set; }

        public string SignCertificate { get; private set; }
        
        public string UnSignCertificate { get; private set; }

        public string EncryptCertificate { get; private set; }

        public string DecryptCertificate { get; private set; }

        /// <summary>
        /// Добавление имен файлов, которые потребуется удалить в конце обработки
        /// </summary>
        /// <param name="fileName">Физическое имя файла (на диске)</param>
        public void AddTempFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (!tempFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            {
                tempFiles.Add(fileName);
            }
        }

        public void SetLogicalFileName(string name)
        {
            this.LogicalFileName = name;
        }

        /// <summary>
        /// Конструктор создания контекста
        /// </summary>
        /// <param name="workDirectory">рабочая директория</param>
        /// <param name="physicalFilename">Имя файла на диске (физическое имя файла)</param>
        /// <param name="direction">Направление обработки файла (входящий или исходящий)</param>
        /// <param name="logicalFileName">Имя файла которое должно быть в архиве (логическое имя файла)</param>
        public CryptoContext(CryptOptions cryptInfo, string workDirectory, string physicalFilename, Direction direction, string logicalFileName = null)
        {
            this.IsMachineStore = cryptInfo.IsMachineStore;
            this.CryptCPpath = cryptInfo.CryptCPpath;
            this.EncryptCertificate = cryptInfo.EncryptCertificate;     // THUMBPRINT сертификата для шифрования (сертификат партнера для шифрования TLS - без закрытого ключа)
            this.SignCertificate = cryptInfo.SignCertificate;           // THUMBPRINT сертификата для подписи (собственный сертификат для ЭЦП - с закрытым ключом)
            this.DecryptCertificate = cryptInfo.DecryptCertificate;     // THUMBPRINT сертификата для расшифровки (собственный сертификат с закрытым ключом - TLS сертификат)
            this.UnSignCertificate = cryptInfo.UnSignCertificate;       // THUMBPRINT сертификата для проверки подписи (сертификат партнера - который прислал данные, без закрытого ключа)
            this.WorkDirectory = workDirectory;
            this.PhysicalFileName = physicalFilename;
            this.Direction = direction;
            this.LogicalFileName = logicalFileName;
        }

        public void Dispose()
        {
            this.DisposeAsync().GetAwaiter().GetResult();
        }

        public async ValueTask DisposeAsync()
        {
            AddTempFile(LastFileName);
            var files = tempFiles.Select(f=> Path.Combine(this.WorkDirectory, f)).ToArray();
            tempFiles.Clear();

            await Retry.Do(() =>
            {
                foreach (string filePath in files)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                    catch
                    {

                    }
                }
            }, TimeSpan.FromMilliseconds(50));
        }
    }
}
