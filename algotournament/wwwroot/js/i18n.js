// i18n - Internationalization Library
class I18n {
    constructor() {
        this.currentLang = 'en';
        this.translations = {};
        this.fallbackLang = 'en';
    }

    // Auto-detect browser language
    detectBrowserLanguage() {
        const browserLang = navigator.language || navigator.userLanguage;
        // Check if browser language is Vietnamese
        if (browserLang.startsWith('vi')) {
            return 'vi';
        }
        return this.fallbackLang;
    }

    // Get saved language preference from localStorage
    getSavedLanguage() {
        const savedLang = localStorage.getItem('preferredLanguage');
        if (savedLang && (savedLang === 'vi' || savedLang === 'en')) {
            return savedLang;
        }
        return null;
    }

    // Save language preference to localStorage
    saveLanguage(lang) {
        localStorage.setItem('preferredLanguage', lang);
    }

    // Load translations for a specific language
    async loadTranslations(lang) {
        try {
            const response = await fetch(`/locales/${lang}.json`);
            if (!response.ok) {
                throw new Error(`Failed to load ${lang} translations`);
            }
            this.translations[lang] = await response.json();
            return true;
        } catch (error) {
            console.error(`Error loading ${lang} translations:`, error);
            // Fallback to English if current language fails
            if (lang !== this.fallbackLang) {
                return this.loadTranslations(this.fallbackLang);
            }
            return false;
        }
    }

    // Initialize i18n
    async init() {
        try {
            // Check for saved language preference first
            const savedLang = this.getSavedLanguage();
            if (savedLang) {
                this.currentLang = savedLang;
            } else {
                // Auto-detect browser language
                this.currentLang = this.detectBrowserLanguage();
            }

            // Load translations for current language
            await this.loadTranslations(this.currentLang);
            
            // Also load fallback language
            if (this.currentLang !== this.fallbackLang) {
                await this.loadTranslations(this.fallbackLang);
            }

            // Apply translations to DOM
            this.applyTranslations();
            
            // Set HTML lang attribute
            document.documentElement.lang = this.currentLang;
        } finally {
            // Always mark i18n as ready, even if loading fails
            document.documentElement.classList.add('i18n-ready');
        }
    }

    // Get translation for a key
    t(key, fallback = key) {
        const keys = key.split('.');
        let value = this.translations[this.currentLang];
        
        for (const k of keys) {
            if (value && typeof value === 'object' && k in value) {
                value = value[k];
            } else {
                // Try fallback language
                value = this.translations[this.fallbackLang];
                for (const k2 of keys) {
                    if (value && typeof value === 'object' && k2 in value) {
                        value = value[k2];
                    } else {
                        return fallback;
                    }
                }
                break;
            }
        }
        
        return value || fallback;
    }

    // Switch language
    async switchLanguage(lang) {
        if (lang === this.currentLang) return;
        
        // Remove i18n-ready to prevent flickering
        document.documentElement.classList.remove('i18n-ready');
        
        this.currentLang = lang;
        this.saveLanguage(lang);
        
        try {
            // Load translations if not already loaded
            if (!this.translations[lang]) {
                await this.loadTranslations(lang);
            }
            
            // Apply translations to DOM
            this.applyTranslations();
            
            // Set HTML lang attribute
            document.documentElement.lang = this.currentLang;
            
            // Dispatch custom event for other components
            window.dispatchEvent(new CustomEvent('languageChanged', { detail: { lang } }));
        } finally {
            // Always mark i18n as ready, even if loading fails
            document.documentElement.classList.add('i18n-ready');
        }
    }

    // Apply translations to DOM elements with data-i18n attribute
    applyTranslations() {
        const elements = document.querySelectorAll('[data-i18n]');
        elements.forEach(element => {
            const key = element.getAttribute('data-i18n');
            const translation = this.t(key);

            // Check if element has a number to substitute
            const number = element.getAttribute('data-number');
            let finalTranslation = translation;
            if (number !== null) {
                finalTranslation = translation.replace('{0}', number);
            }

            if (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA') {
                element.placeholder = finalTranslation;
            } else {
                element.textContent = finalTranslation;
            }
        });
    }

    // Get current language
    getCurrentLanguage() {
        return this.currentLang;
    }
}

// Create global i18n instance
const i18n = new I18n();

// Initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => i18n.init());
} else {
    i18n.init();
}
