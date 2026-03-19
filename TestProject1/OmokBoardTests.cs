using Server.Services;
using Server.Models;

namespace TestProject1
{
    public class OmokBoardTests
    {
        [Fact]
        public void Check1() // 가로로 5개를 뒀을 때 승리 확인
        {
            var board = new OmokBoard();

            board.PlaceStone(0, 0, StoneColor.Black);
            board.PlaceStone(1, 0, StoneColor.Black);
            board.PlaceStone(2, 0, StoneColor.Black);
            board.PlaceStone(3, 0, StoneColor.Black);
            board.PlaceStone(4, 0, StoneColor.Black);

            bool isWin = board.CheckWin(4, 0);

            Assert.True(isWin);

        }

        [Fact]
        public void Check2() // 돌을 둔 곳에 다시 둘 수 있는지 확인
        { 
            var board = new OmokBoard();
            board.PlaceStone(7, 7, StoneColor.Black);

            bool isSuccess = board.PlaceStone(7, 7, StoneColor.White);

            Assert.False(isSuccess);
        }
    }
}