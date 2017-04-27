using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
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

            Globals.board = new char[Globals.BOARD_SIZE, Globals.BOARD_SIZE / 2];
            initializeBoard();
            drawBoard();
            resetLocalLog();
            //movePiece(2, 0, 3, 0);
            //movePiece(5, 0, 4, 0);
            writeBoardToFile("board.txt");

            for (int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    Globals.pbs[(i * Globals.BOARD_SIZE / 2) + j].Click += new System.EventHandler(clickToMove);
                }
            }
        }

        // reads the move from a file
        private bool readMove(string fileName)
        {
            using (TextReader reader = File.OpenText(fileName))
            {

                //Error check if the file does not exist
                if (!File.Exists(fileName))
                {
                    MessageBox.Show(string.Format("File {0} does not exist or is not a text file", fileName));
                }

                string cordSplit = reader.ReadLine();

                if (cordSplit != null)
                {
                    //Reads in the line and splits it into different variables.
                    //Read in format: 10 15 20 25

                    string[] cords = cordSplit.Split(' '); // Splits the cords into individual variables

                    int initRow = int.Parse(cords[0]); //cords[0] = 10 // x = 10
                    int initCol = int.Parse(cords[1]); //cords[1] = 15 // y = 15
                    int destRow = int.Parse(cords[2]); //cords[2] = 20 // x = 20
                    int destCol = int.Parse(cords[3]); //cords[3] = 25 // y = 25

                    return movePiece(initRow, initCol, destRow, destCol);
                }

            }
            return false;
        }

        // handles the human/computer turns
        private void nextTurn()
        {
            isGameOver();
            switch (Globals.turn)
            {
                case Globals.BLACK_TURN:
                    if (!Globals.isBlackHuman)
                    {
                        Globals.humanTurn = false;
                        File.Delete("move.txt");
                        // start the timer
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        // start the computer
                        ProcessStartInfo start = load_executable(Globals.blackPath);
                        // Run the external process
                        Process proc = Process.Start(start);
                        while (!File.Exists("move.txt"))
                        {
                            // if timer is expired white wins, then return
                            if (Globals.timeOut > 0 && sw.ElapsedMilliseconds > Globals.timeOut)
                            {
                                MessageBox.Show("Program timed out, white wins.");
                                Globals.gameFinished = true;
                                proc.Kill();
                                return;
                            }

                        }
                        proc.WaitForExit();
                        // if the move is bad white wins, then return
                        if (!readMove("move.txt"))
                        {
                            MessageBox.Show("Bad move, white wins.");
                            Globals.gameFinished = true;
                            return;
                        }
                        else
                        {
                            nextTurn();
                            return;
                        }
                    }
                    else
                    {
                        Globals.humanTurn = true;
                    }
                    break;
                case Globals.WHITE_TURN:
                    if (!Globals.isWhiteHuman)
                    {
                        Globals.humanTurn = false;
                        File.Delete("move.txt");
                        // start the timer
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        // start the computer
                        ProcessStartInfo start = load_executable(Globals.whitePath);
                        // Run the external process
                        Process proc = Process.Start(start);
                        while (!File.Exists("move.txt"))
                        {
                            // if timer is expired black wins, then return
                            if (Globals.timeOut > 0 && sw.ElapsedMilliseconds > Globals.timeOut)
                            {
                                MessageBox.Show("Program timed out, black wins.");
                                Globals.gameFinished = true;
                                proc.Kill();
                                return;
                            }
                        }
                        proc.WaitForExit();
                        // if the move is bad black wins, then return
                        if (!readMove("move.txt"))
                        {
                            MessageBox.Show("Bad move, black wins.");
                            Globals.gameFinished = true;
                            return;
                        }
                        else
                        {
                            nextTurn();
                            return;
                        }
                    }
                    else
                    {
                        Globals.humanTurn = true;
                    }
                    break;
            }
        }

        // click event for the picture boxes
        private void clickToMove(object sender, EventArgs e)
        {
            string numberAsString = ((PictureBox)sender).Name.Substring(5);
            int position = Int32.Parse(numberAsString);
            if (Globals.gameStarted && Globals.humanTurn && Globals.clickedPosition == -1)
            {
                Globals.clickedPosition = position;
                ((PictureBox)sender).BackColor = System.Drawing.SystemColors.ActiveCaption;
            }
            else if (Globals.gameStarted && Globals.humanTurn && Globals.clickedPosition != -1)
            {
                Globals.pbs[Globals.clickedPosition].BackColor = System.Drawing.SystemColors.ControlDarkDark;
                int fromRow = Globals.clickedPosition / (Globals.BOARD_SIZE / 2);
                int fromCol = Globals.clickedPosition % (Globals.BOARD_SIZE / 2);
                int toRow = position / (Globals.BOARD_SIZE / 2);
                int toCol = position % (Globals.BOARD_SIZE / 2);
                Globals.clickedPosition = -1;
                movePiece(fromRow, fromCol, toRow, toCol);
                nextTurn();
            }

        }

        // check if the game is finished
        private bool isGameOver()
        {
            bool blackPiecesLeft = false;
            bool whitePiecesLeft = false;

            // see if there is a draw
            if (Globals.lastJumpCounter >= 40)
            {
                MessageBox.Show("40 turns have passed without a piece being taken. The game ends in a draw.");
                return true;
            }

            // see if either side is out of pieces
            for (int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                for (int j = 0; j < (Globals.BOARD_SIZE / 2); j++)
                {
                    switch (Globals.board[i, j])
                    {
                        case Globals.WHITE_CHAR:
                        case Globals.WHITE_KING_CHAR:
                            whitePiecesLeft = true;
                            break;
                        case Globals.BLACK_CHAR:
                        case Globals.BLACK_KING_CHAR:
                            blackPiecesLeft = true;
                            break;
                    }
                }
            }
            if (!blackPiecesLeft)
            {
                MessageBox.Show("White wins!");
                Globals.gameStarted = false;
                Globals.gameFinished = true;
                return true;
            }
            else if (!whitePiecesLeft)
            {
                MessageBox.Show("Black wins!");
                Globals.gameStarted = false;
                Globals.gameFinished = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        // sets the resource log to an empty file
        private void resetLocalLog()
        {
            File.WriteAllText("log.txt", string.Empty);
        }

        // appends a string to the local log file
        private void appendToLocalLog(string str)
        {
            File.AppendAllText("log.txt", str);
        }

        // copies the contents of the log to the specified filePath
        private void copyLocalLog(string filePath)
        {
            string[] lines = File.ReadAllLines("log.txt");
            File.WriteAllLines(filePath, lines);
        }

        // copies the contents of a text file to the local log
        private void loadLocalLog(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            File.WriteAllLines("log.txt", lines);
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
            {
                Globals.board[jumpedRow, jumpedCol] = Globals.EMPTY_CHAR;
                Globals.lastJumpCounter = 0;
            }
            else
                Globals.lastJumpCounter++;


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

            // create a string for the move
            string moveAsString = fromRow.ToString() + ' ' + fromCol.ToString() +
                ' ' + toRow.ToString() + ' ' + toCol.ToString() + '\n';

            // send the move to the log
            appendToLocalLog(moveAsString);

            // output the new board to board.txt
            writeBoardToFile("board.txt");

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

        /*private void readBoardFromFile(string fileName)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open);
            string fileString = fs.ToString();
            fs.Close();
        }*/

        // sends the board state to a text file
        private void writeBoardToFile(string fileName)
        {
            // convert the character array to a string array
            string[] boardAsStrings = new string[Globals.BOARD_SIZE];
            string tempString = "";
            for (int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                boardAsStrings[i] = "";
                for (int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    tempString = Globals.board[i, j].ToString();
                    switch(Globals.turn)
                    {
                        case Globals.BLACK_TURN:
                            switch(tempString)
                            {
                                case "b":
                                    tempString = "o";
                                    break;
                                case "B":
                                    tempString = "O";
                                    break;
                                case "w":
                                    tempString = "x";
                                    break;
                                case "W":
                                    tempString = "X";
                                    break;
                                default:
                                    tempString = " ";
                                    break;
                            }
                            break;
                        case Globals.WHITE_TURN:
                            switch (tempString)
                            {
                                case "b":
                                    tempString = "x";
                                    break;
                                case "B":
                                    tempString = "X";
                                    break;
                                case "w":
                                    tempString = "o";
                                    break;
                                case "W":
                                    tempString = "O";
                                    break;
                                default:
                                    tempString = " ";
                                    break;
                            }
                            break;
                    }
                    boardAsStrings[i] += tempString;
                }

            }

            // write each string as its own line
            File.WriteAllLines(fileName, boardAsStrings);

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
            if (dlgOpenReciprocityFile.ShowDialog() == DialogResult.Cancel)

                return "";
            ;

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


            OpenFileDialog dlgOpenReciprocityFile = new OpenFileDialog(); dlgOpenReciprocityFile.InitialDirectory = @"C:\";

            dlgOpenReciprocityFile.RestoreDirectory = true;
            if (dlgOpenReciprocityFile.ShowDialog() == DialogResult.Cancel)

                //If cancel
                return;

            string filename = dlgOpenReciprocityFile.FileName;

            using (TextReader reader = File.OpenText(filename))
            {

                //Error check if the file does not exist
                if (!File.Exists(filename))
                {
                    MessageBox.Show(string.Format("File {0} does not exist or is not a text file", filename));
                }

                string cordSplit = reader.ReadLine();

                while (cordSplit != null)
                {
                    //Reads in the line and splits it into different variables.
                    //Read in format: 10 15 20 25

                    string[] cords = cordSplit.Split(' '); // Splits the cords into individual variables

                    int initRow = int.Parse(cords[0]); //cords[0] = 10 // x = 10
                    int initCol = int.Parse(cords[1]); //cords[1] = 15 // y = 15
                    int destRow = int.Parse(cords[2]); //cords[2] = 20 // x = 20
                    int destCol = int.Parse(cords[3]); //cords[3] = 25 // y = 25

                    movePiece(initRow, initCol, destRow, destCol);
                    cordSplit = reader.ReadLine();
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
        private void saveLogToolStripMenuItem_Click(object sender, EventArgs e)
        {


            SaveFileDialog dlgSaveReciprocityFile = new SaveFileDialog(); dlgSaveReciprocityFile.InitialDirectory = @"C:\";

            dlgSaveReciprocityFile.RestoreDirectory = true;
            if (dlgSaveReciprocityFile.ShowDialog() == DialogResult.Cancel)

                //If cancel
                return;

            copyLocalLog(dlgSaveReciprocityFile.FileName);

        }


        ///////////////////////////////////////////////////////////////////////////////////////
        // Name: private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e) //
        // Description: Starts a timer set to 5000ms(5s) and sets an interval to the same.   //
        ///////////////////////////////////////////////////////////////////////////////////////
        private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.timeOut = 5000;

        }


        ///////////////////////////////////////////////////////////////////////////////////////
        // Name: private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)  //
        // Description: Starts a timer set to 10000ms(10s) and sets an interval to the same. //
        ///////////////////////////////////////////////////////////////////////////////////////
        private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.timeOut = 10000;

            //timer.Stop();

        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e) //
        // Description: Starts a timer set to 20000ms(20s) and sets an interval to the same.   //
        /////////////////////////////////////////////////////////////////////////////////////////
        private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.timeOut = 20000;

            //timer.Stop();

        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void minuteToolStripMenuItem_Click(object sender, EventArgs e)        //
        // Description: Starts a timer set to 60000ms(60s) and sets an interval to the same.   //
        /////////////////////////////////////////////////////////////////////////////////////////
        private void minuteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Globals.timeOut = 60000;

            //timer.Stop();

        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void noLimitToolStripMenuItem_Click(object sender, EventArgs e)       //
        // Description: Starts a timer set to Int32.MaxValue and sets an interval to the same. //
        /////////////////////////////////////////////////////////////////////////////////////////
        private void noLimitToolStripMenuItem_Click(object sender, EventArgs e)
        {

            Globals.timeOut = 0;

            //timer.Stop();

        }




        private void player1ComputerButton_Click(object sender, EventArgs e)
        {
            if (!Globals.gameStarted || Globals.gameFinished)
            {
                string path = calltoWindowsExplorer();
                if (path != "")
                {
                    Globals.blackPath = path;
                    player1FileLabel.Text = path;
                    Globals.isBlackHuman = false;
                }
            }
            else
            {
                MessageBox.Show("Game in progress.");
            }
        }

        private void player2ComputerButton_Click(object sender, EventArgs e)
        {
            if (!Globals.gameStarted || Globals.gameFinished)
            {
                string path = calltoWindowsExplorer();
                if (path != "")
                {
                    Globals.whitePath = path;
                    player2FileLabel.Text = path;
                    Globals.isWhiteHuman = false;
                }
            }
            else
            {
                MessageBox.Show("Game in progress.");
            }
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            if (Globals.gameFinished)
            {
                MessageBox.Show("Click restart game to restart.");
            }
            else
            {
                Globals.gameStarted = true;
                nextTurn();
            }
        }

        private void resetGameButton_Click(object sender, EventArgs e)
        {
            // change later if we don't want to create a new instance and close the current program instance

            //System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
            //this.Close(); // close current instance

            initializeBoard();
            drawBoard();
            resetLocalLog();
            Globals.turn = Globals.BLACK_TURN;
            Globals.gameStarted = false;
            Globals.gameFinished = false;
            Globals.humanTurn = false;
            writeBoardToFile("board.txt");
        }

        private ProcessStartInfo load_executable(string filename)
        {
            // custom function to load an executable
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = filename;
            start.CreateNoWindow = true;// no need for executables that just return moves
            start.WindowStyle = ProcessWindowStyle.Hidden;

            return start;
        }

        private void player1HumanButton_Click(object sender, EventArgs e)
        {
            if (!Globals.gameStarted || Globals.gameFinished)
            {
                player1FileLabel.Text = "Human";
                Globals.isBlackHuman = true;
            }
            else
            {
                MessageBox.Show("Game in progress.");
            }
        }

        private void player2HumanButton_Click(object sender, EventArgs e)
        {
            if (!Globals.gameStarted || Globals.gameFinished)
            {
                player2FileLabel.Text = "Human";
                Globals.isWhiteHuman = true;
            }
            else
            {
                MessageBox.Show("Game in progress.");
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {

        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        // Name: private void exitToolStripMenuItem_Click(object sender, EventArgs e)            //
        // Description: Asks the user if they would like to exit the application or not. If yes  //
        //              the user will exit the application. If no, the user returns to the form. //
        ///////////////////////////////////////////////////////////////////////////////////////////
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure to exit?", "Exit CheckItCheckers", MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.Yes)
            {
                Application.Exit();
            }


        }

        private void space19_Click(object sender, EventArgs e)
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

        private void space31_Click(object sender, EventArgs e)
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
    }
}
