using System;
using System.Collections.Generic;
using System.Drawing;

namespace Golf
{
    using Ball = Golf.CircleGameObj;
    using Hazard = Golf.CircleGameObj;

    
    // --------------------------------------------------------------------------- Golf
    public class Golf
    {
        Ball BALL;
        int HOLE_NUM = 0;
        int STROKE_NUM = 0;
        int Handicap = 0;
        int PlayerDifficulty = 0;
        HoleGeometry holeGeometry;

        // all fairways are 40 yards wide, extend 5 yards beyond the cup, and
        // have 5 yards of rough around the perimeter
        const int FairwayWidth = 40;
        const int FairwayExtension = 5;
        const int RoughAmt = 5;

        // ScoreCard records the ball position after each stroke
        // a new list for each hole
        // include a blank list so index 1 == hole 1

        List<List<Ball>> ScoreCard = new List<List<Ball>> { new List<Ball>() };

        static void w(string s) { Console.WriteLine(s); } // WRITE        
        Random RANDOM = new Random();

        
        // --------------------------------------------------------------- constructor
        public Golf()
        {
            w(" ");
            w("          8\"\"\"\"8 8\"\"\"88 8     8\"\"\"\" ");
            w("          8    \" 8    8 8     8     ");
            w("          8e     8    8 8e    8eeee ");
            w("          88  ee 8    8 88    88    ");
            w("          88   8 8    8 88    88    ");
            w("          88eee8 8eeee8 88eee 88    ");
            w(" ");
            w("Welcome to the Creative Computing Country Club,");
            w("an eighteen hole championship layout located a short");
            w("distance from scenic downtown Lambertville, New Jersey.");
            w("The game will be explained as you play.");
            w("Enjoy your game! See you at the 19th hole...");
            w(" ");
            w("Type QUIT at any time to leave the game.");
            w("Type BAG at any time to review the clubs in your bag.");
            w(" ");

            Wait((z) =>
            {
                w(" ");
                ReviewBag();
                w("Type BAG at any time to review the clubs in your bag.");
                w(" ");

                Wait((zz) =>
                {

                    w(" ");
                    Ask("PGA handicaps range from 0 to 30.\nWhat is your handicap?", 0, 30, (i) =>
                    {
                        Handicap = i;
                        w(" ");

                        Ask("Common difficulties at golf include:\n1=Hook, 2=Slice, 3=Poor Distance, 4=Trap Shots, 5=Putting\nWhich one is your worst?", 1, 5, (j) =>
                        {
                            PlayerDifficulty = j;
                            NewHole();
                        });
                    });
                });
            });
        }


        // --------------------------------------------------------------- NewHole
        void NewHole()
        {
            HOLE_NUM++;
            STROKE_NUM = 0;

            HoleInfo info = CourseInfo[HOLE_NUM];

            int yards = info.Yards;  // from tee to cup
            int par = info.Par;
            var cup = new CircleGameObj(0, 0, 0, GameObjType.CUP);
            var green = new CircleGameObj(0, 0, 10, GameObjType.GREEN);

            var fairway = new RectGameObj(0 - (FairwayWidth / 2),
                                          0 - (green.Radius + FairwayExtension),
                                          FairwayWidth,
                                          yards + (green.Radius + FairwayExtension) + 1,
                                          GameObjType.FAIRWAY);

            var rough = new RectGameObj(fairway.X - RoughAmt,
                                        fairway.Y - RoughAmt,
                                        fairway.Width + (2 * RoughAmt),
                                        fairway.Length + (2 * RoughAmt),
                                        GameObjType.ROUGH);

            BALL = new Ball(0, yards, 0, GameObjType.BALL);

            ScoreCardStartNewHole();

            holeGeometry = new HoleGeometry(cup, green, fairway, rough, info.Hazard);

            w("                |> " + HOLE_NUM);
            w("                |        ");
            w("                |        ");
            w("          ^^^^^^^^^^^^^^^");

            Console.WriteLine("Hole #{0}. You are at the tee. Distance {1} yards, par {2}.", HOLE_NUM, info.Yards, info.Par);
            w(info.Description);

            TeeUp();
        }


        // --------------------------------------------------------------- TeeUp
        // on the green? automatically select putter
        // otherwise pick club and swing strength

        void TeeUp()
        {
            if (IsOnGreen(BALL) && !IsInHazard(BALL, GameObjType.SAND))
            {
                var msg = Odds(20) ? "Keep your head down.\n" : "";

                Ask(msg + "Choose your putt potency. (1-10)", 1, 10, (strength) =>
                {
                    var putter = Clubs[10];
                    Stroke(putter.Item2 * (strength/10), 10);
                });
            }
            else
            {
                Ask("What club do you choose? (1-10)", 1, 10, (c) =>
                {
                    var club = Clubs[c];
                    w(club.Item1);

                    Ask("Now gauge your distance by a percentage of a full swing. (1-10)", 1, 10, (strength) =>
                    {
                        Stroke(club.Item2 * (strength/10), c);
                    });
                });
            };
        }


