using Server.Models;
using Server.Services;

namespace Server.Models
{
    public class GameRoom
    {
        public string RoomId { get; set; }
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public OmokBoard Board { get; set; }
        public RoomStatus Status { get; set; }
        public StoneColor CurrentTurn {  get; set; }

        public GameRoom(string roomId, Player creator) 
        {
            RoomId = roomId;
            Player1 = creator;
            Player1.Color = StoneColor.Black;
            Board = new OmokBoard();
            Status = RoomStatus.Waiting;
            CurrentTurn = StoneColor.Black;
        }

        public void SwitchTurn()
        {
            CurrentTurn = (CurrentTurn == StoneColor.Black) ? StoneColor.White : StoneColor.Black;
        }
    }
}
