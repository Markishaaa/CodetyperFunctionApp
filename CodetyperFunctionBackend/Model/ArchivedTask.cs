namespace CodetyperFunctionBackend.Model
{
    internal class ArchivedTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DeniedAt { get; set; }
        public string? Reason { get; set; }
        public string CreatorId { get; set; }
        public string StaffId { get; set; }

        public ArchivedTask(string name, string description, DateTime deniedAt, string creatorId, string staffId)
        {
            Name = name;
            Description = description;
            DeniedAt = deniedAt;
            CreatorId = creatorId;
            StaffId = staffId;
        }

        public static class Fields
        {
            public const string TableName = "Archive_Tasks";
            public const string Id = "Id";
            public const string Name = "Name";
            public const string Description = "Description";
            public const string DeniedAt = "DeniedAt";
            public const string Reason = "Reason";
            public const string CreatorId = "CreatorId";
            public const string StaffId = "StaffId";
        }
    }
}
