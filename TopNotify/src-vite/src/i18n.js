import HttpApi from "i18next-http-backend";
import LanguageDetector from "i18next-browser-languagedetector";
import i18n from "i18next";
import { initReactI18next } from "react-i18next";

// Supported languages with display names
// RTL languages: Arabic (ar), Hebrew (he)
export const supportedLanguages = {
    en: { name: "English", dir: "ltr" },
    es: { name: "Español", dir: "ltr" },
    fr: { name: "Français", dir: "ltr" },
    de: { name: "Deutsch", dir: "ltr" },
    ja: { name: "日本語", dir: "ltr" },
    ar: { name: "العربية", dir: "rtl" },
    he: { name: "עברית", dir: "rtl" },
};

// Get the list of supported language codes
export const supportedLngs = Object.keys(supportedLanguages);

// Check if a language is RTL
export const isRTL = (lang) => {
    const baseLang = lang?.split("-")[0]; // Handle "en-US" -> "en"
    return supportedLanguages[baseLang]?.dir === "rtl";
};

i18n
    // Load translations via HTTP from /locales/{lng}/translation.json
    .use(HttpApi)
    // Detect user language (localStorage, browser settings, etc.)
    .use(LanguageDetector)
    // Pass i18n instance to react-i18next
    .use(initReactI18next)
    // Initialize i18next
    .init({
        // Fallback language if detection fails or translation missing
        fallbackLng: "en",

        // Supported languages
        supportedLngs: supportedLngs,

        // Don't load region-specific variants (e.g., en-US -> en)
        nonExplicitSupportedLngs: true,

        // Debug mode (disable in production)
        debug: false,

        // Detection order: localStorage first, then browser navigator
        detection: {
            order: ["localStorage", "navigator"],
            caches: ["localStorage"],
            lookupLocalStorage: "topnotify-language",
        },

        // Path to translation files
        backend: {
            loadPath: "/locales/{{lng}}/translation.json",
        },

        // React settings
        interpolation: {
            // React already escapes values to prevent XSS
            escapeValue: false,
        },

        // Wait for translations to load before rendering
        react: {
            useSuspense: true,
        },
    });

export default i18n;
