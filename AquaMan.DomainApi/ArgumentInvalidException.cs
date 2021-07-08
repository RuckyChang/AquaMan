using System;
namespace AquaMan.DomainApi
{
    public class ArgumentInvalidException : Exception
    {
        public string Field { get; }
        public ArgumentInvalidException(string field) : base()
        {
            Field = field;
        }
    }
}
