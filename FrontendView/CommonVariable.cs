public class CommonVariable
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CommonVariable(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId()
    {
        return _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
    }

    public string UserName()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? "Guest";
    }

    public void StoreUserName(string userName)
    {
        _httpContextAccessor.HttpContext?.Session.SetString("UserName", userName);
    }

    public string Token()
    {
        return _httpContextAccessor.HttpContext?.Session.GetString("Token");
    }
}
