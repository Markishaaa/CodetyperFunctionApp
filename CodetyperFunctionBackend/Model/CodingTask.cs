namespace CodetyperFunctionBackend.Model
{
    internal class CodingTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Shown { get; set; }
        public string CreatorId { get; set; }
        public List<CodeSnippet> CodeSnippets { get; set; }

        public CodingTask()
        {
            CodeSnippets = new List<CodeSnippet>();
        }
    }
}