        // --------------------------------------------------------------- Stroke
        void Stroke(int clubAmt, int clubIndex) {

            STROKE_NUM++;

            var flags = 0b000000000000;

            // fore! only when driving
            if((STROKE_NUM == 1) && (clubAmt > 210) && Odds(30)){ w("\"...Fore !\""); };

            // dub
            if(Odds(5)){ flags |= dub; }; // there's always a 5% chance of dubbing it

            // if you're in the rough, or sand, you really should be using a wedge
            if((IsInRough(BALL) || IsInHazard(BALL, GameObjType.SAND)) &&
               !(clubIndex == 8 || clubIndex == 9))
            {
                if(Odds(40)){ flags |= dub; };
            };

            // trap difficulty
            if(IsInHazard(BALL, GameObjType.SAND) && PlayerDifficulty == 4)
            {
                if(Odds(20)){ flags |= dub; };
            }
          
            // hook/slice
            // There's 10% chance of a hook or slice
            // if it's a known playerDifficulty then increase chance to 30%
            // if it's a putt & putting is a playerDifficulty increase to 30%
            
            bool randHookSlice = (PlayerDifficulty == 1 ||
                                  PlayerDifficulty == 2 ||
                                  (PlayerDifficulty == 5 && IsOnGreen(BALL))) ? Odds(30) : Odds(10);
            
            if(randHookSlice){
                if(PlayerDifficulty == 1){
                    if(Odds(80)){ flags |= hook; }else{ flags |= slice; };
                }else if(PlayerDifficulty == 2){
                    if(Odds(80)){ flags |= slice; }else{ flags |= hook; };
                }else{
                    if(Odds(50)){ flags |= hook; }else{ flags |= slice; };
                };
            };

            // beginner's luck !
            // If handicap is greater than 15, there's a 10% chance of avoiding all errors 
            if((Handicap > 15) && (Odds(10))){ flags |= luck; w("Perfect swing!"); };
            
            // ace
            // there's a 10% chance of an Ace on a par 3           
            if(CourseInfo[HOLE_NUM].Par == 3 && Odds(10) && STROKE_NUM == 1) { flags |= ace; };

            // distance:
            // If handicap is < 15, there a 50% chance of reaching club average,
            // a 25% of exceeding it, and a 25% of falling short 
            // If handicap is > 15, there's a 25% chance of reaching club average,
            // and 75% chance of falling short
            // The greater the handicap, the more the ball falls short
            // If poor distance is a known playerDifficulty, then reduce distance by 10% 
            
            int distance;
            int rnd = RANDOM.Next(1, 100);
            
            if(Handicap < 15){
                if(rnd <= 25){
                    distance = clubAmt - (clubAmt * (Handicap/100));
                }else if(rnd > 25 && rnd <= 75){
                    distance = clubAmt;
                }else{
                    distance = clubAmt + Convert.ToInt32(clubAmt * 0.10);
                };
            }else{
                if(rnd <= 75){
                    distance = clubAmt - (clubAmt * (Handicap/100));
                }else{
                    distance = clubAmt;                      
                };
            };
            if(PlayerDifficulty == 3){
                if(Odds(80)){ distance = Convert.ToInt32(distance * 0.10); };
            };
            
            if((flags & luck) == 1){ distance = clubAmt; }
            
            // angle
            // For all strokes, there's a possible "drift" of 4 degrees 
            // a hooks or slice increases the angle between 5-10 degrees, hook uses negative degrees
            int angle = RANDOM.Next(0, 4);
            if((flags & slice) == 1){ angle = RANDOM.Next(5, 10); };
            if((flags & hook) == 1 ){ angle = 0 - RANDOM.Next(5, 10); };
            if((flags & luck) == 1 ){ angle = 0; };
            
            var plot = PlotBall(BALL, distance, angle);  // calculate a new location

            
        }


        // --------------------------------------------------------------- plotBall
        Plot PlotBall(Ball ball, int strokeDistance, int degreesOff)
        {
            var cupVector = new Point(0, 1); 
            var radFromCup = Math.Atan2(ball.Y, ball.X) - Math.Atan2(cupVector.Y, cupVector.X);
            var radFromBall = radFromCup - Math.PI;

            var hypotenuse = strokeDistance;
            var adjacent = Math.Cos(radFromBall + ToRadians(degreesOff)) * hypotenuse;
            var opposite = Math.Sqrt(Math.Pow(hypotenuse, 2) - Math.Pow(adjacent, 2));

            Point newPos;
            if(ToDegrees360(radFromBall + ToRadians(degreesOff)) > 180){
                newPos = new Point(Convert.ToInt32(ball.X - opposite),
                                   Convert.ToInt32(ball.Y - adjacent));
            }else{
                newPos = new Point(Convert.ToInt32(ball.X + opposite),
                                   Convert.ToInt32(ball.Y - adjacent));
            }

            return new Plot(newPos.X, newPos.Y, Convert.ToInt32(opposite));
        }



