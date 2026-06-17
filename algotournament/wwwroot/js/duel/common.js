(function () {
    window.DuelCommon = {
        normalizeRoomCode(value) {
            return (value || '').trim().toUpperCase();
        },

        copyRoomCode(code) {
            const normalized = this.normalizeRoomCode(code);
            if (!normalized) return Promise.reject(new Error('No room code'));
            return navigator.clipboard.writeText(normalized);
        },

        getAntiForgeryToken() {
            const input = document.querySelector('input[name="__RequestVerificationToken"]');
            return input ? input.value : '';
        },

        bindRoomCodeInputs() {
            document.querySelectorAll('.duel-room-code-input').forEach(el => {
                el.addEventListener('input', () => {
                    el.value = el.value.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 6);
                });
            });
        }
    };

    document.addEventListener('DOMContentLoaded', () => {
        window.DuelCommon.bindRoomCodeInputs();

        const copyBtn = document.getElementById('copyRoomCodeBtn');
        if (copyBtn) {
            copyBtn.addEventListener('click', async () => {
                try {
                    await window.DuelCommon.copyRoomCode(copyBtn.dataset.roomCode);
                    copyBtn.textContent = 'Copied!';
                    setTimeout(() => { copyBtn.textContent = 'Copy Room Code'; }, 1500);
                } catch {
                    copyBtn.textContent = 'Copy failed';
                }
            });
        }
    });
})();
