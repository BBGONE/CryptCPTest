using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptCPTest.Crypto
{
    public class CryptoProcessorBuilder
    {
        private List<Func<CryptoContext, Task>> inputLine { get; set; }
        private List<Func<CryptoContext, Task>> outputLine { get; set; }

        public CryptoProcessorBuilder()
        {
            inputLine = new List<Func<CryptoContext, Task>>();
            outputLine = new List<Func<CryptoContext, Task>>();
        }

        /// <summary>
        /// Добавление шагов обработки входящего файла - добавляются в порядке обработки входящего сообщения
        /// </summary>
        /// <param name="inputFunc"></param>
        public void AddInputStep(Func<CryptoContext, Task> inputFunc)
        {
            inputLine.Add(inputFunc);
        }

        /// <summary>
        /// Добавление шагов обработки исходящего файла - добавляются в порядке обработки исходящего сообщения
        /// </summary>
        /// <param name="outputFunc"></param>
        public void AddOutputStep(Func<CryptoContext, Task> outputFunc)
        {
            outputLine.Add(outputFunc);
        }

        /// <summary>
        /// Создать CryptoProcessor
        /// </summary>
        /// <returns></returns>
        public CryptoProcessor Build()
        {
            CryptoProcessor cryptoProcessor = new CryptoProcessor(inputLine.ToArray(), outputLine.ToArray());
            return cryptoProcessor;
        }
    }
}
