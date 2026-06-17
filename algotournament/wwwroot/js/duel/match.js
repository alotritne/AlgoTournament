document.addEventListener("DOMContentLoaded", async () => {
  const config = window.duelMatchConfig;
  if (!config || !config.roomCode) return;

  const timerBar = document.getElementById("matchTimerBar");
  const timeRemainingEl = document.getElementById("timeRemaining");
  const myStatusEl = document.getElementById("myStatus");
  const opponentStatusEl = document.getElementById("opponentStatus");
  const mySubCountEl = document.getElementById("mySubCount");
  const oppSubCountEl = document.getElementById("oppSubCount");
  const feedList = document.getElementById("submissionFeed");
  const submitForm = document.getElementById("duelSubmitForm");
  const submitBtn = document.getElementById("submitBtn");
  const sourceCodeEl = document.getElementById("sourceCode");
  const languageEl = document.getElementById("language");
  const matchEndOverlay = document.getElementById("matchEndOverlay");
  const matchEndTitle = document.getElementById("matchEndTitle");
  const matchEndMessage = document.getElementById("matchEndMessage");

  let endsAtMs = null;
  let matchEnded = false;

  function parseUtcMs(isoString) {
    if (!isoString) return null;
    const s = isoString.trim();
    if (s.endsWith("Z") || /[+-]\d{2}:\d{2}$/.test(s)) {
      return new Date(s).getTime();
    }
    return new Date(s + "Z").getTime();
  }

  function initTimer() {
    if (!timerBar) return;

    const endsAtStr = timerBar.dataset.endsAt;
    const startedAtStr = timerBar.dataset.startedAt;
    const durationMin = parseInt(timerBar.dataset.durationMinutes || "0", 10);

    endsAtMs = parseUtcMs(endsAtStr);

    if ((!endsAtMs || isNaN(endsAtMs)) && startedAtStr && durationMin > 0) {
      const startedMs = parseUtcMs(startedAtStr);
      if (startedMs) endsAtMs = startedMs + durationMin * 60 * 1000;
    }
  }

  function updateTimer() {
    if (!endsAtMs || !timeRemainingEl || matchEnded) return;
    const diff = endsAtMs - Date.now();
    if (diff <= 0) {
      timeRemainingEl.textContent = "00:00";
      return;
    }
    const mins = Math.floor(diff / 60000);
    const secs = Math.floor((diff % 60000) / 1000);
    timeRemainingEl.textContent = `${String(mins).padStart(2, "0")}:${String(secs).padStart(2, "0")}`;
    if (diff < 60000 && timerBar) timerBar.classList.add("duel-timer-warning");
  }

  function disableMatchInputs() {
    matchEnded = true;
    if (submitBtn) submitBtn.disabled = true;
    if (sourceCodeEl) sourceCodeEl.disabled = true;
    if (languageEl) languageEl.disabled = true;
  }

  function showMatchEndOverlay(title, message) {
    disableMatchInputs();
    if (matchEndOverlay) {
      matchEndOverlay.hidden = false;
      if (matchEndTitle) matchEndTitle.textContent = title;
      if (matchEndMessage) matchEndMessage.textContent = message;
    }
  }

  initTimer();
  updateTimer();
  setInterval(updateTimer, 1000);

  function addFeedEntry(text) {
    if (!feedList) return;
    const li = document.createElement("li");
    li.textContent = text;
    feedList.prepend(li);
  }

  function handleSubmissionUpdate(data) {
    if (matchEnded) return;
    const isMe = data.userId === config.currentUserId;
    const statusEl = isMe ? myStatusEl : opponentStatusEl;
    const countEl = isMe ? mySubCountEl : oppSubCountEl;
    if (statusEl) statusEl.textContent = data.status;
    if (countEl) countEl.textContent = data.submissionCount;
    addFeedEntry(
      `${new Date().toLocaleTimeString()} — ${data.handle}: ${data.status} (#${data.submissionCount})`,
    );
  }

  let connection;
  try {
    connection = await window.DuelHubClient.joinRoom(
      config.hubUrl,
      config.roomCode,
    );
  } catch (err) {
    console.error("Match hub connection failed", err);
  }

  if (connection) {
    window.DuelHubClient.on(connection, "MatchStarted", (data) => {
      if (data.endsAt) {
        endsAtMs = parseUtcMs(data.endsAt);
        updateTimer();
      }
    });

    window.DuelHubClient.on(
      connection,
      "SubmissionStatusChanged",
      handleSubmissionUpdate,
    );

    window.DuelHubClient.on(connection, "PlayerAccepted", (data) => {
      handleSubmissionUpdate({
        userId: data.userId,
        handle: data.handle,
        status: "Accepted",
        submissionCount: parseInt(
          (data.userId === config.currentUserId ? mySubCountEl : oppSubCountEl)
            ?.textContent || "0",
          10,
        ),
      });

      const isMe = data.userId === config.currentUserId;
      if (isMe) {
        showMatchEndOverlay("YOU SOLVED IT!", "Waiting for result...");
      } else {
        showMatchEndOverlay("OPPONENT SOLVED THE PROBLEM", "MATCH ENDED");
      }
    });

    window.DuelHubClient.on(connection, "MatchFinished", (data) => {
      const isMeWinner = data.winnerUserId === config.currentUserId;
      const isDraw = data.isDraw;

      if (data.solvedFirst) {
        if (isMeWinner) {
          showMatchEndOverlay(
            "WINNER FOUND",
            "You solved the problem first. Redirecting...",
          );
        } else if (!isDraw) {
          showMatchEndOverlay(
            "WINNER FOUND",
            `Player @${data.winnerHandle || "opponent"} solved the problem first. Redirecting...`,
          );
        }
      } else if (isDraw) {
        showMatchEndOverlay("TIME UP — DRAW", "Redirecting...");
      } else if (isMeWinner) {
        showMatchEndOverlay("YOU WIN", "Redirecting...");
      } else {
        showMatchEndOverlay("YOU LOSE", "Redirecting...");
      }

      setTimeout(() => {
        window.location.href =
          data.redirectUrl || `/Duels/Result/${data.matchId}`;
      }, 1500);
    });
  }

  if (submitForm) {
    submitForm.addEventListener("submit", async (e) => {
      e.preventDefault();
      if (window.VscodeEditor?.flushAll) {
        window.VscodeEditor.flushAll();
      }
      if (matchEnded) return;
      if (submitBtn) submitBtn.disabled = true;

      const formData = new FormData(submitForm);
      const token = window.DuelCommon.getAntiForgeryToken();

      try {
        const response = await fetch(
          window.location.pathname + config.submitUrl,
          {
            method: "POST",
            headers: {
              "X-CSRF-TOKEN": token,
            },
            body: formData,
          },
        );

        const result = await response.json();
        if (!response.ok) {
          addFeedEntry(`Submit error: ${result.error || "Unknown error"}`);
          if (submitBtn && !matchEnded) submitBtn.disabled = false;
        } else {
          if (myStatusEl)
            myStatusEl.textContent = result.status || "Submitting";
          addFeedEntry(
            `${new Date().toLocaleTimeString()} — You: ${result.status || "Submitting"}`,
          );
          if (submitBtn && !matchEnded) submitBtn.disabled = false;
        }
      } catch (err) {
        addFeedEntry("Submit failed: network error");
        if (submitBtn && !matchEnded) submitBtn.disabled = false;
      }
    });
  }
});
