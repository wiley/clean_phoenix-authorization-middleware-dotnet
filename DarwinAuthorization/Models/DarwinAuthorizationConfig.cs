using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace DarwinAuthorization.Models
{
    public class DarwinAuthorizationConfig
    {
        public bool DevEnv { get; set; } = false;

        private string _keyCloakBaseUrl;

        public string KeyCloakBaseUrl
        {
            get { return _keyCloakBaseUrl; }
            set { _keyCloakBaseUrl = value; }
        }

        private string _keyCloakRealm;

        public string KeyCloakRealm
        {
            get { return _keyCloakRealm; }
            set { _keyCloakRealm = value; }
        }

        private string _keyCloakAudience;

        public string KeyCloakAudience
        {
            get { return _keyCloakAudience; }
            set { _keyCloakAudience = value; }
        }

        private string _opaBaseUrl;

        public string OpaBaseUrl
        {
            get { return _opaBaseUrl; }
            set { _opaBaseUrl = value; }
        }

        private string _serviceApiKey;

        public string ServiceApiKey
        {
            get { return _serviceApiKey; }
            set { _serviceApiKey = value; }
        }
    }
}
