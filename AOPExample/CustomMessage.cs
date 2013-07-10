namespace AOPExample
{
    using System;

    [AttributeUsage(AttributeTargets.Method,AllowMultiple = true)]
    public class CustomMessage : Attribute
    {
        public Type ExceptionType { get; set; }
        public string Message { get; set; }
    }
}
