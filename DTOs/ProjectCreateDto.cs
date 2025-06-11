namespace TaskTrackerMinimalAPI.DTOs
{
    public class ProjectCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
