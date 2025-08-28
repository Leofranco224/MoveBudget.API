using MoveBudget.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

}