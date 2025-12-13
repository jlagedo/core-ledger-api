namespace coreledger.model
{
    public class ToDo
    {
        public int Id { get; set; } 
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}