// Language Switcher Component
class LanguageSwitcher {
    constructor() {
        this.currentLang = 'en';
        this.switcherElement = null;
        this.init();
    }

    init() {
        // Wait for i18n to be ready
        if (typeof i18n === 'undefined') {
            setTimeout(() => this.init(), 100);
            return;
        }

        this.currentLang = i18n.getCurrentLanguage();
        this.createSwitcher();
        this.attachEventListeners();
    }

    createSwitcher() {
        // Check if switcher already exists
        if (document.getElementById('language-switcher')) {
            this.switcherElement = document.getElementById('language-switcher');
            return;
        }

        // Create language switcher element
        const switcherHtml = `
            <div id="language-switcher" class="language-switcher">
                <button class="lang-button" data-lang="vi" title="Tiếng Việt">
                    <span class="lang-flag">🇻🇳</span>
                    <span class="lang-name">VN</span>
                </button>
                <button class="lang-button" data-lang="en" title="English">
                    <span class="lang-flag">🇬🇧</span>
                    <span class="lang-name">EN</span>
                </button>
            </div>
        `;

        // Insert switcher after navbar or in a suitable location
        const navbar = document.querySelector('.navbar') || document.querySelector('nav');
        if (navbar) {
            navbar.insertAdjacentHTML('beforeend', switcherHtml);
        } else {
            // Fallback: insert at the beginning of body
            document.body.insertAdjacentHTML('afterbegin', switcherHtml);
        }

        this.switcherElement = document.getElementById('language-switcher');
        this.updateActiveState();
    }

    attachEventListeners() {
        if (!this.switcherElement) return;

        const buttons = this.switcherElement.querySelectorAll('.lang-button');
        buttons.forEach(button => {
            button.addEventListener('click', (e) => {
                const lang = e.currentTarget.dataset.lang;
                this.switchLanguage(lang);
            });
        });

        // Listen for language changes from other components
        window.addEventListener('languageChanged', (e) => {
            this.currentLang = e.detail.lang;
            this.updateActiveState();
        });
    }

    async switchLanguage(lang) {
        if (lang === this.currentLang) return;

        try {
            await i18n.switchLanguage(lang);
            this.currentLang = lang;
            this.updateActiveState();

            // Update URL if using localized routes
            this.updateURL(lang);

            // Dispatch custom event
            window.dispatchEvent(new CustomEvent('languageChanged', { detail: { lang } }));
        } catch (error) {
            console.error('Error switching language:', error);
        }
    }

    updateActiveState() {
        if (!this.switcherElement) return;

        const buttons = this.switcherElement.querySelectorAll('.lang-button');
        buttons.forEach(button => {
            if (button.dataset.lang === this.currentLang) {
                button.classList.add('active');
            } else {
                button.classList.remove('active');
            }
        });
    }

    updateURL(lang) {
        // Update URL to include language prefix if using localized routes
        // Example: /en/problems/sum-of-two-numbers
        const currentPath = window.location.pathname;
        
        // Check if URL already has language prefix
        const langMatch = currentPath.match(/^\/(vi|en)(\/.*)?$/);
        
        if (langMatch) {
            // URL already has language prefix, update it
            const newPath = currentPath.replace(/^\/(vi|en)/, `/${lang}`);
            window.history.replaceState({}, '', newPath);
        } else {
            // Add language prefix
            const newPath = `/${lang}${currentPath}`;
            window.history.replaceState({}, '', newPath);
        }
    }

    getCurrentLanguage() {
        return this.currentLang;
    }
}

// Create global language switcher instance
const languageSwitcher = new LanguageSwitcher();

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = languageSwitcher;
}
