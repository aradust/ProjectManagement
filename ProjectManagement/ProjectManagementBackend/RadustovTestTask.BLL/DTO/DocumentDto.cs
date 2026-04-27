namespace RadustovTestTask.BLL.DTO
{
    public class DocumentDto
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string OriginalName { get; set; }
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}