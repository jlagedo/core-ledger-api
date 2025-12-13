namespace core_ledger_api.Dtos
{
    public class ToDoDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
