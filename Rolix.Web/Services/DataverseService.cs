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
        // Si le client est déjà connecté et prêt, on le retourne directement
        if (_client != null && _client.IsReady)
            return _client;

        // Récupération des infos depuis secrets.json (ou appsettings.json)
        var url = _config["Dataverse:Url"];
        var appId = _config["Dataverse:AppId"];
        var secret = _config["Dataverse:ClientSecret"];
        var user = _config["Dataverse:Username"];
        var pass = _config["Dataverse:Password"];

        string connectionString;

        // SCÉNARIO 1 : Tu as retrouvé le Secret (Idéal)
        if (!string.IsNullOrEmpty(secret))
        {
            connectionString = $"AuthType=ClientSecret;Url={url};ClientId={appId};ClientSecret={secret};";
        }
        // SCÉNARIO 2 : Tu utilises Login/Mot de passe (Ton cas actuel)
        else
        {
            // ID générique de Microsoft
            var finalAppId = !string.IsNullOrEmpty(appId) ? appId : "51f81489-12ee-4a9e-aaae-a2591f45987d";

            // MODIFICATION ICI : On passe en mode "Interactif" pour gérer la 2FA
            connectionString = $@"
                AuthType=OAuth;
                Url={url};
                Username={user};      
                ClientId={finalAppId};
                RedirectUri=http://localhost;
                LoginPrompt=Auto;
                RequireNewInstance=True";

            // NOTE : J'ai retiré "Password={pass};" volontairement.
            // Une fenêtre Microsoft va s'ouvrir au lancement du site pour te demander ton mot de passe 
            // et te permettre de valider la notif sur ton téléphone.
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