        /*
      function stroke(clubAmt, clubIndex){


          let flags = 0b000000000000;

          // fore!, only for drivers
          if((STROKE_NUM == 1) && (clubAmt > 210) && odds(30)){ write(`"...Fore !"`); }
          
          
          // dub
          if(odds(5)){ flags |= dub; }; // there's always a 5% chance of dubbing it


          // if you're in the rough, or sand, you really should be using a wedge
          if((isInRough(BALL) || isInHazard(BALL, "sand"))
             && !(clubIndex == 8 || clubIndex == 9)){
              if(odds(40)){ flags |= dub; };
          };

          // trap difficulty
          if(isInHazard(BALL, "sand") && playerDifficulty == 4){ if(odds(20)){ flags |= dub; }; }
          
          // hook/slice
          // There's 10% chance of a hook or slice;
          // if it's a known playerDifficulty then increase chance to 30%
          // if it's a putt & putting is a playerDifficulty increase to 30%

          let randHookSlice = (playerDifficulty == 1 ||
                               playerDifficulty == 2 ||
                               (playerDifficulty == 5 && isOnGreen(BALL))) ? odds(30) : odds(10);
          
                      
          if(randHookSlice){
              if(playerDifficulty == 1){
                  if(odds(80)){ flags |= hook; }else{ flags |= slice; };
              }else if(playerDifficulty == 2){
                  if(odds(80)){ flags |= slice; }else{ flags |= hook; };
              }else{
                  if(odds(50)){ flags |= hook; }else{ flags |= slice; };
              };
          };

          // beginner's luck !
          // If handicap is greater than 15, there's a 10% chance of avoiding all errors 
          if((handicap > 15) && (odds(10))){ flags |= luck; write("Perfect swing!<br>"); };


          // ace
          // there's a 1 in 100 chance of an Ace on a par 3           
          if(courseInfo[HOLE_NUM][2] == 3 && odds(1) && STROKE_NUM == 1) { flags |= ace; };

          

            distance:
            If handicap is < 15, there a 50% chance of reaching club average, a 25% of exceeding it,
            and a 25% of falling short 
            If handicap is > 15, there's a 25% chance of reaching club average, and 75% chance of
            falling short
            The greater the handicap, the more the ball falls short
            If poor distance is a known playerDifficulty, then reduce distance by 10% 

let distance;

          let rnd = getRandomInclusive(1, 100);              
          if(handicap < 15){
              if(rnd <= 25){
                  distance = clubAmt - (clubAmt * (handicap/100));
              }else if(rnd > 25 && rnd <= 75){
                  distance = clubAmt;
              }else{
                  distance = clubAmt + (clubAmt * 0.10);
              };
          }else{
              if(rnd <= 75){
                  distance = clubAmt - (clubAmt * (handicap/100));
              }else{
                  distance = clubAmt;                      
              };
          };
          if(playerDifficulty == 3){ if(odds(80)){ distance = distance * 0.10; }; };
          if(flags & luck){ distance = clubAmt; }

                               
          // angle
          // For all strokes, there's a possible "drift" of 4 degrees 
          // a hooks or slice increases the angle between 5-10 degrees, hook uses negative degrees
          let angle = getRandomInclusive(0, 4);
          if(flags & slice){ angle = getRandomInclusive(5, 10); }
          if(flags & hook ){ angle = 0 - (getRandomInclusive(5, 10)); }          
          if(flags & luck ){ angle = 0; }
          
          let plot = plotBall(BALL, distance, angle);  // calculate a new location

          flags = findBall(plot, flags)

          //console.log(flags.toString(2).padStart(12,"0"));

          interpretResults(plot, flags)
      }

*/




        // --------------------------------------------------------------- IsOngreen
        bool IsOnGreen(Ball ball)
        {
            return GetDistance(ball, holeGeometry.Cup) < holeGeometry.Green.Radius;
        }

        
        // --------------------------------------------------------------- IsInHazard
        bool IsInHazard(Ball ball, GameObjType hazard)
        {

            bool result = false;
            Array.ForEach(holeGeometry.Hazards, (Hazard h) =>
            {
                if ((GetDistance(ball, h) < h.Radius) && h.Type == hazard) { result = true; };
            });
            return result;
        }

        
        // --------------------------------------------------------------- IsInRough
        bool IsInRough(Ball ball){
            return IsInRectangle(ball, holeGeometry.Rough) &&
                (IsInRectangle(ball, holeGeometry.Fairway) == false);
        }
        

