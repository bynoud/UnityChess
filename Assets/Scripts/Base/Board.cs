﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityChess.Base
{
    public class Board
    {
        public List<BasePiece> BoardPosition { get; set; }
        public static Empty EmptyPiece = new Empty();
        public static Invalid InvalidPiece = new Invalid();

        //used for initial board
        public Board()
        {
            this.BoardPosition = new List<BasePiece>(120);
            this.SetStartingPosition();
        }

        //used for copying a board
        public Board(Board board)
        {
            this.BoardPosition = new List<BasePiece>(board.BoardPosition);
        }

        //used after a move is made and board (BoardEntry) is to be added to a BoardList
        public Board(Board board, Movement move)
        {
            this.BoardPosition = new List<BasePiece>(board.BoardPosition);
            this.MovePiece(move);
        }

        public void SetStartingPosition()
        {
            int i;
            //Will start by setting all squares as invalid, then change to other Piecetypes as necessary
            for (i = 0; i < 120; i++)
            {
                this.BoardPosition[i] = InvalidPiece;
            }

            //Row 2/Rank 7 and Row 7/Rank 2, both rows of pawns
            for (i = 31; i < 39; i++)
            {
                this.BoardPosition[i] = new Pawn(new Square(i), PieceType.BlackPawn);
                this.BoardPosition[i + 50] = new Pawn(new Square(i + 50), PieceType.WhitePawn);
            }

            //Rows 3-6/Ranks 6-3, empty inbetween squares
            for (i = 41; i < 79; i++)
            {
                this.BoardPosition[i] = EmptyPiece;
            }

            //Rows 1 & 8/Ranks 8 & 1, back rows for both players
            this.BoardPosition[21] = new Rook(new Square(21), PieceType.BlackRook);
            this.BoardPosition[22] = new Knight(new Square(22), PieceType.BlackKnight);
            this.BoardPosition[23] = new Bishop(new Square(23), PieceType.BlackBishop);
            this.BoardPosition[24] = new Queen(new Square(24), PieceType.BlackQueen);
            this.BoardPosition[25] = new King(new Square(25), PieceType.BlackKing);
            this.BoardPosition[26] = new Bishop(new Square(26), PieceType.BlackBishop);
            this.BoardPosition[27] = new Knight(new Square(27), PieceType.BlackKnight);
            this.BoardPosition[28] = new Rook(new Square(28), PieceType.BlackRook);

            this.BoardPosition[91] = new Rook(new Square(91), PieceType.WhiteRook);
            this.BoardPosition[92] = new Knight(new Square(92), PieceType.WhiteKnight);
            this.BoardPosition[93] = new Bishop(new Square(93), PieceType.WhiteBishop);
            this.BoardPosition[94] = new Queen(new Square(94), PieceType.WhiteQueen);
            this.BoardPosition[95] = new King(new Square(95), PieceType.WhiteKing);
            this.BoardPosition[96] = new Bishop(new Square(96), PieceType.WhiteBishop);
            this.BoardPosition[97] = new Knight(new Square(97), PieceType.WhiteKnight);
            this.BoardPosition[98] = new Rook(new Square(98), PieceType.WhiteRook);
        }

        public void MovePiece(Movement move)
        {
            //update board
            this.BoardPosition[Square.squareAsIndex(move.Piece.Position)] = EmptyPiece;
            this.BoardPosition[Square.squareAsIndex(move.End)] = move.Piece;

            //update piece
            move.Piece.HasMoved = true;
            move.Piece.Position = move.End;

            //update possible moves list for every piece on board
            foreach (BasePiece BP in this.BoardPosition)
            {
                if (BP.Type != PieceType.Invalid && BP.Type != PieceType.Empty) { ((Piece)BP).Update(this); }
            }
        }
    }
}