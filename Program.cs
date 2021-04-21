using System;
using System.Collections.Generic;

namespace Golf
{
    public class Golf
    {
        int Handicap;
        int Difficulty;
        bool ShowClubSuggestion = true;
        int Hole = 0;
        List<(int, int)> GameData = new List<(int, int)>() {(0,0)}; // strokes, yards

        int[][] CourseInfo = new int[][] {    // Course info: yards, par, left feature, right feature
            new int[] { },
            new int[] { 361, 4, 1, 2 },
            new int[] { 389, 3, 2, 1 },
            new int[] { 206, 3, 2, 2 },
            new int[] { 500, 5, 5, 1 },
            new int[] { 408, 4, 3, 4 },
            new int[] { 359, 4, 2, 0 },
            new int[] { 424, 5, 2, 5 },
            new int[] { 388, 4, 2, 1 },
            new int[] { 196, 3, 4, 1 },
            new int[] { 400, 4, 2, 3 },
            new int[] { 560, 5, 0, 5 },
            new int[] { 132, 3, 1, 0 },
            new int[] { 357, 4, 0, 4 },
            new int[] { 294, 4, 1, 1 },
            new int[] { 475, 5, 1, 2 },
            new int[] { 375, 4, 0, 0 },
            new int[] { 180, 3, 2, 5 },
            new int[] { 550, 5, 5, 3 }
        };

        string[] Features = new string[]{"Fairway", "Rough", "Trees", "Adjacent Fairway", "Trap", "Water"};

        (string, int)[] ClubYardage = new (string, int)[] { ("",0), ("Driver", 250), ("3 Wood", 225), ("5 Wood", 205), ("Hybrid", 190), ("4 Iron", 170), ("5 Iron", 165), ("6 Iron", 160), ("7 Iron", 150), ("8 Iron", 140), ("9 Iron", 125), ("Pitching wedge", 110), ("Sand wedge", 90), ("Lob wedge", 80), ("Putter", 10) };


        static void w(string s) { Console.WriteLine(s); }

        
        // --------------------------------------------------------------- constructor
        public Golf()
        {
            w("\n");
            w("          8\"\"\"\"8 8\"\"\"88 8     8\"\"\"\" ");
            w("          8    \" 8    8 8     8     ");
            w("          8e     8    8 8e    8eeee ");
            w("          88  ee 8    8 88    88    ");
            w("          88   8 8    8 88    88    ");
            w("          88eee8 8eeee8 88eee 88    ");
            w("\n");

            w("Welcome to the Creative Computing Country Club,");
            w("an eighteen hole championship layout located a short");
            w("distance from scenic downtown Morristown, New Jersey.");
            w("The commentator will explain the game as you play.");
            w("Enjoy your game! See you at the 19th hole...\n");
            w("Type QUIT at any time to leave the game.");
            w("\n");

            Handicap = ReadInt("PGA handicaps range from 0 to 30.\nWhat is your handicap?", "", 0, 30);
            if (Handicap == -1) { Quit(); return; }

            w("\nDifficulties at golf include:");
            w("1=Hook, 2=Slice, 3=Poor Distance, 4=Trap Shots, 5=Putting");
            
            Difficulty = ReadInt("Which one is your worst?", "", 1, 5);
            if (Difficulty == -1) { Quit(); return; }
            
            StartHole();
        }

        
        // --------------------------------------------------------------- StartHole
        private void StartHole()
        {
            Hole++;
            if (Hole > 18) {  }; //done
            GameData.Add((0, CourseInfo[Hole][0]));

            var info = CourseInfo[Hole];

            w("\n                |> " + Hole);
            w("                |        ");
            w("                |        ");
            w("          ^^^^^^^^^^^^^^^");

            w(string.Format("You are at the tee off HOLE #{0}, Distance is {1} yards, Par {2}.", Hole, info[0], info[1]));
            w(string.Format("On your left is {0}.", Features[info[2]]));
            w(string.Format("On your right is {0}.", Features[info[3]]));

            var club = ChooseClub();
            if (club == -1) { Quit(); return; };

            var power = 100;
            if (GameData[Hole].Item2 > 100){
                power = ReadInt("Now gauge your distance by a percentage of a full swing (1 to 100):", "", 1, 100);
               if (power == -1) { Quit(); return; };
            }

            Swing(club, power);
        }


