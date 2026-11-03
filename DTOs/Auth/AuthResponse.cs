namespace LibraryApi.DTOs.Auth;

public record AuthResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken);
