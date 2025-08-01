using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using matrix_dotnet.Api;
using Refit;

public class MatrixApiClient : IMatrixApi {
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private string? _accessToken;

    public MatrixApiClient(HttpClient httpClient, string? accessToken = null)
    {
        _httpClient = httpClient;
        _accessToken = accessToken;
        _jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    }

    public void SetAccessToken(string token) => _accessToken = token;

    private void AddAuthHeader()
    {
        if (!string.IsNullOrEmpty(_accessToken))
        {
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
        }
    }

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/_matrix/client/v3/login", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<LoginResponse>(_jsonOptions);
    }

    public async Task<RefreshResponse> Refresh(RefreshRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/_matrix/client/v3/refresh", request, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RefreshResponse>(_jsonOptions);
    }

    public async Task<JoinedRoomsResponse> GetJoinedRooms()
    {
        AddAuthHeader();
        var response = await _httpClient.GetAsync("/_matrix/client/v3/joined_rooms");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JoinedRoomsResponse>(_jsonOptions);
    }

    public async Task<SendEventResponse> SendEvent<TEvent>(RoomID roomId, string eventType, string txnId, TEvent body) where TEvent : EventContent
    {
        AddAuthHeader();
        var url = $"/_matrix/client/v3/rooms/{roomId}/send/{eventType}/{txnId}";
        var response = await _httpClient.PutAsJsonAsync(url, body, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SendEventResponse>(_jsonOptions);
    }

    public async Task<SyncResponse> Sync(string? filter = null, string full_state = "false", SetPresence set_presence = SetPresence.offline, string? since = null, int timeout = 0)
    {
        AddAuthHeader();
        var url = $"/_matrix/client/v3/sync?full_state={full_state}&set_presence={set_presence}&timeout={timeout}";
        if (filter != null) url += $"&filter={filter}";
        if (since != null) url += $"&since={since}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<SyncResponse>(_jsonOptions);
    }

    public async Task<RoomMessagesResponse> GetRoomMessages(RoomID roomId, Dir dir, string? filter = null, string? from = null, int? limit = null, string? to = null)
    {
        AddAuthHeader();
        var url = $"/_matrix/client/v3/rooms/{roomId}/messages?dir={dir}";
        if (filter != null) url += $"&filter={filter}";
        if (from != null) url += $"&from={from}";
        if (limit.HasValue) url += $"&limit={limit.Value}";
        if (to != null) url += $"&to={to}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoomMessagesResponse>(_jsonOptions);
    }

    // Implement other methods similarly...

    // Example for DownloadMedia
    public async Task<Stream> DownloadMedia(string serverName, string mediaId, int? timeout_ms = null)
    {
        AddAuthHeader();
        var url = $"/_matrix/client/v1/media/download/{serverName}/{mediaId}";
        if (timeout_ms.HasValue) url += $"?timeout_ms={timeout_ms.Value}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

	public Task<Stream> OldDownloadMedia(string serverName, string mediaId, int? timeout_ms = null) {
		throw new NotImplementedException();
	}

	public Task<RedactResponse> Redact(EventID eventId, RoomID roomId, string txnId, RedactRequest? request) {
		throw new NotImplementedException();
	}

	public Task<RoomCreationResponse> CreateRoom(RoomCreationRequest request) {
		throw new NotImplementedException();
	}

	public Task Invite(RoomID roomID, InviteRequest request) {
		throw new NotImplementedException();
	}

	public Task<JoinResponse> Join(string roomIdOrAlias, [Body] JoinRequest request, string[]? server_name = null) {
		throw new NotImplementedException();
	}

	public Task Leave(RoomID roomId, LeaveRequest request) {
		throw new NotImplementedException();
	}

	// You can continue implementing the rest of the interface methods in a similar fashion.
}
