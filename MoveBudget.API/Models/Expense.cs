public class Expense
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal Value { get; set; }
    public string Currency { get; set; } = null!;
    public DateTime Date { get; set; }

    public int UserId { get; set; }
}
