using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Timers;

namespace CheckItCheckers
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Globals.pbs = this.Controls.OfType<PictureBox>()
                        .Where(pb => pb.Name.StartsWith("space"))
                        .OrderBy(pb => int.Parse(pb.Name.Replace("space", "")))
                        .ToArray();

            char[,] board = new char[Globals.BOARD_SIZE, Globals.BOARD_SIZE / 2];

            updatePiece(2, 1, Globals.EMPTY);
        }
        
        public void movePiece(int piece, int fromRow, int fromCol, int toRow, int toCol)
        {
            // TODO: if the move is valid
            updatePiece(fromRow, fromCol, Globals.EMPTY);
            // TODO: remove any piece that was jumped
            updatePiece(toRow, toCol, piece);

            // TODO: update char[,] board
        }

        private void readBoardFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            string fileString = fs.ToString();
            fs.Close();
        }

        private void writeBoardToFile(string fileName, char[,] board)
        {
            string[] boardAsStrings = new string[Globals.BOARD_SIZE];
            for(int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                //boardAsStrings[i] = board[i];
            }
            //File.WriteAllLines("board.txt", );
            
        }

        private void updatePiece(int row, int column, int color)
        {
            switch (color)
            {
                case Globals.WHITE:
                    Globals.pbs[(row * (Globals.BOARD_SIZE / 2)) + column].Image = global::CheckItCheckers.Properties.Resources.white_piece;
                    break;
                case Globals.BLACK:
                    Globals.pbs[(row * (Globals.BOARD_SIZE / 2)) + column].Image = global::CheckItCheckers.Properties.Resources.black_piece;
                    break;
                case Globals.WHITE_KING:
                    Globals.pbs[(row * (Globals.BOARD_SIZE / 2)) + column].Image = global::CheckItCheckers.Properties.Resources.white_king;
                    break;
                case Globals.BLACK_KING:
                    Globals.pbs[(row * (Globals.BOARD_SIZE / 2)) + column].Image = global::CheckItCheckers.Properties.Resources.black_king;
                    break;
                default:
                    Globals.pbs[(row * (Globals.BOARD_SIZE / 2)) + column].Image = null;
                    break;
            }
            this.Refresh();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        //Load File 
        private void loadLogToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //NEED TO SET FILE NAME HERE
            string filename = "test.txt";

            using (TextReader reader = File.OpenText(filename))
            {

                //Error check if the file does not exist
                if (!File.Exists(filename))
                {
                    MessageBox.Show(string.Format("File {0} does not exist or is not a text file", filename));
                }

                //Reads in the line and splits it into different variables.
                //Read in format: 10 15 
                string cordSplit = reader.ReadLine(); 
                string[] cords = cordSplit.Split(' ');

                int x = int.Parse(cords[0]); //cords[0] = 10 // x = 10
                int y = int.Parse(cords[1]); //cords[1] = 15 // y = 15

                //David: Call move function here. 
                //move(x,y);
            }
        }

        private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 5000;
            timer.Enabled = true;

            //timer.Stop();

        }

        private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 10000;
            timer.Enabled = true;

            //timer.Stop();

        }

        private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 20000;
            timer.Enabled = true;

            //timer.Stop();

        }

        private void minuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 60000;
            timer.Enabled = true;

            //timer.Stop();

        }

        private void noLimitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //This may not work. Need to test still. Since we can not set timer.Interval = PosativeInfinity, 
            //I have to set to the upper bound for the timer class.

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = Int32.MaxValue;
            timer.Enabled = true;

            //timer.Stop();

        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Time Limit has been reached!");

            //Probably swap to other players turn here.
        }
    }
}
