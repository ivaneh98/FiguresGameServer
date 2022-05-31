using Microsoft.Data.Sqlite;

namespace FiguresGameServer
{
    public class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string str = _packet.ReadString();
            string[] auth = str.Split('|');

            string _state = auth[0];
            string _username = auth[1];
            string _password = auth[2];
            string sqlExpression;
            SqliteCommand command;
            switch (_state)
            {
                case "auth":
                    sqlExpression = $"SELECT * FROM Users where login = @username and password = @password";
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _username);
                        command.Parameters.AddWithValue("@password", _password);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                Console.WriteLine($"login: {_username} password: {_password}");
                                for (int i = 1; i <= Server.MaxPlayers; i++)
                                {
                                    if (Server.clients[i].tcp.GetLogin() == _username)
                                    {
                                        ServerSend.Command(_fromClient, "PlayerAuthorizedAlready");
                                        Console.WriteLine("This player already on server");
                                        return;
                                    }
                                }
                                string _command = "AuthSuccess";
                                ServerSend.Command(_fromClient, _command);
                                Server.clients[_fromClient].tcp.SetLogin(_username);
                            }
                            else
                            {
                                ServerSend.Command(_fromClient, "WrongLogPass");

                                Server.clients[_fromClient].tcp.socket.Close();
                                Server.clients[_fromClient].tcp.socket = null;
                            }
                        }
                    }
                    break;

                case "reg":
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        sqlExpression = $"SELECT * FROM Users where login = @username";

                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _username);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                ServerSend.Command(_fromClient, "UserAlreadyExist");
                                return;
                            }
                        }
                        if (_username != "" && _password != "")
                        {
                            command = new SqliteCommand();
                            command.Connection = connection;
                            command.CommandText = $"INSERT INTO Users (login, password) VALUES (@username, @password)";
                            command.Parameters.AddWithValue("@username", _username);
                            command.Parameters.AddWithValue("@password", _password);
                            int number = command.ExecuteNonQuery();
                            string _command = "RegistrationSuccess";
                            ServerSend.Command(_fromClient, _command);
                            Console.WriteLine($"В таблицу Users добавлено объектов: {number}");

                            DateTime last_visit = DateTime.Now;
                            TimeSpan oneDay = new TimeSpan(1, 0, 0, 0);
                            last_visit = last_visit.Subtract(oneDay);
                            string format = "yyyy/MM/dd hh:mm:ss";
                            Console.WriteLine(last_visit.ToString(format));
                            last_visit = Utilites.ConvertToDateTime(last_visit.ToString(format));
                            var money = 0;
                            var wins = 0;
                            var losses = 0;
                            var highscore = 0;

                            sqlExpression = $"INSERT INTO UsersData (login, last_visit, money, wins, losses, highscore)" +
                                $" VALUES (@username, @last_visit, @money, @wins, @losses, @highscore)";
                            command = new SqliteCommand(sqlExpression, connection);
                            command.Parameters.AddWithValue("@username", _username);
                            command.Parameters.AddWithValue("@last_visit", last_visit);
                            command.Parameters.AddWithValue("@money", money);
                            command.Parameters.AddWithValue("@wins", wins);
                            command.Parameters.AddWithValue("@losses", losses);
                            command.Parameters.AddWithValue("@highscore", highscore);
                            command.ExecuteNonQuery();
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void CommandReceived(int _fromClient, Packet _packet)
        {
            string sqlExpression;
            SqliteCommand command;
            if (Server.clients[_fromClient].tcp.socket == null)
                return;
            int _clientIdCheck = _packet.ReadInt();
            string[] _command = _packet.ReadString().Split('|');
            Console.WriteLine($"id: {_fromClient}; command: {_command[0]}");
            switch (_command[0])
            {
                case "FindEnemy":
                    Player player = new Player(Server.clients[_fromClient], _command[1]);
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {

                        if (Server.rooms[i].Open(player))
                        {
                            break;
                        }
                        else if (Server.rooms[i].Connect(player))
                        {
                            break;
                        }
                    }
                    break;
                case "PlayerReady":
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {
                        if (Server.rooms[i].PlayerReady(_fromClient))
                        {
                            break;
                        }
                    }
                    break;
                case "AddPoint":
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {
                        if (Server.rooms[i].AddPoint(_fromClient))
                        {
                            break;
                        }
                    }
                    break;
                case "AddHealth":
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {
                        if (Server.rooms[i].AddHealth(_fromClient))
                        {
                            break;
                        }
                    }
                    break;
                case "RemoveHealth":
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {
                        if (Server.rooms[i].RemoveHealth(_fromClient))
                        {
                            break;
                        }
                    }
                    break;
                case "DontLogoutMe":
                    Server.clients[_fromClient].tcp.Connected();
                    break;
                case "CloseRoom":
                    for (int i = 1; i <= Server.MaxRooms; i++)
                    {
                        if (Server.rooms[i].CloseRoom(_fromClient))
                        {
                            break;
                        }
                    }
                    break;
                case "GetLeaderboard":
                    sqlExpression = $"SELECT login, highscore FROM UsersData";
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                Dictionary<string, int> leaders = new Dictionary<string, int>();
                                string leaderscore = "";

                                while (reader.Read())
                                {
                                    leaders.Add((string)reader.GetValue(0), Convert.ToInt32(reader.GetValue(1)));
                                }
                                int count = 0;
                                foreach (var leader in leaders.OrderByDescending(key => key.Value))
                                {
                                    leaderscore += $"{leader.Key}~{leader.Value}~";
                                    count++;
                                    if (count >= 10)
                                    {
                                        break;
                                    }
                                }
                                leaderscore = leaderscore.Remove(leaderscore.Length - 1);
                                ServerSend.Command(_clientIdCheck, $"LeaderboardSuccess|{leaderscore}");

                            }
                        }
                    }
                    break;
                case "GetDaily":
                    int reward = 1000;
                    sqlExpression = $"SELECT * FROM UsersData where login = @username";

                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _command[1]);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                var lastVisit = DateTime.Now;
                                var money = reward;

                                sqlExpression = $"UPDATE UsersData SET last_visit = '{lastVisit}', money = '{money}' where login = @username";
                                command = new SqliteCommand(sqlExpression, connection);
                                command.Parameters.AddWithValue("@username", _command[1]);
                                command.ExecuteNonQuery();
                                ServerSend.Command(_fromClient, $"DaityRewardSuccess|{money}");
                            }
                        }
                    }
                    break;
                case "SetHighscore":
                    sqlExpression = $"SELECT * FROM UsersData where login = @username";
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _command[1]);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                sqlExpression = $"UPDATE UsersData SET highscore = '{_command[2]}' where login = @username";
                                command = new SqliteCommand(sqlExpression, connection);
                                command.Parameters.AddWithValue("@username", _command[1]);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    break;
                case "AddMoney":
                    sqlExpression = $"SELECT * FROM UsersData where login = @username";
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _command[1]);
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                reader.Read();
                                int money = reader.GetInt32(2) + int.Parse(_command[2]);
                                sqlExpression = $"UPDATE UsersData SET money = '{money}' where login = @username";
                                command = new SqliteCommand(sqlExpression, connection);
                                command.Parameters.AddWithValue("@username", _command[1]);
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                    break;
                case "GetData":
                    sqlExpression = $"SELECT * FROM UsersData where login = @username";
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _command[1]);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                reader.Read();
                                var lastVisit = reader.GetValue(1);

                                var money = reader.GetValue(2);
                                var wins = reader.GetValue(3);
                                var losses = reader.GetValue(4);
                                var highscore = reader.GetValue(5);
                                ServerSend.Command(_clientIdCheck, $"DataSuccess|{lastVisit}|{money}|{wins}|{losses}|{highscore}");

                            }
                        }
                    }
                    break;
                case "Respawn":
                    sqlExpression = $"SELECT money FROM UsersData where login = @username";
                    int respawnPrice = 100;
                    using (var connection = new SqliteConnection(Constants.connectionString))
                    {
                        connection.Open();
                        command = new SqliteCommand(sqlExpression, connection);
                        command.Parameters.AddWithValue("@username", _command[1]);

                        using (SqliteDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows) // если есть данные
                            {
                                reader.Read();
                                var money = reader.GetInt32(0);

                                if (money >= respawnPrice)
                                {
                                    money -= respawnPrice;

                                    sqlExpression = $"UPDATE UsersData SET money = '{money}' where login = @username";
                                    command = new SqliteCommand(sqlExpression, connection);
                                    command.Parameters.AddWithValue("@username", _command[1]);
                                    command.ExecuteNonQuery();

                                    ServerSend.Command(_fromClient, $"RespawnSuccess");
                                }
                                else
                                {
                                    ServerSend.Command(_fromClient, "RespawnNotEnough");
                                }

                            }
                        }
                    }
                    break;
            }
        }
    }
}
