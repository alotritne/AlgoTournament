import { fetchMockData } from './data-loader.js';

document.addEventListener('DOMContentLoaded', async () => {
    if (!window.location.pathname.includes('profile.html')) return;

    const sidebar = document.getElementById('profile-sidebar');
    const mainCol = document.getElementById('profile-main');
    if (!sidebar || !mainCol) return;

    // Load user data
    const users = await fetchMockData('users.json');
    const user = (users && users.length > 0) ? users[0] : null;

    if (!user) {
        sidebar.innerHTML = '<p style="font-weight:700;" data-i18n="profile.user_not_found">User not found.</p>';
        mainCol.innerHTML = '';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    // Determine rank color
    let color = 'var(--text-color)';
    let rankTitle = 'profile.newbie';
    if (user.rating >= 2400) { color = 'red'; rankTitle = 'profile.legendary_grandmaster'; }
    else if (user.rating >= 1900) { color = '#d2691e'; rankTitle = 'profile.grandmaster'; }
    else if (user.rating >= 1600) { color = 'blue'; rankTitle = 'profile.expert'; }
    else if (user.rating >= 1400) { color = 'cyan'; rankTitle = 'profile.specialist'; }
    else if (user.rating >= 1200) { color = 'green'; rankTitle = 'profile.pupil'; }

    // Render sidebar
    sidebar.innerHTML = `
        <div style="border: var(--border-width) solid var(--text-color); background: #fff; padding: 2rem; text-align: center; box-shadow: var(--shadow-offset) var(--shadow-offset) 0 var(--text-color);">
            <div style="width: 150px; height: 150px; background: var(--text-color); margin: 0 auto 1.5rem; border: var(--border-width) solid var(--text-color);"></div>
            <h2 style="font-size: 1.8rem; font-weight: 800; color: ${color};">${user.handle}</h2>
            <p style="font-weight: 700; text-transform: uppercase;">${rankTitle}</p>
            <p style="margin-top: 1rem; font-size: 0.9rem;">Rating: ${user.rating} | Rank: #${user.rank}</p>
            <a href="profile-edit.html" class="btn btn-secondary" style="margin-top: 1.5rem; width: 100%;" data-i18n="dashboard.edit_profile">Edit Profile</a>
        </div>
    `;
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    // Render stats cards
    mainCol.innerHTML = `
        <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 1.5rem; margin-bottom: 3rem;">
            <div style="border: var(--border-width) solid var(--text-color); background: #fff; padding: 1.5rem; text-align: center; box-shadow: 4px 4px 0 var(--text-color);">
                <div style="font-size: 2.5rem; font-weight: 800; color: var(--accent-color);">${user.rating}</div>
                <div style="font-weight: 700; text-transform: uppercase;" data-i18n="dashboard.rating">Rating</div>
            </div>
            <div style="border: var(--border-width) solid var(--text-color); background: #fff; padding: 1.5rem; text-align: center; box-shadow: 4px 4px 0 var(--text-color);">
                <div style="font-size: 2.5rem; font-weight: 800; color: var(--accent-color);">${user.solved}</div>
                <div style="font-weight: 700; text-transform: uppercase;" data-i18n="dashboard.solved">Solved</div>
            </div>
            <div style="border: var(--border-width) solid var(--text-color); background: #fff; padding: 1.5rem; text-align: center; box-shadow: 4px 4px 0 var(--text-color);">
                <div style="font-size: 2.5rem; font-weight: 800; color: var(--accent-color);">${user.contests}</div>
                <div style="font-weight: 700; text-transform: uppercase;" data-i18n="dashboard.contests">Contests</div>
            </div>
        </div>

        <h3 class="section-title"><span class="prompt-symbol">&gt;</span> <span data-i18n="dashboard.recent_submissions">recent submissions</span></h3>
        <div class="table-container">
            <table class="brutalist-table">
                <thead>
                    <tr>
                        <th>#</th>
                        <th>Time</th>
                        <th>Problem</th>
                        <th>Language</th>
                        <th>Verdict</th>
                        <th>Time</th>
                        <th>Memory</th>
                    </tr>
                </thead>
                <tbody id="profile-submissions">
                    <tr><td colspan="7" style="opacity:0.5;" data-i18n="loading.submissions">Loading submissions...</td></tr>
                </tbody>
            </table>
        </div>
    `;
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    // Load submissions
    const submissions = await fetchMockData('submissions.json');
    const subTbody = document.getElementById('profile-submissions');
    if (!subTbody) return;

    if (!submissions || submissions.length === 0) {
        subTbody.innerHTML = '<tr><td colspan="7" style="font-weight:700;" data-i18n="dashboard.no_submissions_yet">No submissions yet.</td></tr>';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    subTbody.innerHTML = '';
    submissions.slice(0, 15).forEach(s => {
        const tr = document.createElement('tr');
        let verdictColor = 'var(--text-color)';
        if (s.verdict.includes('Accepted')) verdictColor = '#228b22';
        else if (s.verdict.includes('Wrong Answer') || s.verdict.includes('Runtime')) verdictColor = 'red';
        else if (s.verdict.includes('Time Limit')) verdictColor = '#d2691e';

        tr.innerHTML = `
            <td>${s.id}</td>
            <td>${s.timestamp}</td>
            <td><a href="problem-detail.html" style="font-weight: 700;">Problem ${s.problem_id}</a></td>
            <td>${s.language}</td>
            <td style="color: ${verdictColor}; font-weight: 800;">${s.verdict}</td>
            <td>${s.time_ms} ms</td>
            <td>${Math.round(s.memory_kb / 1024)} KB</td>
        `;
        subTbody.appendChild(tr);
    });
    if (typeof i18n !== 'undefined') i18n.applyTranslations();
});
