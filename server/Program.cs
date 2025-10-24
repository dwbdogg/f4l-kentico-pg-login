using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// CORS for Kentico domain if hosting API separately
app.Use(async (ctx, next) => {
    ctx.Response.Headers["Access-Control-Allow-Origin"] = ctx.Request.Headers.Origin;
    ctx.Response.Headers["Access-Control-Allow-Credentials"] = "true";
    ctx.Response.Headers["Access-Control-Allow-Headers"] = "content-type";
    if (ctx.Request.Method == "OPTIONS") { ctx.Response.StatusCode = 204; return; }
    await next();
});

var http = new HttpClient();
string PG_BASE = Environment.GetEnvironmentVariable("PG_BASE_URL") ?? "https://fit4less.perfectgym.pl";
string PG_CLIENT_ID = Environment.GetEnvironmentVariable("PG_CLIENT_ID") ?? "CHANGE_ME";
string PG_CLIENT_SECRET = Environment.GetEnvironmentVariable("PG_CLIENT_SECRET") ?? "CHANGE_ME";

// Helper to add required PG headers
HttpRequestMessage WithPgHeaders(HttpRequestMessage req) {
    req.Headers.Add("X-Client-Id", PG_CLIENT_ID);
    req.Headers.Add("X-Client-Secret", PG_CLIENT_SECRET);
    return req;
}

// Demo cookie session (httpOnly)
app.MapPost("/api/login", async (HttpContext ctx) => {
    using var sr = new StreamReader(ctx.Request.Body);
    var body = await sr.ReadToEndAsync();
    var doc = JsonDocument.Parse(body);
    string email = doc.RootElement.GetProperty("email").GetString()!;
    string password = doc.RootElement.GetProperty("password").GetString()!;

    // Replace path below with the real PerfectGym login endpoint for your tenant.
    // This might be something like /Api/v2/Customers/Login or using the Client Portal auth endpoint.
    var loginUrl = $"{PG_BASE}/Api/v2.2/customers/login";
    var payload = JsonSerializer.Serialize(new { email, password });
    var req = WithPgHeaders(new HttpRequestMessage(HttpMethod.Post, loginUrl) {
        Content = new StringContent(payload, Encoding.UTF8, "application/json")
    });
    var res = await http.SendAsync(req);
    var text = await res.Content.ReadAsStringAsync();

    if (!res.IsSuccessStatusCode) {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsJsonAsync(new { error = "Login failed", details = text });
        return;
    }

    // Expect a token or cookie; adapt to actual API response shape
    var token = JsonDocument.Parse(text).RootElement.GetProperty("token").GetString();
    ctx.Response.Cookies.Append("pg_token", token!, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Path = "/" });
    await ctx.Response.WriteAsJsonAsync(new { ok = true });
});

app.MapGet("/api/me", async (HttpContext ctx) => {
    if (!ctx.Request.Cookies.TryGetValue("pg_token", out var token)) {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsJsonAsync(new { error = "Not authenticated" });
        return;
    }
    // Example call to a 'current user' endpoint; replace with real one
    var meUrl = $"{PG_BASE}/Api/v2.2/customers/me";
    var req = WithPgHeaders(new HttpRequestMessage(HttpMethod.Get, meUrl));
    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    var res = await http.SendAsync(req);
    var text = await res.Content.ReadAsStringAsync();
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsync(text);
});

app.MapPost("/api/logout", (HttpContext ctx) => {
    if (ctx.Request.Cookies.ContainsKey("pg_token")) {
        ctx.Response.Cookies.Delete("pg_token", new CookieOptions{ Path = "/" });
    }
    return Results.Json(new { ok = true });
});

app.Run();
