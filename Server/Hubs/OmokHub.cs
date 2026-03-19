using Microsoft.AspNetCore.SignalR;
using Server.Models;
using Server.Services;

namespace Server.Hubs
{
    /// <summary>
    /// 클라이언트와의 통신을 담당하는 클래스입니다.
    /// </summary>
    public class OmokHub : Hub
    {
        private readonly RoomManager _roomManager;

        public OmokHub(RoomManager roomManager)
        { 
            _roomManager = roomManager;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[Connected] Client ID: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string connectionId = Context.ConnectionId;
            Console.WriteLine($"[Disconnected] Client ID: {connectionId}");

            // 클라이언트의 연결이 끊길 경우, 그 클라이언트가 있던 방을 할당 해제
            var room = _roomManager.FindRoomByConnectionId(connectionId);

            if (room != null)
            {
                if (room.Status != RoomStatus.Finished)
                { 
                    room.Status = RoomStatus.Finished;
                    await Clients.Group(room.RoomId).SendAsync("OpponentLeft");
                }

                if (_roomManager.RemoveRoom(room.RoomId))
                {
                    Console.WriteLine($"[메모리 정리] {room.RoomId} 방 폭파 완료 (원인: 유저 이탈)");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task QuickMatch(string nickname)
        {
            var player = new Player { ConnectionId = Context.ConnectionId, Nickname = nickname };
            var room = _roomManager.QuickMatch(player);

            await Groups.AddToGroupAsync(Context.ConnectionId, room.RoomId);
            await Clients.Caller.SendAsync("Matched", room.RoomId);

            if (room.Status == RoomStatus.Playing)
            {
                await Clients.Group(room.RoomId).SendAsync("PlayerJoined", nickname);
            }
        }

        /// <summary>
        /// 유저가 지정한 좌표에 착수합니다.
        /// </summary>
        /// <param name="roomId">방 고유 식별자</param>
        /// <param name="x">x 좌표</param>
        /// <param name="y">y 좌표</param>
        /// <returns></returns>
        public async Task PlaceStone(string roomId, int x, int y)
        {
            var room = _roomManager.GetRoom(roomId);
            if (room == null || room.Status != RoomStatus.Playing) return;

            string connectionId = Context.ConnectionId;
            StoneColor requestColor = StoneColor.None;

            if (room.Player1 != null && room.Player1.ConnectionId == connectionId)
            {
                requestColor = room.Player1.Color;
            }
            else if (room.Player2 != null && room.Player2.ConnectionId == connectionId)
            {
                requestColor = room.Player2.Color;
            }
            else
            {
                return;
            }

            if (room.CurrentTurn != requestColor)
            {
                return;
            }

            

            if (room.Board.PlaceStone(x, y, requestColor))
            {
                room.SwitchTurn();

                await Clients.Group(roomId).SendAsync("StonePlaced", x, y, (int) requestColor);

                if (room.Board.CheckWin(x, y))
                {
                    room.Status = RoomStatus.Finished;
                    await Clients.Group(roomId).SendAsync("GameEnded", (int)requestColor);
                }
                else
                {
                    await Clients.Group(roomId).SendAsync("TurnChanged", (int)room.CurrentTurn);
                }
            }

        }
    }
}
