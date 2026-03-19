using Server.Models;

namespace Server.Services
{
    /// <summary>
    /// 오목판의 상태를 관리하고 승패 로직을 처리하는 클래스입니다.
    /// </summary>
    public class OmokBoard
    {
        public const int Size = 15;
        private readonly StoneColor[,] _grid;
        const int WinCount = 5;
        private readonly (int x, int y)[] Directions = {(0, 1), (1, 0), (1, 1), (-1, 1)};

        public OmokBoard()
        {
            _grid = new StoneColor[Size, Size];
        }

        /// <summary>
        /// 지정된 좌표에 돌을 착수합니다.
        /// </summary>
        /// <param name="x">x 좌표</param>
        /// <param name="y">y 좌표</param>
        /// <param name="color">착수할 돌 색상</param>
        /// <returns>착수 성공 여부 (true: 성공, false: 실패)</returns>
        public bool PlaceStone(int x, int y, StoneColor color)
        {
            if (IsInvalidPosition(x, y))
            { 
                return false; 
            }

            _grid[x, y] = color;
            return true;
        }

        /// <summary>
        /// 해당 좌표가 오목판 범위를 벗어났거나, 이미 돌이 놓여 있는지 확인합니다.
        /// </summary>
        /// <param name="x">x 좌표</param>
        /// <param name="y">y 좌표</param>
        /// <returns>착수 불가 여부 (true: 착수 불가, false: 착수 가능)</returns>
        private bool IsInvalidPosition(int x, int y)
        {
            return (x < 0 || x >= Size || y < 0 || y >= Size || _grid[x, y] != StoneColor.None);
        }

        /// <summary>
        /// 해당 좌표가 바둑판 범위 내에 있고, 주어진 돌과 색상이 같은지 확인합니다.
        /// </summary>
        /// <param name="x">x 좌표</param>
        /// <param name="y">y 좌표</param>
        /// <param name="color">확인할 돌 색상</param>
        /// <returns>동일 색상 여부 (true: 동일 색상, false: 다른 색상)</returns>
        private bool IsSameStone(int x, int y, StoneColor color)
        {
            return (x >= 0 && x < Size && y >= 0 && y < Size && _grid[x, y] == color);
        }

        /// <summary>
        /// 최근 둔 돌을 기준으로 특정 방향으로 연속된 돌의 개수를 계산합니다.
        /// </summary>
        /// <param name="x">기준 x 좌표</param>
        /// <param name="y">기준 y 좌표</param>
        /// <param name="dx">x 방향</param>
        /// <param name="dy">y 방향</param>
        /// <returns>연결된 돌 갯수</returns>
        private int CountStone(int x, int y, int dx, int dy)
        {
            StoneColor color = _grid[x, y];
            int count = 1;

            int nx = x + dx;
            int ny = y + dy;

            while (IsSameStone(nx, ny, color))
            {
                count++;
                nx += dx;
                ny += dy;
            }

            nx = x - dx;
            ny = y - dy;

            while (IsSameStone(nx, ny, color))
            {
                count++;
                nx -= dx;
                ny -= dy;
            }

            return count;
        }

        /// <summary>
        /// 최근 둔 돌을 기준으로 5목이 완성됐는지 확인하여 승패를 판정합니다.
        /// </summary>
        /// <param name="x">x 좌표</param>
        /// <param name="y">y 좌표</param>
        /// <returns>승리 여부 (true: 승리, false: 계속 진행)</returns>
        public bool CheckWin(int x, int y)
        {
            foreach (var d in Directions)
            {
                if (CountStone(x, y, d.x, d.y) >= WinCount) return true;
            }

            return false;
        }
    }
}
