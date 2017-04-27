using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CheckItCheckers
{
    public class Globals
    {
        public const int BOARD_SIZE = 8;
        public static PictureBox[] pbs;
        public static char[,] board;
        public static int timeOut = 0;
        public const int WHITE = 0;
        public const int BLACK = 1;
        public const int WHITE_KING = 2;
        public const int BLACK_KING = 3;
        public const int EMPTY = -1;
        public const char BLACK_CHAR = 'b';
        public const char WHITE_CHAR = 'w';
        public const char BLACK_KING_CHAR = 'B';
        public const char WHITE_KING_CHAR = 'W';
        public const char EMPTY_CHAR = ' ';
        public const bool BLACK_TURN = false;
        public const bool WHITE_TURN = true;
        public static bool turn = BLACK_TURN;
        public static bool isBlackHuman = true;
        public static bool isWhiteHuman = true;
        public static int lastJumpCounter = 0;
        public static bool gameStarted = true;
        public static bool humanTurn = true;
        public static int clickedPosition = -1;
        public static bool gameFinished = false;
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm f = new MainForm();
            Application.Run(f);

        }
    }
}
