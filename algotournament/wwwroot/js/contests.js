import { fetchMockData } from './data-loader.js';

document.addEventListener('DOMContentLoaded', async () => {
    if (!window.location.pathname.includes('contests.html')) return;

    const contestList = document.querySelector('.contest-list');
    const tbody = document.querySelector('.brutalist-table tbody');

    const contests = await fetchMockData('contests.json');

    if (!contests || contests.length === 0) {
        if (contestList) contestList.innerHTML = '<p style="font-weight:700;" data-i18n="empty.no_upcoming_contests_time">No upcoming contests.</p>';
        if (tbody) tbody.innerHTML = '<tr><td colspan="5" style="font-weight:700;" data-i18n="empty.no_past_contests">No past contests.</td></tr>';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    // Upcoming / Running contests
    const upcoming = contests.filter(c => c.status === 'upcoming' || c.status === 'running');
    if (contestList) {
        if (upcoming.length === 0) {
            contestList.innerHTML = '<p style="font-weight:700;" data-i18n="empty.no_upcoming_contests_time">No upcoming contests at this time.</p>';
        } else {
            contestList.innerHTML = '';
            upcoming.forEach(c => {
                const article = document.createElement('article');
                article.className = 'contest-card';

                const date = new Date(c.start_time);
                const month = date.toLocaleString('default', { month: 'short' }).toUpperCase();
                const day = date.getDate().toString().padStart(2, '0');

                const badgeClass = c.rated ? 'badge-rated' : 'badge-unrated';
                const badgeText = c.rated ? 'contest.rated' : 'contest.unrated';

                article.innerHTML = `
                    <div class="contest-date">
                        <span class="month">${month}</span>
                        <span class="day">${day}</span>
                    </div>
                    <div class="contest-details">
                        <h3>${c.name}</h3>
                        <p>Start: ${c.start_time} | Writers: ${c.writers.join(', ')}</p>
                        <div class="contest-meta">
                            <span class="duration">Duration: ${c.duration}</span>
                            <span class="badge ${badgeClass}">${badgeText}</span>
                        </div>
                    </div>
                    <div class="contest-action">
                        <a href="contest-detail.html" class="link-action" data-i18n="${c.status === 'running' ? 'contest.enter_contest' : 'contest.register_now'}">${c.status === 'running' ? 'Enter &rarr;' : 'Register &rarr;'}</a>
                    </div>
                `;
                contestList.appendChild(article);
            });
        }
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
    }

    // Past contests
    if (tbody) {
        const past = contests.filter(c => c.status === 'past');
        if (past.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="font-weight:700;" data-i18n="empty.no_past_contests">No past contests.</td></tr>';
        } else {
            tbody.innerHTML = '';
            past.forEach(c => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td><a href="contest-detail.html" style="font-weight: 700;">${c.name}</a></td>
                    <td>${c.writers.join(', ')}</td>
                    <td>${c.start_time}</td>
                    <td>${c.duration}</td>
                    <td><a href="ranking-contest.html" style="text-decoration: underline;" data-i18n="contests.standings">Final Standings</a></td>
                `;
                tbody.appendChild(tr);
            });
        }
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
    }
});
