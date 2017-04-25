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

            Globals.board = new char[Globals.BOARD_SIZE, Globals.BOARD_SIZE / 2];
            initializeBoard();
            drawBoard();
        }

        // sets the board to the initial state
        private void initializeBoard()
        {
            // black pieces
            for(int i = 0; i < 3; i++)
            {
                for(int j = 0; j < Globals.BOARD_SIZE / 2; j++)
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
            for(int i = 0; i < Globals.BOARD_SIZE; i++)
            {
                for(int j = 0; j < Globals.BOARD_SIZE / 2; j++)
                {
                    switch(Globals.board[i,j])
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
            switch(Globals.turn)
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
       
        //  checks and completes a move
        //      returns true if the move was made
        //      returns false if the move was invalid
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
            switch(piece)
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
                    switch(Math.Abs(fromRow - toRow))
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
            if(jumpedCol != -1 && jumpedRow != -1)
                Globals.board[jumpedRow, jumpedCol] = Globals.EMPTY_CHAR;

            // alternate turn
            switch(Globals.turn)
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void player1HumanButton_Click(object sender, EventArgs e)
        {
            // TODO: set player 1 to human

            player1FileLabel.Text = "Human";
        }

        private void player2HumanButton_Click(object sender, EventArgs e)
        {
            // TODO: set player 2 to human

            player2FileLabel.Text = "Human";
        }

        private void startGameButton_Click(object sender, EventArgs e)
        {
            
        }

        private void resetGameButton_Click(object sender, EventArgs e)
        {

        }

        private void fiveSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tenSecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void twentySecondsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void minuteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void noLimitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
