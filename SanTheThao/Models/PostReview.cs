public class PostReview
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = "";

    public string? UserName { get; set; }
}