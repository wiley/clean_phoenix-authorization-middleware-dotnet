namespace DarwinAuthorization.Models
{
    public class DarwinAuthorizationContext
    {
        private int _userId;

        public int UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        private bool _hasJwt;

        public bool HasJwt
        {
            get { return _hasJwt; }
            set { _hasJwt = value; }
        }

        private bool _hastApiKey;

        public bool HasApiKey
        {
            get { return _hastApiKey; }
            set { _hastApiKey = value; }
        }

        private Guid _transactionID;

        public Guid TransactionID
        {
            get { return _transactionID; }
            set { _transactionID = value; }
        }

        private int? _authData;

        public int? AuthData
        {
            get { return _authData; }
            set { _authData = value; }
        }

    }
}
