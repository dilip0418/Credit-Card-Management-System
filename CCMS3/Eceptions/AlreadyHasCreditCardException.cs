namespace CCMS3.Eceptions
{
    public class AlreadyHasCreditCardException : Exception
    {
        public AlreadyHasCreditCardException()
        {
        }

        public AlreadyHasCreditCardException(string? message) : base(message)
        {
        }
    }
}
