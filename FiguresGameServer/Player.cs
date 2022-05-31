namespace FiguresGameServer
{
    public class Player
    {
        public int score;
        private int _lifes = 1;
        public int lifes
        {
            get { return _lifes; }
            set
            {
                _lifes = value;
                if (_lifes <= 0)
                {
                    _lifes = 0;
                    isLoss = true;
                }
            }
        }
        public bool isLoss;
        public string login;
        public Client client;
        public Player(Client _client, string _login)
        {
            score = 0;
            lifes = 5;
            isLoss = false;
            login = _login;
            client = _client;
        }
    }
}
