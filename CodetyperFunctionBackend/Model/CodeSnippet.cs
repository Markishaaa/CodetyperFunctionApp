namespace CodetyperFunctionBackend.Model
{
    internal class CodeSnippet
    {
        public int Id { get; set; } 
        public string Content { get; set; }
        public bool Shown { get; set; }
        public string LanguageName { get; set; }
        public int TaskId { get; set; }
        public string CreatorId { get; set; }
    }
}
