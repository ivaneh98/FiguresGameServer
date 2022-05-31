using Microsoft.Data.Sqlite;
using System.Timers;

namespace FiguresGameServer
{
    public class Room
    {
        Player player;
        Player player2;
        int id;
        public bool isFull = false;
        public bool isEmpty = true;
        private System.Timers.Timer aTimer;
        private int readyPlayers = 0;
        private int count = 0;
        private bool isStart = false;
        private string winnerLogin = "";
        private string loserLogin = "";
        public Room(int _roomID)
        {
            id = _roomID;
        }
        public bool Open(Player _player)
        {
            if (isEmpty)
            {
                player = _player;
                isEmpty = false;

                ServerSend.Command(player.client.id, "FindingEnemy");
                Console.WriteLine("Room was opened");
                return true;

            }
            else
            {
                Console.WriteLine("Failed to open room");
                return false;
            }
        }
        public bool TechnicalLoss(int id)
        {
            if (!isEmpty && !isFull)
            {
                if (player.client.id == id)
                {
                    Close();
                    return true;
                }
            }
            else if (isFull)
            {
                if (player.client.id == id)
                {
                    winnerLogin = player2.login;
                    loserLogin = player.login;
                    ServerSend.Command(player.client.id, $"MatchResult|loss|{player.score + player2.score}");
                    ServerSend.Command(player2.client.id, $"MatchResult|win|{player.score + player2.score}");
                    Close();
                }
                if (player2.client.id == id)
                {
                    winnerLogin = player.login;
                    loserLogin = player2.login;
                    ServerSend.Command(player.client.id, $"MatchResult|win|{player.score + player2.score}");
                    ServerSend.Command(player2.client.id, $"MatchResult|loss|{player.score + player2.score}");
                    Close();
                }
            }
            return false;
        }
        public bool Connect(Player _player2)
        {
            if (!isFull)
            {
                player2 = _player2;
                isFull = true;
                Console.WriteLine("Player connected success");
                Start();
                return true;
            }
            else
            {
                Console.WriteLine("Room is full");
                return false;
            }
        }
        public bool PlayerReady(int id)
        {
            if (player.client.id == id || player2.client.id == id)
            {

                readyPlayers++;
                if (readyPlayers == 2)
                {
                    GenerateFigure();
                    Ready();
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool CloseRoom(int id)
        {
            if (player.client.id == id)
            {
                Close();
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool AddPoint(int id)
        {
            if (player.client.id == id || player2.client.id == id)
            {
                if (player.client.id == id)
                {
                    ServerSend.Command(player2.client.id, $"EnemyScore|{player.login}|{player.score}|{player.lifes}");
                    player.score++;
                }
                if (player2.client.id == id)
                {
                    ServerSend.Command(player.client.id, $"EnemyScore|{player2.login}|{player2.score}|{player2.lifes}");
                    player2.score++;
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool RemoveHealth(int id)
        {
            if (player.client.id == id || player2.client.id == id)
            {
                if (player.client.id == id)
                {
                    player.lifes--;
                    ServerSend.Command(player2.client.id, $"EnemyScore|{player.login}|{player.score}|{player.lifes}");
                    if (player.isLoss)
                    {
                        winnerLogin = player2.login;
                        loserLogin = player.login;
                        ServerSend.Command(player.client.id, $"MatchResult|loss|{player.score + player2.score}");
                        ServerSend.Command(player2.client.id, $"MatchResult|win|{player.score + player2.score}");
                        Close();
                    }
                }
                if (player2.client.id == id)
                {
                    player2.lifes--;
                    ServerSend.Command(player.client.id, $"EnemyScore|{player2.login}|{player2.score}|{player2.lifes}");
                    if (player2.isLoss)
                    {
                        winnerLogin = player.login;
                        loserLogin = player2.login;
                        ServerSend.Command(player.client.id, $"MatchResult|win|{player.score + player2.score}");
                        ServerSend.Command(player2.client.id, $"MatchResult|loss|{player.score + player2.score}");
                        Close();
                    }
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public bool AddHealth(int id)
        {
            if (player.client.id == id || player2.client.id == id)
            {
                if (player.client.id == id)
                {
                    ServerSend.Command(player2.client.id, $"EnemyScore|{player.login}|{player.score}|{player.lifes}");
                    player.lifes++;
                }
                if (player2.client.id == id)
                {
                    ServerSend.Command(player.client.id, $"EnemyScore|{player2.login}|{player2.score}|{player2.lifes}");
                    player2.lifes++;
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public void Start()
        {
            ServerSend.Command(player.client.id, "EnemyFound");
            ServerSend.Command(player2.client.id, "EnemyFound");

        }
        public void Ready()
        {
            ServerSend.Command(player2.client.id, $"EnemyScore|{player.login}|{player.score}|{player.lifes}");
            ServerSend.Command(player.client.id, $"EnemyScore|{player2.login}|{player2.score}|{player2.lifes}");

            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(3000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            aTimer.Start();
            isStart = true;
        }


        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            GenerateFigure();
        }
        private void GenerateFigure()
        {
            count++;
            Random randomFigure = new Random();
            Random randomPosition = new Random();
            int idFigure = randomFigure.Next(0, 2);
            float position = (float)randomPosition.NextDouble();
            ServerSend.Command(player.client.id, $"DelayedSpawn|{idFigure}|{position}");
            ServerSend.Command(player2.client.id, $"DelayedSpawn|{idFigure}|{position}");
            if (count > 10)
            {
                ServerSend.Command(player.client.id, "DelayedSpawnBonus");
                ServerSend.Command(player2.client.id, "DelayedSpawnBonus");
            }
        }
        public void Close()
        {
            isFull = false;
            isEmpty = true;
            readyPlayers = 0;
            if (isStart)
            {
                string sqlExpression = $"SELECT * FROM UsersData where login = @username";
                using (var connection = new SqliteConnection(Constants.connectionString))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                    command.Parameters.AddWithValue("@username", winnerLogin);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            reader.Read();
                            int money = reader.GetInt32(2) + player.score + player2.score;
                            int wins = reader.GetInt32(3) + 1;
                            sqlExpression = $"UPDATE UsersData SET money = '{money}', wins = '{wins}' where login = @username";
                            command = new SqliteCommand(sqlExpression, connection);
                            command.Parameters.AddWithValue("@username", winnerLogin);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                sqlExpression = $"SELECT * FROM UsersData where login = @username";
                using (var connection = new SqliteConnection(Constants.connectionString))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand(sqlExpression, connection);
                    command.Parameters.AddWithValue("@username", loserLogin);
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows) // если есть данные
                        {
                            reader.Read();
                            int losses = reader.GetInt32(4) + 1;
                            sqlExpression = $"UPDATE UsersData SET losses = '{losses}' where login = @username";
                            command = new SqliteCommand(sqlExpression, connection);
                            command.Parameters.AddWithValue("@username", loserLogin);
                            command.ExecuteNonQuery();
                        }
                    }
                }
                aTimer.Stop();
                aTimer.Dispose();
            }
            isStart = false;
            count = 0;
        }
    }
}
