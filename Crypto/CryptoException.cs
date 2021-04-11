using System;

namespace CryptCPTest.Crypto
{
    public class CryptoException: Exception
    {
        public string Operation { get; private set; }
        
        public CryptoException(string operation) :
            this(operation, null)
        {

        }

        public CryptoException(string operation, Exception innerException) :
          base($"Неудалось произвести операцию \"{operation}\"", innerException)
        {
            this.Operation = operation;
        }
    }
}
