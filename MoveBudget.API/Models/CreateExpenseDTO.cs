using System.ComponentModel.DataAnnotations;

public class CreateExpenseDTO
{
    [Required, MaxLength(100)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// Categoria da despesa (ex: "Food", "Transport").
    /// </summary>
    [Required, MaxLength(50)]
    public string Category { get; set; } = null!;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Value { get; set; }

    /// <summary>
    /// Moeda da despesa (ex: "EUR").
    /// </summary>
    [Required, MaxLength(3)]
    public string Currency { get; set; } = null!;

    [Required]
    public DateTime Date { get; set; }
}
