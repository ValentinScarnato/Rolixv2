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
        // Si le client est déjà connecté, on le retourne directement
        if (_client != null && _client.IsReady)
            return _client;

        // Récupération des infos depuis secrets.json (fichier config)
        var url = _config["Dataverse:Url"];
        var appId = _config["Dataverse:AppId"];
        var secret = _config["Dataverse:ClientSecret"];
        var user = _config["Dataverse:Username"];
        var pass = _config["Dataverse:Password"];

        string connectionString;

        
        if (!string.IsNullOrEmpty(secret))
        {
            connectionString = $"AuthType=ClientSecret;Url={url};ClientId={appId};ClientSecret={secret};";
        }
        
        else
        {
            var finalAppId = !string.IsNullOrEmpty(appId) ? appId : "51f81489-12ee-4a9e-aaae-a2591f45987d";

            // A2F
            connectionString = $@"
                AuthType=OAuth;
                Url={url};
                Username={user};      
                ClientId={finalAppId};
                RedirectUri=http://localhost;
                LoginPrompt=Auto;
                RequireNewInstance=True";
        }

        try
        {
            _client = new ServiceClient(connectionString);

            if (!_client.IsReady)
            {
                // Permet de voir l'erreur exacte si la connexion échoue
                throw new Exception($"Dataverse non connecté : {_client.LastError}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Erreur critique de connexion Dataverse. Vérifiez vos identifiants dans secrets.json.", ex);
        }

        return _client;
    }
}