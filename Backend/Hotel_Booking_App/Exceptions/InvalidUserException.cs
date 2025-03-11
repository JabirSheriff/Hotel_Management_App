namespace Hotel_Booking_App.Exceptions
{
    public class InvalidUserException : Exception
    {
        private string message;
        public InvalidUserException()
        {
            message = "Invalid username or password";
        }
        public InvalidUserException(string message) : base(message)
        {
            this.message = message;
        }
        public override string Message => message;
    }
}
