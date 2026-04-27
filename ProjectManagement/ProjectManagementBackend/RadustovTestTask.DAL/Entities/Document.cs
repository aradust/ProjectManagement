namespace RadustovTestTask.DAL.Entities
{
    public class Document
    {
        public long Id { get; set; }

        public string FileName { get; set; }
        public string OriginalName { get; set; }
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public long ProjectId { get; set; }
        public Project Project { get; set; }
    }
}