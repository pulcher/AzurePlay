using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EhTalk.GameClient
{
    class Program
    {
        public static string GameName { get; set; }
        public static int NumberOfPlayers { get; set; }
        public static int TimeLimit { get; set; }

        public static List<string> AvailablePlayers = new List<string>
        {
            "Robby",
            "T-800",
            "Clyde",
            "Wopr",
            "K-9",
            "Marvin",
            "C-3PO",
            "R2-D2",
            "T-1000",
            "Rosie",
            "Optimus Prime",
            "Megatron",
            "WALL-E",
            "Johnny 5",
            "Tom Servo",
            "Gort",
            "Data",
            "Bender",
            "Robot",
            "Dalecks",
            "Bishop",
            "Twiki",
            "RoboCop",
            "Kitt",
            "MechaGodzilla",

        };

        public static List<string> CurrentPlayers { get; set; } = new List<string>();

        static void Main(string[] args)
        {
            Console.WriteLine("Shall we play a game?");
            Console.Write("Game Name: ");
            GameName = Console.ReadLine();

            Console.Write($"Number Players(1 - {AvailablePlayers.Count}): ");
            NumberOfPlayers = Convert.ToInt32(Console.ReadLine());

            Console.Write("Time Limit (seconds):");
            TimeLimit = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Press enter to start the game");

            PickGamePlayers(NumberOfPlayers);

            StartGame();

            Console.WriteLine("Press a key to close the window:");
            Console.ReadKey();
        }

        private static void StartGame()
        {
            Console.WriteLine($"Game: {GameName} starting....");
            Console.WriteLine($"{NumberOfPlayers} players for {TimeLimit}");
            Console.WriteLine("Now entering the arena:");
            foreach (var player in CurrentPlayers)
            {
                Console.WriteLine($"- {player}");
            }

            Console.WriteLine();

            // set end time
            // set game id
            // set start time
            // 
            RunGame();
        }

        private static void RunGame()
        {
            /* While (currTime < endtime)
             * { 
             *      Wait for some random time
             *      pick player1
             *      pick player2
             *      
             *      pickAction
             *      
             *      sendAction(p1, p2, action)
             * 
             * 
             * 
             * 
             * 
             * 
             */
        }

        private static string PickPlayer()
        {
            var rnd = new Random();

            return CurrentPlayers[rnd.Next(0, CurrentPlayers.Count)];
        }

        private static string PickAction()
        {
            return "Killed";
        }

        private static void PickGamePlayers(int numberOfPlayers)
        {
            var rnd = new Random();

            while(AvailablePlayers.Count > 0 && CurrentPlayers.Count < numberOfPlayers)
            {
                var pick = rnd.Next(0, AvailablePlayers.Count);
                CurrentPlayers.Add(AvailablePlayers[pick]);
                AvailablePlayers.RemoveAt(pick);
            }
        }
    }

    
    public class GameEvent
    {
        public string GameId { get; set; }
        public DateTime EventTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Action { get; set; }
    }

    /* report
     *  number running games
     *  
     *  Leaderboard ranked
     *      by kills(humiliations)
     *      by Heals
     *  
     *  Game Over Results
     *  
     *  Event log?
     */
}
