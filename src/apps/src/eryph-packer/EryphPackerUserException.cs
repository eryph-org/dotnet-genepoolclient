namespace Eryph.Packer
{
    internal class EryphPackerUserException : Exception
    {
        public EryphPackerUserException(string message) : base(message)
        {
        }

        public EryphPackerUserException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
