namespace CodetyperFunctionBackend.Model
{
    internal class ArchivedSnippet
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime DeniedAt { get; set; }
        public string Reason { get; set; }
        public string LanguageName { get; set; }
        public int TaskId { get; set; }
        public string CreatorId { get; set; }
        public string StaffId { get; set; }

        public ArchivedSnippet(string content, DateTime deniedAt, string languageName, int taskId, string creatorId, string staffId)
        {
            Content = content;
            DeniedAt = deniedAt;
            Reason = languageName;
            LanguageName = languageName;
            TaskId = taskId;
            CreatorId = creatorId;
            StaffId = staffId;
        }

        public static class Fields
        {
            public const string TableName = "ArchivedSnippet";
            public const string Id = "Id";
            public const string Content = "Content";
            public const string DeniedAt = "DeniedAt";
            public const string Reason = "Reason";
            public const string LanguageName = "LanguageName";
            public const string TaskId = "TaskId";
            public const string CreatorId = "CreatorId";
            public const string StaffId = "StaffId";
        }
    }
}
