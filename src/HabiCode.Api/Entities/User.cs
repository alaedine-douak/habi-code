namespace HabiCode.Api.Entities;

public sealed class User
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAtUTC { get; set; }
    public DateTime? UpdatedAtUTC { get; set; }


    /// <summary>
    /// We'll use this to store the IdentidyId from Identidy Provider
    /// This could be any identity provider like Azure AD, Cognito, KeyClock, Auth0, etc.
    /// </summary>
    public string IdentityId { get; set; }
}
