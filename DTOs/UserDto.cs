using System.ComponentModel.DataAnnotations;

namespace UserApi.DTOs;

public class CreateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [RegularExpression(@".*\S.*", ErrorMessage = "Name cannot be whitespace only.")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Bio { get; set; }
}

public class UpdateUserDto
{
    [StringLength(100)]
    public string? Name { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