        // --------------------------------------------------------------- ScoreCard
        void ScoreCardStartNewHole() { ScoreCard.Add(new List<Ball>()); }

        void ScoreCardRecordStroke(Ball ball) { ScoreCard[HOLE_NUM].Add(ball); }

        Ball ScoreCardGetPreviousStroke()
        {
            return ScoreCard[HOLE_NUM][ScoreCard[HOLE_NUM].Count - 1];
        }

        int ScoreCardGetTotal()
        {
            int total = 0;
            ScoreCard.ForEach((h) => { total += h.Count; });
            return total;
        }


        // --------------------------------------------------------------- Ask
        // input from console is always an integer passed to a callback
        // or "quit" to end game

        void Ask(string question, int min, int max, Action<int> callback)
        {
            w(question);
            string i = Console.ReadLine().Trim().ToLower();
            if (i == "quit") { Quit(); return; };
            if (i == "bag") { ReviewBag(); };

            int n;
            bool success = Int32.TryParse(i, out n);

            if (success)
            {
                if (n >= min && n <= max)
                {
                    callback(n);
                }
                else
                {
                    Ask(question, min, max, callback);
                }
            }
            else
            {
                Ask(question, min, max, callback);
            };
        }


        // --------------------------------------------------------------- Wait
        void Wait(Action<int> callback)
        {
            w("Press any key to continue.");

            ConsoleKeyInfo keyinfo;
            do { keyinfo = Console.ReadKey(true); }
            while (keyinfo.KeyChar < 0);
            callback(0);
        }


        // --------------------------------------------------------------- ReviewBag
        void ReviewBag()
        {
            w(" ");
            w("  #     Club      Average Yardage");
            w("-----------------------------------");
            w("  1    Driver           250");
            w("  2    3 Wood           225");
            w("  3    5 Wood           200");
            w("  4    Hybrid           190");
            w("  5    4 Iron           170");
            w("  6    7 Iron           150");
            w("  7    9 Iron           125");
            w("  8    Pitching wedge   110");
            w("  9    Sand wedge        75");
            w(" 10    Putter            10");
            w(" ");
        }


        // --------------------------------------------------------------- Quit
        void Quit()
        {
            w("");
            w("Looks like rain.Goodbye!");
            w("");
            return;
        }

        /*

      let STROKE_NUM = 0;      
      let handicap = 0;
      let playerDifficulty = 0;
      let holeGeometry;
      let scoreCard = [[]]; 
      let BALL = {id:"BALL", x:0, y:0};          
      let inputCallback = () => {};

        */


        // YOUR BAG
        // ======================================================== Clubs
        (string, int)[] Clubs = new (string, int)[] {
            ("",0),
                
                // name, average yardage                
                ("Driver", 250),
                ("3 Wood", 225),
                ("5 Wood", 200),
                ("Hybrid", 190),
                ("4 Iron", 170),
                ("7 Iron", 150),
                ("9 Iron", 125),
                ("Pitching wedge", 110),
                ("Sand wedge", 75),
                ("Putter", 10)
                };


        // THE COURSE
        // ======================================================== CourseInfo

