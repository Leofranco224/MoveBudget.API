/// <summary>
/// Filtros opcionais para buscar despesas.
/// </summary>
public class ExpenseFilterDTO
{
    /// <summary>
    /// Filtra despesas pela categoria (ex: "Food").
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filtra despesas pela moeda (ex: "EUR").
    /// </summary>
    public string? Currency { get; set; }

    /// <summary>
    /// Data mínima para filtrar despesas.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Data máxima para filtrar despesas.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Campo para ordenação: "value" ou "date".
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Direção da ordenação: "asc" ou "desc".
    /// </summary>
    public string? Order { get; set; }
}