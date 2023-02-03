using System.Collections.Generic;

namespace UnityChess {
	public class Canon : Piece<Canon> {
		public Canon() : base(Side.None) {}
		public Canon(Side owner) : base(owner) {}

		public override string ToText() => Owner==Side.White ? "C" : "c";

		protected override IEnumerable<Movement> EnumeratePossibleMoves (
			Board board,
			Square position
		) {
			foreach (Square offset in SquareUtil.CardinalOffsets) {
				int jumped = 0;
				Square endSquare = position + offset;

				while (endSquare.IsValid()) {
					Movement testMove = new Movement(position, endSquare);
                    endSquare += offset;

                    if (board.IsOccupiedAt(testMove.End)) {
						jumped++;

					}
					if (jumped==1) { continue;}

					yield return testMove;

					if (jumped>1) break;
					
				}
			}

		}
	}
}