        HoleInfo[] CourseInfo = new HoleInfo[]{
            new HoleInfo(0, 0, 0, new Hazard[]{}, ""), // include a blank so index 1 == hole 1

            
            // -------------------------------------------------------- front 9
            // hole, yards, par, hazards, (description)

            new HoleInfo(1, 361, 4,
                         new Hazard[]{
                             new Hazard( 20, 100, 10, GameObjType.TREES),
                             new Hazard(-20,  80, 10, GameObjType.TREES),
                             new Hazard(-20, 100, 10, GameObjType.TREES)
                         },
                         "There are a couple of trees on the left and right."),

            new HoleInfo(2, 389, 4,
                         new Hazard[]{
                             new Hazard(0, 160, 20, GameObjType.WATER)
                         },
                         "There is a large water hazard across the fairway about 150 yards."),

            new HoleInfo(3, 206, 3,
                         new Hazard[]{
                             new Hazard( 20,  20,  5, GameObjType.WATER),
                             new Hazard(-20, 160, 10, GameObjType.WATER),
                             new Hazard( 10,  12,  5, GameObjType.SAND)
                         },
                         "There is some sand and water near the green."),

            new HoleInfo(4, 500, 5,
                         new Hazard[]{
                             new Hazard(-14, 12, 12, GameObjType.SAND)
                         },
                         "There's a bunker to the left of the green."),

            new HoleInfo(5, 408, 4,
                         new Hazard[]{
                             new Hazard(20, 120, 20, GameObjType.TREES),
                             new Hazard(20, 160, 20, GameObjType.TREES),
                             new Hazard(10,  20,  5, GameObjType.SAND)
                         },
                         "There are some trees to your right."),

            new HoleInfo(6, 359, 4,
                         new Hazard[]{
                             new Hazard( 14, 0, 4, GameObjType.SAND),
                             new Hazard(-14, 0, 4, GameObjType.SAND)
                         },
                         ""),

            new HoleInfo(7, 424, 5,
                         new Hazard[]{
                             new Hazard(20, 200, 10, GameObjType.SAND),
                             new Hazard(10, 180, 10, GameObjType.SAND),
                             new Hazard(20, 160, 10, GameObjType.SAND)
                         },
                         "There are several sand traps along your right."),

            new HoleInfo(8, 388, 4,
                         new Hazard[]{
                             new Hazard(-20, 340, 10, GameObjType.TREES)
                         },
                         ""),

            new HoleInfo(9, 196, 3,
                         new Hazard[]{
                             new Hazard(-30, 180, 20, GameObjType.TREES),
                             new Hazard( 14,  -8,  5, GameObjType.SAND)
                         },
                         ""), 
            
            // -------------------------------------------------------- back 9
            // hole, yards, par, hazards, (description)

            new HoleInfo(10, 400, 4,
                         new Hazard[]{
                             new Hazard(-14, -8, 5, GameObjType.SAND),
                             new Hazard( 14, -8, 5, GameObjType.SAND)
                         },
                         ""),

            new HoleInfo(11, 560, 5,
                         new Hazard[]{
                             new Hazard(-20, 400, 10, GameObjType.TREES),
                             new Hazard(-10, 380, 10, GameObjType.TREES),
                             new Hazard(-20, 260, 10, GameObjType.TREES),
                             new Hazard(-20, 200, 10, GameObjType.TREES),
                             new Hazard(-10, 180, 10, GameObjType.TREES),
                             new Hazard(-20, 160, 10, GameObjType.TREES)
                         },
                         "Lots of trees along the left of the fairway."),

            new HoleInfo(12, 132, 3,
                         new Hazard[]{
                             new Hazard(-10, 120, 10, GameObjType.WATER),
                             new Hazard( -5, 100, 10, GameObjType.SAND)
                         },
                         "There is water and sand directly in front of you. A good drive should clear both."),

            new HoleInfo(13, 357, 4,
                         new Hazard[]{
                             new Hazard(-20, 200, 10, GameObjType.TREES),
                             new Hazard(-10, 180, 10, GameObjType.TREES),
                             new Hazard(-20, 160, 10, GameObjType.TREES),
                             new Hazard( 14,  12,  8, GameObjType.SAND)
                         },
                         ""),

            new HoleInfo(14, 294, 4,
                         new Hazard[]{
                             new Hazard(0, 20, 10, GameObjType.SAND)
                         },
                         ""),

            new HoleInfo(15, 475, 5,
                         new Hazard[]{
                             new Hazard(-20, 20, 10, GameObjType.WATER),
                             new Hazard( 10, 20, 10, GameObjType.SAND)
                         },
                         "Some sand and water near the green."),

            new HoleInfo(16, 375, 4,
                         new Hazard[]{
                             new Hazard(-14, -8, 5, GameObjType.SAND)
                         },
                         ""),

            new HoleInfo(17, 180, 3,
                         new Hazard[]{
                             new Hazard( 20, 100, 10, GameObjType.TREES),
                             new Hazard(-20,  80, 10, GameObjType.TREES)
                         },
                         ""),

            new HoleInfo(18, 550, 5,
                         new Hazard[]{
                             new Hazard(20, 30, 15, GameObjType.WATER)
                         },
                         "There is a water hazard near the green.")
        };


        
        // -------------------------------------------------------- HoleInfo
        class HoleInfo
        {
            public int Hole { get; }
            public int Yards { get; }
            public int Par { get; }
            public Hazard[] Hazard { get; }
            public string Description { get; }

            public HoleInfo(int hole, int yards, int par, Hazard[] hazard, string description)
            {
                Hole = hole;
                Yards = yards;
                Par = par;
                Hazard = hazard;
                Description = description;
            }
        }


        public enum GameObjType { BALL, CUP, GREEN, FAIRWAY, ROUGH, TREES, WATER, SAND }


        // -------------------------------------------------------- CircleGameObj
        public class CircleGameObj
        {

            public GameObjType Type { get; }
            public int X { get; }
            public int Y { get; }
            public int Radius { get; }

            public CircleGameObj(int x, int y, int r, GameObjType type)
            {
                Type = type;
                X = x;
                Y = y;
                Radius = r;
            }
        }


        // -------------------------------------------------------- RectGameObj
        public class RectGameObj
        {

            public GameObjType Type { get; }
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Length { get; }

            public RectGameObj(int x, int y, int w, int l, GameObjType type)
            {
                Type = type;
                X = x;
                Y = y;
                Width = w;
                Length = l;
            }
        }


        // -------------------------------------------------------- HoleGeometry
        public class HoleGeometry
        {

