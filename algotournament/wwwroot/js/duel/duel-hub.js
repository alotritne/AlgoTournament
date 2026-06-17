window.DuelHubClient = (function () {
    let connection = null;

    async function connect(hubUrl) {
        if (connection) return connection;

        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        await connection.start();
        return connection;
    }

    async function joinRoom(hubUrl, roomCode) {
        const conn = await connect(hubUrl);
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
    }

    function stopConnection() {
        if (connection) {
            connection.stop().catch(() => {});
        }
    }

    function on(conn, eventName, handler) {
        conn.on(eventName, handler);
    }

    return { connect, joinRoom, leaveRoom, stopConnection, on, getConnection: () => connection };
})();
