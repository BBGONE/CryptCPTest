using CryptCPTest.Crypto;
using Example.Utils;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CryptCPTest.Test
{
    public class TestCryptoProcessor
    {
        #region CryptoProcesor Factories

        public static CryptoProcessor GetCryptoProcessor(Direction direction)
        {
            CryptoProcessorBuilder builder = new CryptoProcessorBuilder();

            switch (direction)
            {
                case Direction.Output:
                    builder.AddOutputStep(GetZipFileHandler());
                    builder.AddOutputStep(GetSignFileHandler());
                    builder.AddOutputStep(GetEncryptFileHandler());
                    break;
                case Direction.Input:
                    builder.AddInputStep(GetDecryptFileHandler());
                    builder.AddInputStep(GetUnSignFileHandler());
                    builder.AddInputStep(GetUnZipFileHandler());
                    break;
            }

            return builder.Build();
        }
        #endregion

        #region Utilities

        public static string RemoveLastExtension(string fileName, string defaultExt = ".txt")
        {
            string tmp = Path.GetFileNameWithoutExtension(fileName);
            if (string.IsNullOrEmpty(Path.GetExtension(tmp)))
            {
                tmp = Path.ChangeExtension(tmp, defaultExt);
            }

            return tmp;
        }

        public static string AppendExtension(string fileName, string ext)
        {
            if (Path.GetExtension(fileName)?.Length > 0)
            {
                return $"{fileName}.{ext.TrimStart('.')}";
            }
            else
            {
                return Path.ChangeExtension(fileName, ext);
            }
        }

        #endregion


        #region Создатели обработчиков файлов

        /// <summary>
        /// Создает обработчик подписания файла
        /// </summary>
        /// <param name="outputExt">расширение выходного файла которое будет присвоено файлу в обработчике</param>
        /// <returns>Возвращает обработчик подписания файла</returns>
        public static Func<CryptoContext, Task> GetSignFileHandler(string outputExt = ".sgn")
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string outputFile = Path.ChangeExtension(inputFile, outputExt);
                    string storeType = context.IsMachineStore ? "m" : "u";
                    // string pfx = @"c:\!WORKRDP\EXPORT\tls.pfx";

                    ProcessExec shell = new ProcessExec();
                    shell.FileName = context.CryptCPpath;
                    shell.Arguments = $"-sign -thumbprint {context.SignCertificate} -nochain -norev -{storeType}My -der -strict \"{inputFile}\" \"{outputFile}\"";
                    shell.WorkingDirectory = context.WorkDirectory;

                    _ = await shell.Start();

                    context.AddTempFile(inputFile);
                    context.LastFileName = outputFile;
                }
                catch (Exception ex)
                {
                    throw new CryptoException("Sign", ex);
                }
            };

            return result;
        }

        /// <summary>
        /// Создает обработчик верификации подписанного файла
        /// </summary>
        /// <returns>Возвращает обработчик верификации подписанного файла</returns>
        public static Func<CryptoContext, Task> GetUnSignFileHandler()
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string outputFile = Path.ChangeExtension(inputFile, ".usg");
                    string storeType = context.IsMachineStore ? "m" : "u";

                    ProcessExec shell = new ProcessExec();
                    shell.FileName = context.CryptCPpath;
                    shell.Arguments = $"-verify -nochain -norev -{storeType}AddressBook \"{inputFile}\" \"{outputFile}\"";
                    shell.WorkingDirectory = context.WorkDirectory;

                    _ = await shell.Start();

                    context.AddTempFile(inputFile);
                    context.LastFileName = outputFile;
                }
                catch (Exception ex)
                {
                    throw new CryptoException("UnSign", ex);
                }
            };

            return result;
        }

        /// <summary>
        /// Создает обработчик шифрования файла
        /// </summary>
        /// <param name="outputExt">расширение выходного файла которое будет присвоено файлу в обработчике</param>
        /// <returns>Возвращает обработчик шифрования файла</returns>
        public static Func<CryptoContext, Task> GetEncryptFileHandler(string outputExt = ".enc")
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string outputFile = Path.ChangeExtension(inputFile, outputExt);
                    string storeType = context.IsMachineStore ? "m" : "u";

                    ProcessExec shell = new ProcessExec();
                    shell.FileName = context.CryptCPpath;
                    shell.Arguments = $"-encr -thumbprint {context.EncryptCertificate} -nochain -norev -{storeType} -der \"{inputFile}\" \"{outputFile}\"";
                    shell.WorkingDirectory = context.WorkDirectory;

                    _ = await shell.Start();

                    context.AddTempFile(inputFile);
                    context.LastFileName = outputFile;
                }
                catch (Exception ex)
                {
                    throw new CryptoException("Encrypt", ex);
                }
            };

            return result;
        }

        /// <summary>
        /// Создает обработчик расшифровывания файла
        /// </summary>
        /// <returns>Возвращает обработчик расшифровывания файла</returns>
        public static Func<CryptoContext, Task> GetDecryptFileHandler()
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string outputFile = Path.ChangeExtension(inputFile, ".unc");
                    string storeType = context.IsMachineStore ? "m" : "u";

                    ProcessExec shell = new ProcessExec();
                    shell.FileName = context.CryptCPpath;
                    shell.Arguments = $"-decr -thumbprint {context.DecryptCertificate} \"{inputFile}\" \"{outputFile}\"";
                    shell.WorkingDirectory = context.WorkDirectory;

                    _ = await shell.Start();

                    context.AddTempFile(inputFile);
                    context.LastFileName = outputFile;
                }
                catch(Exception ex)
                {
                    throw new CryptoException("Decrypt", ex);
                }
            };

            return result;
        }

        /// <summary>
        /// Создает обработчик архивации файла
        /// </summary>
        /// <param name="outputExt">расширение выходного файла которое будет присвоено файлу в обработчике</param>
        /// <returns>Возвращает обработчик архивации файла</returns>
        public static Func<CryptoContext, Task> GetZipFileHandler(string outputExt = ".zip")
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                await Task.CompletedTask;

                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string entryName = context.LogicalFileName ?? throw new ArgumentNullException(nameof(context.LogicalFileName));

                    string outputFile = Path.ChangeExtension(inputFile, outputExt);
                    string inputPath = Path.Combine(context.WorkDirectory, inputFile);
                    string outputPath = Path.Combine(context.WorkDirectory, outputFile);


                    using (FileStream resultStream = File.Create(outputPath))
                    using (ZipArchive archive = new ZipArchive(resultStream, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(inputPath, entryName);
                    }

                    context.AddTempFile(inputFile);
                    context.LastFileName = outputFile;
                }
                catch (Exception ex)
                {
                    throw new CryptoException("Zip", ex);
                }
            };

            return result;
        }

        /// <summary>
        /// Создает обработчик распаковки файла из zip архива
        /// </summary>
        /// <returns>Возвращает обработчик распаковки файла</returns>
        public static Func<CryptoContext, Task> GetUnZipFileHandler()
        {
            Func<CryptoContext, Task> result = async (context) =>
            {
                await Task.CompletedTask;

                try
                {
                    string inputFile = context.LastFileName ?? context.PhysicalFileName;
                    string inputPath = Path.Combine(context.WorkDirectory, inputFile);
                    string outputFile = Path.ChangeExtension(inputFile, ".xyz");
                    string outputPath = Path.GetFullPath(Path.Combine(context.WorkDirectory, outputFile));


                    using (ZipArchive archive = ZipFile.OpenRead(inputPath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries.Take(1))
                        {
                            entry.ExtractToFile(outputPath);

                            context.AddTempFile(inputFile);
                            context.LastFileName = outputFile;
                            // можно использовать и entry.FullName (это относительный путь в архиве)
                            context.SetLogicalFileName(entry.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new CryptoException("UnZip", ex);
                }
            };

            return result;
        }

        #endregion

  
        /// <summary>
        ///  Для обработки исходящего файла
        /// </summary>
        /// <param name="data">входящие нешифрованные данные</param>
        /// <param name="zippedFileName">имя файла в архиве</param>
        /// <returns></returns>
        public static async Task<byte[]> ExecuteOutput(byte[] data, CryptOptions cryptInfo, string zippedFileName)
        {
            string tempFile = Path.GetTempFileName();
            string workingDirectory = Path.GetDirectoryName(tempFile);
            string fileName = Path.GetFileName(tempFile);

            await File.WriteAllBytesAsync(tempFile, data);

            var processor = GetCryptoProcessor(Direction.Output);
            
            await using (CryptoContext context = new CryptoContext(cryptInfo, workingDirectory, fileName, Direction.Output, zippedFileName))
            {
                await processor.Execute(context);

                // получаем результат последовательность (исходные байты -> zip.sgn.enc)
                byte[] result = await File.ReadAllBytesAsync(Path.Combine(context.WorkDirectory, context.LastFileName));
                return result;
            }
        }

        /// <summary>
        /// Для обработки входящего файла
        /// </summary>
        /// <param name="data">входящие zip.sgn.enc данные</param>
        /// <returns>расшифрованные байты даннных и имя файла из архива</returns>
        public static async Task<(byte[] result, string fileName)> ExecuteInput(byte[] data, CryptOptions cryptInfo)
        {
            string tempFile = Path.GetTempFileName();
            string workingDirectory = Path.GetDirectoryName(tempFile);
            string fileName = Path.GetFileName(tempFile);

            await File.WriteAllBytesAsync(tempFile, data);

            var processor = GetCryptoProcessor(Direction.Input);

            await using (CryptoContext context = new CryptoContext(cryptInfo, workingDirectory, fileName, Direction.Input))
            {
                await processor.Execute(context);

                string logicalFileName = context.LogicalFileName;
                byte[] result = await File.ReadAllBytesAsync(Path.Combine(context.WorkDirectory, context.LastFileName));
                return (result, logicalFileName);
            }
        }
    }
}