        // --------------------------------------------------------------- Swing
        private void Swing(int club, int power)
        {
            int currentDistance = GameData[Hole].Item2;
            int clubAve = ClubYardage[club].Item2;
            
            if(currentDistance > 100){
                
            }else{

            }
            
        }
        
        // there's a 1 in 100 chance of an Ace on a par 3
        // if handicap < 15, likely to reach our exceed clubAve
        // if handicap > 15,  likely less to reach ave
        // distance varies by up to 10% of ave        
        // 20% chance of hook or slice, unless it's a known difficulty increase to 30%
        // 5% chance of dub
        // beginners luck
        // if handicap > 15, 10% chance of avoiding slice/hook/dub
        // birdie par -1
        // bogey par + 1
        

        // --------------------------------------------------------------- stroke
        //              cup
        //              /|
        //     next    / | 
        //      hyp   /  | 
        //           /opp| 
        //   ball   * ---| distanceAtOpposite
        //           \   |
        //            \  | 
        //       hyp   \ |  adj
        //              \| 
        //             angle
        //             tee
        //      
        //      <- hook    slice ->  
        /*
          trigonometry: knowing the angle of the stroke and the distance
          we can compute the new distance to the cup using 2 right triangles
          By taking the absolute value of distanceAtOpposite, the stroke 
          distance can be either short of the cup, or overshoot the cup
          
          angle should be provided in degrees, negative for hook, positive for slice
          distance off line will be returned similarly: -hook, +slice
          return (new distance from cup, distance off line)             
        */

        private (int, int) Stroke(int distanceToCup, int strokeDistance, int angleDegrees)
        {
            bool hook = angleDegrees < 0;
            double radians = Math.Abs(angleDegrees) * (Math.PI / 180);

            int hypotenuse = strokeDistance;
            double adjacent = Math.Cos(radians) * hypotenuse;
            double opposite = Math.Sqrt(Math.Pow(hypotenuse, 2) - Math.Pow(adjacent, 2)); // pythagoras
            
            double distanceAtOpposite = Math.Abs(distanceToCup - adjacent);
            double nextHypotenuse = Math.Sqrt(Math.Pow(distanceAtOpposite, 2) + Math.Pow(opposite, 2));

            int distanceFromCup = Convert.ToInt32(nextHypotenuse);
            int offline = Convert.ToInt32(opposite);
            if (hook) { offline = -offline; };

            return (distanceFromCup, offline);
        }
        

        // --------------------------------------------------------------- ChooseClub
        private int ChooseClub()
        {
            if (ShowClubSuggestion)
            {
                w("\n  Yardage Desired                 Suggested Clubs");
                w("  -------------------------------------------------");
                w("  280 TO 200 Yards                   1 TO 4        ");
                w("  200 TO 100 Yards                   5 TO 9        ");
                w("  0 TO 100 Yards                    10 TO 14       ");
            };
            ShowClubSuggestion = false; // only once
            
            w("\nYour bag:");
            w("1) Driver, 2) 3-Wood, 3) 5-Wood, 4) Hybrid, 5) 4-Iron, 6) 5-Iron, 7) 6-Iron, 8) 7-Iron, 9) 8-Iron, 10) 9-Iron, 11) Pitching wedge, 12) Sand wedge, 13) Lob wedge, 14) Putter\n");

            return ReadInt("What club do you choose? (1-14)", "\nThat club is not in your bag.\n", 1, 14);
        }


        // --------------------------------------------------------------- Quit
        private void Quit()
        {
            w("\nLooks like rain. Goodbye!");
            return;
        }

        
        // --------------------------------------------------------------- ReadInt
        private int ReadInt(string question, string errorMsg, int min, int max)
        {
            w(question);
            string i = Console.ReadLine().Trim().ToLower();
            if (i == "quit") { return -1; };

            int n;
            bool success = Int32.TryParse(i, out n);

            if (success)
            {
                if (n >= min && n <= max)
                {
                    return n;
                }
                else
                {
                    w(errorMsg);
                    return ReadInt(question, errorMsg, min, max);
                }
            }
            else
            {
                w(errorMsg);
                return ReadInt(question, errorMsg, min, max);
            };
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Golf g = new Golf();
        }
    }
}
