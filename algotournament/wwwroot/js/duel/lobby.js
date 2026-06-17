document.addEventListener('DOMContentLoaded', async () => {
    const config = window.duelLobbyConfig;
    if (!config || !config.roomCode) return;

    const readyBtn = document.getElementById('readyBtn');
    const countdownOverlay = document.getElementById('countdownOverlay');
    const countdownNumber = document.getElementById('countdownNumber');

    function renderPlayerCard(slotIndex, participant) {
        const cardId = slotIndex === 1 ? 'player1Card' : 'player2Card';
        const card = document.getElementById(cardId);
        if (!card) return;

        if (!participant) {
            card.innerHTML = `
                <div class="duel-player-slot">PLAYER ${slotIndex}</div>
                <div class="duel-player-empty">${slotIndex === 1 ? 'Waiting for host...' : 'Waiting for opponent...'}</div>`;
            return;
        }

        const readyClass = participant.isReady ? 'is-ready' : '';
        card.innerHTML = `
            <div class="duel-player-slot">PLAYER ${slotIndex}</div>
            <div class="duel-player-handle">${participant.handle}</div>
            <div class="duel-player-rating">Rating: ${participant.rating}</div>
            <div class="duel-ready-status ${readyClass} is-connected" data-user-id="${participant.userId}">
                <span class="duel-connected-dot"></span>
                ${participant.isReady ? 'READY' : 'NOT READY'}
            </div>`;
    }

    function renderLobby(lobby) {
        if (!lobby || !lobby.participants) return;

        if (lobby.status === 4 || lobby.status === 'Cancelled') {
            alert('This duel room was closed because a player left.');
            window.location.href = '/Duels/Join';
            return;
        }

        const p1 = lobby.participants.find(p => p.slotIndex === 1);
        const p2 = lobby.participants.find(p => p.slotIndex === 2);
        renderPlayerCard(1, p1);
        renderPlayerCard(2, p2);
    }

    let connection;
    try {
        connection = await window.DuelHubClient.joinRoom(config.hubUrl, config.roomCode);
    } catch (err) {
        console.error('Failed to connect to duel hub', err);
        return;
    }

    window.DuelHubClient.on(connection, 'LobbyUpdated', lobby => {
        renderLobby(lobby);
    });

    window.DuelHubClient.on(connection, 'PlayerJoined', data => {
        renderPlayerCard(data.slotIndex, {
            userId: data.userId,
            handle: data.handle,
            rating: data.rating,
            isReady: false,
            slotIndex: data.slotIndex
        });
    });

    window.DuelHubClient.on(connection, 'PlayerLeft', data => {
        const statusEl = document.querySelector(`.duel-ready-status[data-user-id="${data.userId}"]`);
        if (statusEl) {
            const card = statusEl.closest('.duel-player-card');
            const slotIndex = card?.id === 'player1Card' ? 1 : 2;
            renderPlayerCard(slotIndex, null);
        }
    });

    window.DuelHubClient.on(connection, 'PlayerReady', data => {
        const el = document.querySelector(`.duel-ready-status[data-user-id="${data.userId}"]`);
        if (el) {
            el.classList.toggle('is-ready', data.isReady);
            el.innerHTML = `<span class="duel-connected-dot"></span> ${data.isReady ? 'READY' : 'NOT READY'}`;
        }
        if (data.userId === config.currentUserId && readyBtn) {
            readyBtn.dataset.ready = data.isReady ? 'true' : 'false';
            readyBtn.textContent = data.isReady ? 'Unready' : 'Ready';
            readyBtn.classList.toggle('duel-ready-active', data.isReady);
        }
    });

    window.DuelHubClient.on(connection, 'CountdownStarted', data => {
        if (!countdownOverlay || !countdownNumber) return;
        countdownOverlay.hidden = false;
        let remaining = data.seconds || 3;
        countdownNumber.textContent = remaining;
        const interval = setInterval(() => {
            remaining -= 1;
            if (remaining <= 0) {
                clearInterval(interval);
                countdownOverlay.hidden = true;
            } else {
                countdownNumber.textContent = remaining;
            }
        }, 1000);
    });

    window.DuelHubClient.on(connection, 'MatchStarted', () => {
        window.location.href = `/Duels/Match/${config.roomCode}`;
    });

    if (readyBtn) {
        readyBtn.addEventListener('click', async () => {
            const isReady = readyBtn.dataset.ready === 'true';
            try {
                await connection.invoke('SetReady', config.roomCode, !isReady);
            } catch (err) {
                console.error('SetReady failed', err);
            }
        });
    }

    window.addEventListener('pagehide', () => {
        window.DuelHubClient.stopConnection();
    });
});
