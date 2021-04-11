using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptCPTest.Crypto
{
    public class CryptoProcessor
    {
        private IEnumerable<Func<CryptoContext, Task>> inputPipeline { get;  }
        private IEnumerable<Func<CryptoContext, Task>> outputPipeline { get; }

        internal CryptoProcessor(IEnumerable<Func<CryptoContext, Task>> inputPipeline, IEnumerable<Func<CryptoContext, Task>> outputPipeline)
        {
            this.inputPipeline = inputPipeline;
            this.outputPipeline = outputPipeline;
        }

        public async Task Execute(CryptoContext context)
        {
            switch (context.Direction)
            {
                case Direction.Input:
                    await ProcessInput(context);
                    break;
                case Direction.Output:
                    await ProcessOutput(context);
                    break;
            }
        }

        protected virtual async Task ProcessInput(CryptoContext context)
        {
            foreach (var step in inputPipeline)
            {
                await step(context);
            }
        }

        protected virtual async Task ProcessOutput(CryptoContext context)
        {
            foreach (var step in outputPipeline)
            {
                await step(context);
            }
        }
    }
}
