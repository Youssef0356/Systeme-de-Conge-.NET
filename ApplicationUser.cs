namespace testingdatabase.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public override string UserName { get; set; } = "";
    public override string Email { get; set; } = "";

    public int Sold { get; set; } = 22;
}
