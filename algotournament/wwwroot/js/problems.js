import { fetchMockData } from './data-loader.js';

document.addEventListener('DOMContentLoaded', async () => {
    if (!window.location.pathname.includes('problems.html')) return;

    const tbody = document.querySelector('.brutalist-table tbody');
    const searchInput = document.querySelector('.brutalist-input');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="5" style="opacity:0.5;" data-i18n="loading.problems">Loading problems...</td></tr>';
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    const problems = await fetchMockData('problems.json');

    if (!problems || problems.length === 0) {
        tbody.innerHTML = '<tr><td colspan="5" style="font-weight:700;" data-i18n="empty.no_problems_available">No problems available.</td></tr>';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    function renderProblems(data) {
        tbody.innerHTML = '';
        if (data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="5" style="font-weight:700;" data-i18n="empty.no_matching_problems">No matching problems.</td></tr>';
            if (typeof i18n !== 'undefined') i18n.applyTranslations();
            return;
        }
        data.forEach(p => {
            const tr = document.createElement('tr');

            let diffClass = 'diff-easy';
            if (p.difficulty >= 1500 && p.difficulty < 2400) diffClass = 'diff-medium';
            else if (p.difficulty >= 2400) diffClass = 'diff-hard';

            const tagsHtml = p.tags.map(t => `<span class="tag">${t}</span>`).join('');

            tr.innerHTML = `
                <td>${p.id}</td>
                <td><a href="problem-detail.html" style="font-weight: 700;">${p.name}</a></td>
                <td>${tagsHtml}</td>
                <td class="${diffClass}">${p.difficulty}</td>
                <td>x ${p.solved.toLocaleString()}</td>
            `;
            tbody.appendChild(tr);
        });
    }
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    renderProblems(problems.slice(0, 20));

    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase();
            const filtered = problems.filter(p =>
                p.name.toLowerCase().includes(query) ||
                p.tags.some(t => t.toLowerCase().includes(query)) ||
                p.id.toString().includes(query)
            );
            renderProblems(filtered.slice(0, 20));
        });
    }
});
