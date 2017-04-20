﻿using System;
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

        

        
    }
}
