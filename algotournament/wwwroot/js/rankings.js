import { fetchMockData } from './data-loader.js';

document.addEventListener('DOMContentLoaded', async () => {
    if (!window.location.pathname.includes('ranking.html')) return;

    const tbody = document.querySelector('.brutalist-table tbody');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="4" style="opacity:0.5;" data-i18n="loading.rankings">Loading rankings...</td></tr>';
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    const users = await fetchMockData('users.json');

    if (!users || users.length === 0) {
        tbody.innerHTML = '<tr><td colspan="4" style="font-weight:700;" data-i18n="empty.no_users_ranked">No ranking data available.</td></tr>';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    tbody.innerHTML = '';
    users.slice(0, 50).forEach(u => {
        const tr = document.createElement('tr');

        let color = 'var(--text-color)';
        if (u.rating >= 2400) color = 'red';
        else if (u.rating >= 1900) color = '#d2691e';
        else if (u.rating >= 1600) color = 'blue';
        else if (u.rating >= 1400) color = 'cyan';
        else if (u.rating >= 1200) color = 'green';

        tr.innerHTML = `
            <td>${u.rank}</td>
            <td><a href="profile.html" style="color: ${color}; font-weight: 800;">${u.handle}</a></td>
            <td style="color: ${color}; font-weight: 800;">${u.rating}</td>
            <td>${u.contests}</td>
        `;
        tbody.appendChild(tr);
    });
    if (typeof i18n !== 'undefined') i18n.applyTranslations();
});
