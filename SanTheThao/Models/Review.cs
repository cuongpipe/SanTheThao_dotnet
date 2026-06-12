public class Review
{
    public int Id { get; set; }

    public int CourtId { get; set; }

    public int Rating { get; set; } // 1-5

    public string Comment { get; set; } = "";

    public string? UserName { get; set; }
}