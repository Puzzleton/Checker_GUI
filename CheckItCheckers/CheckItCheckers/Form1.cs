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
using System.Diagnostics;

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

            Globals.board = new char[Globals.BOARD_SIZE, Globals.BOARD_SIZE / 2];
            initializeBoard();
            drawBoard();
        }

        // sets the board to the initial state
        private void initializeBoard()
        {
            // black pieces
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    Globals.board[i, j] = Globals.BLACK_CHAR;
                }
            }
            // middle spaces
            for (int i = 3; i < 5; i++)
            {
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    Globals.board[i, j] = Globals.EMPTY_CHAR;
                }
            }
            // white pieces
            for (int i = 5; i < Globals.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    Globals.board[i, j] = Globals.WHITE_CHAR;
                }
            }
        }

        // changes the picturebox images to match the board
        private void drawBoard()
        {
            for (int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    switch (Globals.board[i, j])
                    {
                        case (Globals.BLACK_CHAR):
                            Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Image = global::CheckItCheckers.Properties.Resources.black_piece;
                            break;
                        case (Globals.WHITE_CHAR):
                            Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Image = global::CheckItCheckers.Properties.Resources.white_piece;
                            break;
                        case (Globals.BLACK_KING_CHAR):
                            Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Image = global::CheckItCheckers.Properties.Resources.black_king;
                            break;
                        case (Globals.WHITE_KING_CHAR):
                            Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Image = global::CheckItCheckers.Properties.Resources.white_king;
                            break;
                        default:
                            Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Image = null;
                            break;
                    }
                }
            }
            switch (Globals.turn)
            {
                case Globals.BLACK_TURN:
                    turnTrackerLabel.Text = "Black's Turn.";
                    break;
                case Globals.WHITE_TURN:
                    turnTrackerLabel.Text = "White's Turn.";
                    break;
                default:
                    turnTrackerLabel.Text = "How did this happen??";
                    break;
            }

            this.Refresh();
        }

        public bool movePiece(int fromRow, int fromCol, int toRow, int toCol)
        {
            int jumpedRow = -1, jumpedCol = -1; // the location of a (possibly) jumped piece
            char piece = Globals.board[fromRow, fromCol];

            // check move validity

            // - check that toSpace is on the board
            if (toRow < 0 || toCol < 0 || toRow >= Globals.BOARD_SIZE || toCol >= Globals.BOARD_SIZE / 2)
                return false;

            // - check that the toSpace is empty
            if (Globals.board[toRow, toCol] != Globals.EMPTY_CHAR)
                return false;

            // - check that the move is in the piece's range
            // --- check direction of movement
            switch (piece)
            {
                // normal black piece
                case Globals.BLACK_CHAR:
                    if (fromRow >= toRow || Globals.turn != Globals.BLACK_TURN)
                        return false;
                    break;


                // normal white piece
                case Globals.WHITE_CHAR:
                    if (toRow >= fromRow || Globals.turn != Globals.WHITE_TURN)
                        return false;
                    break;


                // kings aren't restricted to direction
                case Globals.WHITE_KING_CHAR:
                    if (Globals.turn != Globals.WHITE_TURN)
                        return false;
                    break;
                case Globals.BLACK_KING_CHAR:
                    if (Globals.turn != Globals.BLACK_TURN)
                        return false;
                    break;


                // nothing else is valid
                default:
                    return false;
            }

            // --- check odd/even row
            switch (fromRow % 2)
            {
                // even row
                case 0:
                    // check jumping/not jumping
                    switch (Math.Abs(fromRow - toRow))
                    {
                        // not jumping
                        case 1:
                            if (toCol != fromCol && toCol != fromCol + 1)
                                return false;
                            break;

                        // jumping
                        case 2:
                            // jump left
                            if (toCol == fromCol - 1)
                                jumpedCol = fromCol;
                            // jump right
                            else if (toCol == fromCol + 1)
                                jumpedCol = fromCol + 1;
                            else
                                return false;
                            jumpedRow = (fromRow + toRow) / 2;
                            // check that the jumped piece is of the opposite color
                            if (!oppositeColors(fromRow, fromCol, jumpedRow, jumpedCol))
                                return false;
                            break;

                        // anything else is invalid
                        default:
                            return false;
                    }
                    break;


                // odd row
                case 1:
                    // check jumping/not jumping
                    switch (Math.Abs(fromRow - toRow))
                    {
                        // not jumping
                        case 1:
                            if (toCol != fromCol && toCol != fromCol - 1)
                                return false;
                            break;

                        // jumping
                        case 2:
                            // jump left
                            if (toCol == fromCol - 1)
                                jumpedCol = fromCol - 1;
                            // jump right
                            else if (toCol == fromCol + 1)
                                jumpedCol = fromCol;
                            else
                                return false;
                            jumpedRow = (fromRow + toRow) / 2;
                            // check that the jumped piece is of the opposite color
                            if (!oppositeColors(fromRow, fromCol, jumpedRow, jumpedCol))
                                return false;
                            break;

                        // anything else is invalid
                        default:
                            return false;
                    }
                    break;


                // not sure how this could even happen
                default:
                    return false;
            }

            // remove the piece from its previous position
            Globals.board[fromRow, fromCol] = Globals.EMPTY_CHAR;

            // place the piece at its new position
            switch (piece)
            {
                case Globals.WHITE_CHAR:
                    Globals.board[toRow, toCol] = Globals.WHITE_CHAR;
                    break;
                case Globals.BLACK_CHAR:
                    Globals.board[toRow, toCol] = Globals.BLACK_CHAR;
                    break;
                case Globals.WHITE_KING_CHAR:
                    Globals.board[toRow, toCol] = Globals.WHITE_KING_CHAR;
                    break;
                case Globals.BLACK_KING_CHAR:
                    Globals.board[toRow, toCol] = Globals.BLACK_KING_CHAR;
                    break;
                default:
                    break;
            }

            // remove the jumped piece if there was a jump
            if (jumpedCol != -1 && jumpedRow != -1)
                Globals.board[jumpedRow, jumpedCol] = Globals.EMPTY_CHAR;

            // alternate turn
            switch (Globals.turn)
            {
                case Globals.BLACK_TURN:
                    if (toRow == Globals.BOARD_SIZE - 1)
                        Globals.board[toRow, toCol] = Globals.BLACK_KING_CHAR;
                    Globals.turn = Globals.WHITE_TURN;
                    break;
                case Globals.WHITE_TURN:
                    if (toRow == 0)
                        Globals.board[toRow, toCol] = Globals.WHITE_KING_CHAR;
                    Globals.turn = Globals.BLACK_TURN;
                    break;

                default:
                    break;
            }

            // redraw the new board
            drawBoard();

            return true; // move was valid
        }

        // returns true if the piece at row1, col1 is the color opposite the piece at row2, col2
        // returns false otherwise
        private bool oppositeColors(int row1, int col1, int row2, int col2)
        {
            // check that both positions are on the board, return false if not
            if (row1 < 0 || row2 < 0 || col1 < 0 || col2 < 0 || row1 >= Globals.BOARD_SIZE ||
                col1 >= Globals.BOARD_SIZE / 2 || row2 >= Globals.BOARD_SIZE || col2 >= Globals.BOARD_SIZE / 2)
                return false;

            switch (Globals.board[row1, col1])
            {
                case Globals.BLACK_CHAR:
                case Globals.BLACK_KING_CHAR:
                    if (Globals.board[row2, col2] == Globals.WHITE_CHAR || Globals.board[row2, col2] == Globals.WHITE_KING_CHAR)
                        return true;
                    break;

                case Globals.WHITE_CHAR:
                case Globals.WHITE_KING_CHAR:
                    if (Globals.board[row2, col2] == Globals.BLACK_CHAR || Globals.board[row2, col2] == Globals.BLACK_KING_CHAR)
                        return true;
                    break;
                default:
                    return false;
            }
            return false;
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

        public string calltoWindowsExplorer()
        {
            //string newfilepath = "";
           // System.Diagnostics.Process completepath;

            OpenFileDialog dlgOpenReciprocityFile = new OpenFileDialog(); dlgOpenReciprocityFile.InitialDirectory = @"C:\";

            dlgOpenReciprocityFile.Filter = "Executable Files (*.exe)|*.*";
            //newfilepath = 
            dlgOpenReciprocityFile.FilterIndex = 1;

            dlgOpenReciprocityFile.RestoreDirectory = true;
            if (dlgOpenReciprocityFile.ShowDialog() == DialogResult.Cancel) ;

            //completepath = Process.Start("explorer.exe", "/select," + newfilepath);

            //mpletepath = "/select, \"" + newfilepath + "\"";
            return dlgOpenReciprocityFile.FileName;
        }


        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }


        //////////////////////////////////////////////////////////////////////////////////////
        // Name: private void loadLogToolStripMenuItem_Click(object sender, EventArgs e)    //
        // Description: Loads the coords from the file in the following format              //
        //              [INITAL ROW] [INITIAL COL] [DESTINATION ROW] [ DESTINATION COL]     //
        //              Then the move() can be called and the four variables can be passed. //
        //////////////////////////////////////////////////////////////////////////////////////
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

                while (true)
                {
                    //Reads in the line and splits it into different variables.
                    //Read in format: 10 15 20 25
                    string cordSplit = reader.ReadLine();

                    if (cordSplit == null) //If the line is null it's EOF. Break to end of loop.
                    {
                        break;
                    }

                    string[] cords = cordSplit.Split(' '); // Splits the cords into individual variables

                    int initRow = int.Parse(cords[0]); //cords[0] = 10 // x = 10
                    int initCol = int.Parse(cords[1]); //cords[1] = 15 // y = 15
                    int destRow = int.Parse(cords[2]); //cords[2] = 20 // y = 20
                    int destCol = int.Parse(cords[3]); //cords[3] = 25 // y = 25
                }   

                }
            }


        //////////////////////////////////////////////////////////////////////////////////////
        // Name: private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)    //
        // Description: Checks to see if a file exits with name = log.txt in the dir. If it //
        //              does it deletes it so that there is no duplicate files. This        //
        //              function is designed to write the variables in a formatted fasion.  //
        //              HOWEVER, variables will have to be passed to this each time it      //
        //              writes to the file.                                                 //
        //////////////////////////////////////////////////////////////////////////////////////
        // EXAMPLE PRINTOUT:                                                                //
        // <[Inital Row: #] [Initial Col: #]> <[Destination Row: #] [Destination Col: #]>   //
        //////////////////////////////////////////////////////////////////////////////////////
        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //THIS IS HARDCODED SO IT CAN COMPILE. THESE VARS MUST BE SENT TO THIS FUCNTION!
            ////////////////////////////////////////////////////////
            int initRow = 0, initCol = 0, destRow = 0, destCol = 0; 
            ////////////////////////////////////////////////////////

            string filePath = @"log.txt"; //PATH TO FILE HERE

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write($"<[Initial Row: {initRow}]");
                writer.Write($" [Initial Col: {initCol}]>");
                writer.Write($"<[Destination Row: {destRow}]");
                writer.WriteLine($" [Destination Col: {destCol}]>");

            }

        }


        ///////////////////////////////////////////////////////////////////////////////////////
        // Name: private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e) //
        // Description: Starts a timer set to 5000ms(5s) and sets an interval to the same.   //
        ///////////////////////////////////////////////////////////////////////////////////////
        private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 5000;
            timer.Enabled = true;

            //timer.Stop();

        }


        ///////////////////////////////////////////////////////////////////////////////////////
        // Name: private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)  //
        // Description: Starts a timer set to 10000ms(10s) and sets an interval to the same. //
        ///////////////////////////////////////////////////////////////////////////////////////
        private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 10000;
            timer.Enabled = true;

            //timer.Stop();

        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e) //
        // Description: Starts a timer set to 20000ms(20s) and sets an interval to the same.   //
        /////////////////////////////////////////////////////////////////////////////////////////
        private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 20000;
            timer.Enabled = true;

            //timer.Stop();

        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void minuteToolStripMenuItem_Click(object sender, EventArgs e)        //
        // Description: Starts a timer set to 60000ms(60s) and sets an interval to the same.   //
        /////////////////////////////////////////////////////////////////////////////////////////
        private void minuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 60000;
            timer.Enabled = true;

            //timer.Stop();

        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void noLimitToolStripMenuItem_Click(object sender, EventArgs e)       //
        // Description: Starts a timer set to Int32.MaxValue and sets an interval to the same. //
        /////////////////////////////////////////////////////////////////////////////////////////
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


        private void player1ComputerButton_Click(object sender, EventArgs e)
        {
            string path = calltoWindowsExplorer();
            load_executable(path);
            player1FileLabel.Text = path;
        }

        private void player2ComputerButton_Click(object sender, EventArgs e)
        {
            string path = calltoWindowsExplorer();
            load_executable(path);
            player2FileLabel.Text = path;
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {

        }

        private void resetGameButton_Click(object sender, EventArgs e)
        {
            // change later if we don't want to create a new instance and close the current program instance

            System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
            this.Close(); // close current instance
        }

        private void load_executable(string filename)
        {
            // custom function to load an executable
            Process load = new Process();
            load.StartInfo.FileName = filename;
            load.StartInfo.CreateNoWindow = true; // no need for executables that just return moves
        }

        private void player1HumanButton_Click(object sender, EventArgs e)
        {
            player1FileLabel.Text = "Human";
        } 

        private void player2HumanButton_Click(object sender, EventArgs e)
        {
            player2FileLabel.Text = "Human";
        }




        // ignore these, creation code is too ugly to mess with in other file
        // created when moving the checkerboard over more

        private void player1FileLabel_Click(object sender, EventArgs e)
        {

        }

        private void player2FileLabel_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void space31_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox34_Click(object sender, EventArgs e)
        {

        }

        private void space30_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox36_Click(object sender, EventArgs e)
        {

        }

        private void space29_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox38_Click(object sender, EventArgs e)
        {

        }

        private void space28_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox40_Click(object sender, EventArgs e)
        {

        }

        private void space27_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox42_Click(object sender, EventArgs e)
        {

        }

        private void space26_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox44_Click(object sender, EventArgs e)
        {

        }

        private void space25_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox46_Click(object sender, EventArgs e)
        {

        }

        private void space24_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox48_Click(object sender, EventArgs e)
        {

        }

        private void space23_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox50_Click(object sender, EventArgs e)
        {

        }

        private void space22_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox52_Click(object sender, EventArgs e)
        {

        }

        private void space21_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox54_Click(object sender, EventArgs e)
        {

        }

        private void space20_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox56_Click(object sender, EventArgs e)
        {

        }

        private void space19_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox58_Click(object sender, EventArgs e)
        {

        }

        private void space18_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox60_Click(object sender, EventArgs e)
        {

        }

        private void space17_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox62_Click(object sender, EventArgs e)
        {

        }

        private void space16_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox64_Click(object sender, EventArgs e)
        {

        }

        private void space15_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox18_Click(object sender, EventArgs e)
        {

        }

        private void space14_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox20_Click(object sender, EventArgs e)
        {

        }

        private void space13_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox22_Click(object sender, EventArgs e)
        {

        }

        private void space12_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox24_Click(object sender, EventArgs e)
        {

        }

        private void space11_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox26_Click(object sender, EventArgs e)
        {

        }

        private void space10_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox28_Click(object sender, EventArgs e)
        {

        }

        private void space9_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox30_Click(object sender, EventArgs e)
        {

        }

        private void space8_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox32_Click(object sender, EventArgs e)
        {

        }

        private void space7_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {

        }

        private void space6_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {

        }

        private void space5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void space4_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {

        }

        private void space3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void space2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {

        }

        private void space1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void space0_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

    }
}
