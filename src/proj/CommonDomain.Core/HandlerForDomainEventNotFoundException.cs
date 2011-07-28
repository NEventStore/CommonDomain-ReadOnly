namespace CommonDomain.Core
{
    using System;

    public class HandlerForDomainEventNotFoundException : Exception
    {
        public HandlerForDomainEventNotFoundException(string message)
            : base(message)
        {
        }
    }
}