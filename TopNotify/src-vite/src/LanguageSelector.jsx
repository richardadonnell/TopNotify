import { Select } from "@chakra-ui/react";
import { supportedLanguages } from "./i18n.js";
import { useTranslation } from "react-i18next";

/**
 * Language selector dropdown component.
 * Allows users to choose their preferred UI language.
 * Changes are saved to localStorage and synced to C# settings.
 */
export default function LanguageSelector() {
    const { t, i18n } = useTranslation();

    // Handle language change
    const handleLanguageChange = (e) => {
        const newLang = e.target.value;
        
        // Change the language in i18next (also saves to localStorage via detection config)
        i18n.changeLanguage(newLang);
        
        // Also save to C# settings for persistence
        window.ChangeValue("PreferredLanguage", newLang);
    };

    return (
        <div className="flexx facenter fillx gap20">
            <label>{t('settings.language')}</label>
            <Select 
                value={i18n.resolvedLanguage || "en"} 
                onChange={handleLanguageChange}
                style={{ marginInlineStart: "auto", width: "auto", minWidth: "120px" }}
                size="sm"
            >
                {Object.entries(supportedLanguages).map(([code, lang]) => (
                    <option key={code} value={code}>
                        {lang.name}
                    </option>
                ))}
            </Select>
        </div>
    );
}
