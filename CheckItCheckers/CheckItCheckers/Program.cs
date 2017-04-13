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
        public const int WHITE = 0;
        public const int BLACK = 1;
        public const int WHITE_KING = 2;
        public const int BLACK_KING = 3;
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
            Application.Run(new MainForm());
            
        }
    }
}
