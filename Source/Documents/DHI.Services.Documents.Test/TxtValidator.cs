namespace DHI.Services.Documents.Test
{
    using System.IO;

    public class TxtValidator : BaseValidator
    {
        public TxtValidator(string pattern = @".*\.txt$") 
            : base(pattern)
        {
        }

        public override (bool validated, string message) Validate(Stream stream)
        {
            TextReader reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            return text.Contains("Hello") ? (true, null) : (false, "Document does not contain the word `Hello`!");
        }
    }
}