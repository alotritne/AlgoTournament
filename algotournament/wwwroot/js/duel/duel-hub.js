window.DuelHubClient = (function () {
    let connection = null;
    let currentRoomCode = null;
    const reconnectHandlers = [];

    async function connect(hubUrl) {
        if (connection) return connection;

        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        connection.onreconnected(async () => {
            if (currentRoomCode) {
                try {
                    await connection.invoke('JoinRoomGroup', currentRoomCode);
                } catch (err) {
                    console.warn('Re-join room group failed', err);
                }
            }
            reconnectHandlers.forEach(h => { try { h(); } catch (e) { console.warn(e); } });
        });

        await connection.start();
        return connection;
    }

    async function joinRoom(hubUrl, roomCode) {
        const conn = await connect(hubUrl);
        currentRoomCode = roomCode;
        await conn.invoke('JoinRoomGroup', roomCode);
        return conn;
    }

    async function leaveRoom(roomCode) {
        if (!connection) return;
        try {
            await connection.invoke('LeaveRoomGroup', roomCode);
        } catch (err) {
            console.warn('LeaveRoomGroup failed', err);
        }
        if (currentRoomCode === roomCode) currentRoomCode = null;
    }

    function stopConnection() {
        if (connection) {
            connection.stop().catch(() => {});
            connection = null;
            currentRoomCode = null;
        }
    }

    function on(conn, eventName, handler) {
        conn.on(eventName, handler);
    }

    function onReconnect(handler) {
        reconnectHandlers.push(handler);
    }

    return { connect, joinRoom, leaveRoom, stopConnection, on, onReconnect, getConnection: () => connection };
})();
