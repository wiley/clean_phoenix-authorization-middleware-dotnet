using System.Text.Json.Serialization;

namespace DarwinAuthorization.Models.OPA
{
    public class Data
    {
        private Input input;

        public Input Input
        {
            get { return input; }
            set { input = value; }
        }
    }
}
