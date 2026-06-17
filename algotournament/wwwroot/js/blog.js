import { fetchMockData } from './data-loader.js';

document.addEventListener('DOMContentLoaded', async () => {
    if (!window.location.pathname.includes('blog.html')) return;

    const blogList = document.querySelector('.blog-list');
    if (!blogList) return;

    blogList.innerHTML = '<p style="opacity:0.5;" data-i18n="loading.posts">Loading posts...</p>';
    if (typeof i18n !== 'undefined') i18n.applyTranslations();

    const blogs = await fetchMockData('blogs.json');

    if (!blogs || blogs.length === 0) {
        blogList.innerHTML = '<p style="font-weight:700;" data-i18n="empty.no_blog_posts">No blog posts available.</p>';
        if (typeof i18n !== 'undefined') i18n.applyTranslations();
        return;
    }

    blogList.innerHTML = '';
    blogs.forEach(b => {
        const article = document.createElement('article');
        article.className = 'blog-post';

        const tagsHtml = b.tags.map(t => `<span class="tag">${t}</span>`).join('');

        article.innerHTML = `
            <div class="blog-meta">${b.date} &bull; <span data-i18n="blog.by">by</span> <a href="profile.html" style="text-decoration:underline;">${b.author}</a></div>
            <h2 class="blog-title"><a href="blog-detail.html">${b.title}</a></h2>
            <div style="margin-bottom: 1rem;">${tagsHtml}</div>
            <p class="blog-excerpt">${b.excerpt}</p>
            <a href="blog-detail.html" class="btn btn-primary" style="padding: 0.5rem 1rem; font-size: 0.9rem;" data-i18n="blog.read_more">Read More &rarr;</a>
        `;
        blogList.appendChild(article);
    });
    if (typeof i18n !== 'undefined') i18n.applyTranslations();
});