            public CircleGameObj Cup { get; }
            public CircleGameObj Green { get; }
            public RectGameObj Fairway { get; }
            public RectGameObj Rough { get; }
            public Hazard[] Hazards { get; }

            public HoleGeometry(CircleGameObj cup, CircleGameObj green, RectGameObj fairway, RectGameObj rough, Hazard[] haz)
            {
                Cup = cup;
                Green = green;
                Fairway = fairway;
                Rough = rough;
                Hazards = haz;
            }
        }


        // -------------------------------------------------------- Plot
        public class Plot
        {
            public int X { get; }
            public int Y { get; }
            public int Offline { get; }

            public Plot(int x, int y, int offline) {
                X = x;
                Y = y;
                Offline = offline;
            }
        }


        // -------------------------------------------------------- bitwise Flags
        int dub = 0b00000000000001;
        int hook = 0b00000000000010;
        int slice = 0b00000000000100;
        int passedCup = 0b00000000001000;
        int inCup = 0b00000000010000;
        int onFairway = 0b00000000100000;
        int onGreen = 0b00000001000000;
        int inRough = 0b00000010000000;
        int inSand = 0b00000100000000;
        int inTrees = 0b00001000000000;
        int inWater = 0b00010000000000;
        int outOfBounds = 0b00100000000000;
        int luck = 0b01000000000000;
        int ace = 0b10000000000000;




        // -------------------------------------------------------- GetDistance
        // distance between 2 points
        double GetDistance(CircleGameObj o1, CircleGameObj o2)
        {
            return Math.Sqrt(Math.Pow((o2.X - o1.X), 2) + Math.Pow((o2.Y - o1.Y), 2));
        }


        // -------------------------------------------------------- IsInRectangle
        bool IsInRectangle(CircleGameObj pt, RectGameObj rect) {
            return ((pt.X > rect.X) &&
                    (pt.X < rect.X + rect.Width) &&                                    
                    (pt.Y > rect.Y) &&
                    (pt.Y < rect.Y + rect.Length));
        }

        // -------------------------------------------------------- ToRadians
        double ToRadians (int angle) { return angle * (Math.PI / 180); }

        
        // -------------------------------------------------------- ToDegrees360
        // radians to 360 degrees
        double ToDegrees360 (double angle) {
            double deg = angle * (180 / Math.PI);
            if (deg < 0.0) {deg += 360.0;}
            return deg;
        }

        

        Random RND = new Random();

