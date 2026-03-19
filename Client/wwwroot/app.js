// 1. HTML 요소 가져오기
const lobbyScreen = document.getElementById('lobby-screen');
const gameScreen = document.getElementById('game-screen');
const joinButton = document.getElementById('joinButton');
const statusText = document.getElementById('statusText');
const canvas = document.getElementById('omokBoard');
const ctx = canvas.getContext('2d');

// 게임 상태 변수
let currentRoomId = '';
let isGameStarted = false;
let isGameOver = false;
const boardSize = 15;
const cellSize = 600 / boardSize; // 캔버스가 600px이므로 한 칸은 40px
const margin = cellSize / 2;      // 선이 시작하는 여백 (20px)

const serverUrl = CONFIG.SERVER_URL;

// 2. SignalR 서버와 연결 설정
const connection = new signalR.HubConnectionBuilder()
    .withUrl(serverUrl)
    .build();

// 3. 서버에서 날아오는 이벤트(메시지) 받기
connection.on("PlayerJoined", (nickname) => {
    isGameStarted = true;
    statusText.innerText = `[${nickname}]님이 방에 참여했습니다! 게임을 시작하세요.`;
});

connection.on("OpponentLeft", () => {
    if (isGameOver) return;

    isGameOver = true;
    alert("상대방이 도망갔습니다! 기권승!");
    statusText.innerText = "상대방 연결 끊김. 새로고침하여 다시 시작하세요.";
});

connection.on("StonePlaced", (x, y, colorValue) => {
    drawStone(x, y, colorValue);
});

connection.on("TurnChanged", (turnValue) => {
    const turnText = turnValue === 1 ? "흑돌(Black)" : "백돌(White)";
    statusText.innerText = `현재 차례: ${turnText}`;
});

connection.on("GameEnded", (colorValue) => {
    isGameOver = true;
    const winnerText = colorValue === 1 ? "흑돌" : "백돌";
    alert(`🎉 게임 종료! ${winnerText}이(가) 승리했습니다!`);
    statusText.innerText = `게임 종료 (${winnerText} 승리)`;
});

// 4. 서버와 연결 시작
connection.start().then(() => {
    console.log("서버와 성공적으로 연결되었습니다!");
}).catch(err => console.error("연결 에러: ", err));

connection.on("Matched", (roomId) => {
    currentRoomId = roomId; // 서버가 몰래 파준 방 번호를 내 변수에 쏙 저장합니다.

    // 화면 전환 및 바둑판 그리기
    lobbyScreen.style.display = 'none';
    gameScreen.style.display = 'block';
    drawBoard();

    statusText.innerText = "상대방을 찾는 중입니다... (매칭 대기)";
});

// 2. 2명이 꽉 차서 게임이 시작될 때 텍스트
connection.on("PlayerJoined", (nickname) => {
    statusText.innerText = `[${nickname}]님과 매칭되었습니다! 흑돌부터 시작합니다.`;
});

// 3. 방 입장 버튼 클릭 이벤트 수정 (방 번호를 묻지 않음!)
joinButton.onclick = async () => {
    const nickname = document.getElementById('nicknameInput').value;

    if (!nickname) {
        alert("닉네임을 입력해주세요.");
        return;
    }

    try {
        // 방 번호 없이 닉네임만 던지면서 "나 매칭 잡아줘!" 라고 외칩니다.
        await connection.invoke("QuickMatch", nickname);
    } catch (err) {
        console.error("매칭 실패: ", err);
    }
};

// 6. 바둑판 클릭 시 돌 두기 (서버로 전송)
canvas.onclick = async (event) => {
    if (!currentRoomId || !isGameStarted || isGameOver) return;

    // 클릭한 픽셀 좌표를 15x15 배열 인덱스(0~14)로 변환
    const rect = canvas.getBoundingClientRect();
    const clickX = event.clientX - rect.left;
    const clickY = event.clientY - rect.top;

    const x = Math.round((clickX - margin) / cellSize);
    const y = Math.round((clickY - margin) / cellSize);

    // 보드 범위를 벗어나면 무시
    if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return;

    try {
        // 서버의 PlaceStone 메서드 호출 (서버가 검증하고 허락해야만 돌이 그려짐)
        await connection.invoke("PlaceStone", currentRoomId, x, y);
    } catch (err) {
        console.error("돌 두기 실패: ", err);
    }
};

// --- 그리기 함수들 (Canvas API) ---
function drawBoard() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.strokeStyle = '#000000';
    ctx.lineWidth = 1;

    for (let i = 0; i < boardSize; i++) {
        // 가로선
        ctx.beginPath();
        ctx.moveTo(margin, margin + i * cellSize);
        ctx.lineTo(canvas.width - margin, margin + i * cellSize);
        ctx.stroke();

        // 세로선
        ctx.beginPath();
        ctx.moveTo(margin + i * cellSize, margin);
        ctx.lineTo(margin + i * cellSize, canvas.height - margin);
        ctx.stroke();
    }
}

function drawStone(x, y, colorValue) {
    const centerX = margin + x * cellSize;
    const centerY = margin + y * cellSize;

    ctx.beginPath();
    ctx.arc(centerX, centerY, cellSize / 2 - 2, 0, 2 * Math.PI);

    // colorValue: 1은 Black, 2는 White (Enums.cs 기준)
    ctx.fillStyle = colorValue === 1 ? '#000000' : '#FFFFFF';
    ctx.fill();

    ctx.strokeStyle = '#000000';
    ctx.stroke();
}