using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rolix.Web.Services;

public class DataverseService
{
    private readonly IConfiguration _config;
    private ServiceClient? _client;

    public DataverseService(IConfiguration config)
    {
        _config = config;
    }

    public ServiceClient GetClient()
    {
        if (_client != null && _client.IsReady)
            return _client;

        var url = _config["Dataverse:Url"];
        var appId = _config["Dataverse:AppId"];
        var redirectUri = _config["Dataverse:RedirectUri"];
        var loginPrompt = _config["Dataverse:LoginPrompt"] ?? "Auto";

        var connectionString =
            $"AuthType=OAuth;" +
            $"Url={url};" +
            $"AppId={appId};" +
            $"RedirectUri={redirectUri};" +
            $"LoginPrompt={loginPrompt};";

        _client = new ServiceClient(connectionString);
        return _client;
    }
}