        // -------------------------------------------------------- Odds
        // chance an integer is <= the given argument
        // between 1-100
        bool Odds(int x)
        {
            return RND.Next(1, 100) <= x;
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

/*
      
      // ---------------------------------------------------------------------------------- stroke
      function stroke(clubAmt, clubIndex){

          STROKE_NUM++;

          let flags = 0b000000000000;

          // fore!, only for drivers
          if((STROKE_NUM == 1) && (clubAmt > 210) && odds(30)){ write(`"...Fore !"`); }
          
          
          // dub
          if(odds(5)){ flags |= dub; }; // there's always a 5% chance of dubbing it


          // if you're in the rough, or sand, you really should be using a wedge
          if((isInRough(BALL) || isInHazard(BALL, "sand"))
             && !(clubIndex == 8 || clubIndex == 9)){
              if(odds(40)){ flags |= dub; };
          };

          // trap difficulty
          if(isInHazard(BALL, "sand") && playerDifficulty == 4){ if(odds(20)){ flags |= dub; }; }
          
          // hook/slice
          // There's 10% chance of a hook or slice;
          // if it's a known playerDifficulty then increase chance to 30%
          // if it's a putt & putting is a playerDifficulty increase to 30%

          let randHookSlice = (playerDifficulty == 1 ||
                               playerDifficulty == 2 ||
                               (playerDifficulty == 5 && isOnGreen(BALL))) ? odds(30) : odds(10);
          
                      
          if(randHookSlice){
              if(playerDifficulty == 1){
                  if(odds(80)){ flags |= hook; }else{ flags |= slice; };
              }else if(playerDifficulty == 2){
                  if(odds(80)){ flags |= slice; }else{ flags |= hook; };
              }else{
                  if(odds(50)){ flags |= hook; }else{ flags |= slice; };
              };
          };

          // beginner's luck !
          // If handicap is greater than 15, there's a 10% chance of avoiding all errors 
          if((handicap > 15) && (odds(10))){ flags |= luck; write("Perfect swing!<br>"); };


          // ace
          // there's a 1 in 100 chance of an Ace on a par 3           
          if(courseInfo[HOLE_NUM][2] == 3 && odds(1) && STROKE_NUM == 1) { flags |= ace; };

          

            distance:
            If handicap is < 15, there a 50% chance of reaching club average, a 25% of exceeding it,
            and a 25% of falling short 
            If handicap is > 15, there's a 25% chance of reaching club average, and 75% chance of
            falling short
            The greater the handicap, the more the ball falls short
            If poor distance is a known playerDifficulty, then reduce distance by 10% 

let distance;

          let rnd = getRandomInclusive(1, 100);              
          if(handicap < 15){
              if(rnd <= 25){
                  distance = clubAmt - (clubAmt * (handicap/100));
              }else if(rnd > 25 && rnd <= 75){
                  distance = clubAmt;
              }else{
                  distance = clubAmt + (clubAmt * 0.10);
              };
          }else{
              if(rnd <= 75){
                  distance = clubAmt - (clubAmt * (handicap/100));
              }else{
                  distance = clubAmt;                      
              };
          };
          if(playerDifficulty == 3){ if(odds(80)){ distance = distance * 0.10; }; };
          if(flags & luck){ distance = clubAmt; }

                               
          // angle
          // For all strokes, there's a possible "drift" of 4 degrees 
          // a hooks or slice increases the angle between 5-10 degrees, hook uses negative degrees
          let angle = getRandomInclusive(0, 4);
          if(flags & slice){ angle = getRandomInclusive(5, 10); }
          if(flags & hook ){ angle = 0 - (getRandomInclusive(5, 10)); }          
          if(flags & luck ){ angle = 0; }
          
          let plot = plotBall(BALL, distance, angle);  // calculate a new location

          flags = findBall(plot, flags)

          //console.log(flags.toString(2).padStart(12,"0"));

          interpretResults(plot, flags)
      }


      // ---------------------------------------------------------------------------------- interpretResults
      function interpretResults(plot, flags){

          let cupDistance = Math.round(getDistance(plot, holeGeometry.cup));
          let travelDistance = Math.round(getDistance(plot, BALL));
          
          write('<br>');

          if(flags & ace){
              write("Hole in One! You aced it.<br>");
              scoreCardRecordStroke({id:"BALL", x:0, y:0});
              reportCurrentScore();              
              return;
          };

          if(flags & inTrees){
              write("Your ball is lost in the trees. Take a penalty stroke.<br>");
              scoreCardRecordStroke({id:"BALL", x:BALL.x, y:BALL.y});              
              teeUp();
              return;
          };

          if(flags & inWater){
              let msg = odds(50) ? "Your ball has gone to a watery grave." : "Your ball is lost in the water.";
              write(`${msg} Take a penalty stroke.<br>`);
              scoreCardRecordStroke({id:"BALL", x:BALL.x, y:BALL.y});              
              teeUp();
              return;
          };

          if(flags & outOfBounds){
              write("Out of bounds. Take a penalty stroke.<br>");
              scoreCardRecordStroke({id:"BALL", x:BALL.x, y:BALL.y});              
              teeUp();
              return;
          };

          if(flags & dub){
              write("You dubbed it.<br>");
              scoreCardRecordStroke({id:"BALL", x:BALL.x, y:BALL.y});              
              teeUp();
              return;
          };          
          
          if(flags & inCup){
              let msg = odds(50) ? "You holed it.<br>" : "It's in!<br>";
              write(msg);
              scoreCardRecordStroke({id:"BALL", x:plot.x, y:plot.y});
              reportCurrentScore();
              return;              
          };

          if((flags & slice) && !(flags & onGreen)){              
              let bad = (flags & outOfBounds) ? " badly" : "";
              write(`You sliced${bad}: ${plot.offLine} yards offline.<br>`);
          };

          if((flags & hook) && !(flags & onGreen)){
              let bad = (flags & outOfBounds) ? " badly" : "";
              write(`You hooked${bad}: ${plot.offLine} yards offline.<br>`);
          };

          if(STROKE_NUM > 1){
              let d1 = getDistance(scoreCardGetPreviousStroke(), holeGeometry.cup);
              let d2 = cupDistance;
              if(d2 > d1){ write(`Too much club.<br>`); }
          }
          
          if(flags & inRough){
              write("You're in the rough.<br>");
          };

          if(flags & inSand){
              write("You're in a sand trap.<br>");
          };          

          if(flags & onGreen){
              let pd = (cupDistance < 4) ? ((cupDistance * 3) + " feet") : (cupDistance + " yards");              
              write(`You're on the green. It's ${pd} from the pin.<br>`);
          };

          if((flags & onFairway) || (flags & inRough)){
              write(`<p>Shot went ${travelDistance} yards. It's ${cupDistance} yards from the cup.</p>`);
          };                                                

          scoreCardRecordStroke({id:"BALL", x:plot.x, y:plot.y});              
          BALL = {id:"BALL", x:plot.x, y:plot.y};          

          teeUp();          
      }


      //--------------------------------------------------------------------------- reportCurrentScore
      function reportCurrentScore(){

          let par = courseInfo[HOLE_NUM][2];
          if(scoreCard[HOLE_NUM].length == par + 1){ write(`A bogey. One above par.<br>`); }          
          if(scoreCard[HOLE_NUM].length == par){ write(`Par. Nice.<br>`); }
          if(scoreCard[HOLE_NUM].length == (par - 1)){ write(`A birdie! One below par.<br>`); }
          if(scoreCard[HOLE_NUM].length == (par - 2)){ write(`An Eagle! Two below par.<br>`); }                    

          let totalPar = 0;
          for(var i=1; i <= HOLE_NUM; i++){ totalPar += courseInfo[i][2]; }

          let plural = (HOLE_NUM > 1) ? "s" : "";
          write(`Total par for ${HOLE_NUM} hole${plural} is: ${totalPar}. Your total is: ${scoreCardGetTotal()}.<br>`);
          
          if(HOLE_NUM == 18){
              gameOver();
          }else{
              window.setTimeout(newHole, 2000);
          };
      }
          

      //--------------------------------------------------------------------------- scoreCard
      function scoreCardStartNewHole(){ scoreCard.push([]); }      
      function scoreCardRecordStroke(ball){ scoreCard[HOLE_NUM].push(ball); }
      function scoreCardGetPreviousStroke(){ return scoreCard[HOLE_NUM][scoreCard[HOLE_NUM].length - 1]; }
      function scoreCardGetTotal(){ let total = 0; scoreCard.forEach((h)=>{ total += h.length;}); return total; }      

      
      // ---------------------------------------------------------------------------------- plotBall
      function plotBall(ballPt, strokeDistance, degreesOff){

          let cupVector = {x:0, y:-1}; 

          let radFromCup = Math.atan2(ballPt.y, ballPt.x) - Math.atan2(cupVector.y, cupVector.x);
          let radFromBall = radFromCup - Math.PI;
          
          let hypotenuse = strokeDistance;
          let adjacent = Math.cos(radFromBall + toRadians(degreesOff)) * hypotenuse;
          let opposite = Math.sqrt(Math.pow(hypotenuse, 2) - Math.pow(adjacent, 2));

          let newPos;
          if(toDegrees360(radFromBall + toRadians(degreesOff)) > 180){
              newPos = {x:ballPt.x - opposite, y:ballPt.y - adjacent};
          }else{
              newPos = {x:ballPt.x + opposite, y:ballPt.y - adjacent};
          }

          return {x:newPos.x, y:newPos.y, offLine:Math.round(opposite)};
      }

      
      // ---------------------------------------------------------------------------------- findBall
      function findBall(ball, flags){

          if (isOnFairway(ball) && !isOnGreen(ball)) { flags |= onFairway; }
          if (isOnGreen(ball)){ flags |= onGreen; }
          if (isInRough(ball)){ flags |= inRough; }
          if (isInHazard(ball, "water")){ flags |= inWater; }
          if (isInHazard(ball, "trees")){ flags |= inTrees; }
          if (isInHazard(ball, "sand")){ flags |= inSand; }                    
          if (isOutOfBounds(ball)){ flags |= outOfBounds; }
          if (ball.y < 0){ flags |= passedCup; }
          if (getDistance(ball, holeGeometry.cup) < 1){ flags |= inCup; }          

          return flags;        
      }
      
      function isOnFairway(ball){ return isInRectangle(ball, holeGeometry.fairway); };

      function isOnGreen(ball){ return getDistance(ball, holeGeometry.cup) < holeGeometry.green.radius; };

      function isInRough(ball){
          return isInRectangle(ball, holeGeometry.rough) && (isInRectangle(ball, holeGeometry.fairway) == false);
      }

      function isInHazard(ball, hazard){

          let result = false;
          holeGeometry.hazards.forEach((h) => {
              if((getDistance(ball, h) < h.radius) && h.type == hazard){ result = true; };
          });
          return result;
      }

      function isOutOfBounds(ball){
          return (isOnFairway(ball) == false) && (isInRough(ball) == false)
      }

      
      // ---------------------------------------------------------------------------------- math helpers
      function toRadians (angle) { return angle * (Math.PI / 180); }
      
      function toDegrees360 (angle) { // radians to 360 degrees
          let deg = angle * (180 / Math.PI);
          if (deg < 0.0) {deg += 360.0;}
          return deg;
      }

      function getDistance(pt1, pt2) { // distance between 2 points
          return Math.sqrt(Math.pow((pt2.x - pt1.x),2) + Math.pow((pt2.y - pt1.y),2));
      }

      function isInRectangle(pt, rect) {
          return ((pt.x > rect.x) &&
                  (pt.x < rect.x + rect.width) &&                                    
                  (pt.y > rect.y) &&
                  (pt.y < rect.y + rect.length));
      }

      // random number inclusive of the max and min
      function getRandomInclusive(min, max) { return Math.floor(Math.random() * (max - min + 1) + min); }

 */
