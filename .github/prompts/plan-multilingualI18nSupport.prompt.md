## Plan: Implement Multilingual (i18n) Support with RTL

Add complete internationalization to TopNotify's React frontend and C# backend, including RTL language support and AI-assisted translations for 6 initial languages.

### Steps

1. **Set up `react-i18next` with RTL support** — Install `i18next`, `react-i18next`, `i18next-browser-languagedetector`, and `i18next-http-backend`. Create `src-vite/src/i18n.js` config with RTL detection using `i18n.dir()`, import in `main.jsx`, and add a `useEffect` to set `document.documentElement.dir` dynamically

2. **Create translation JSON files** — Add `public/locales/en/translation.json` with ~28 strings extracted from React components, structured with logical groupings (e.g., `about.*`, `settings.*`, `sounds.*`)

3. **Add RTL-aware CSS** — Update styles to use CSS logical properties where needed; Chakra UI handles most RTL automatically, but verify icon positioning in `Preview.jsx` and `drag.html`

4. **Wrap all frontend strings with `t()` function** — Update 11 component files to use `useTranslation()` hook

5. **Add `PreferredLanguage` to settings** — Extend `Settings.cs` with `public string? PreferredLanguage = null;` (null = auto-detect), update `GetForIPC()` to include detected language

6. **Create language selector component** — Add `LanguageSelector.jsx` dropdown using Chakra UI, persist via `window.ChangeValue()`, call `i18n.changeLanguage()`

7. **Add C# resource files for backend** — Create `Resources/Strings.resx` with ~21 strings from tray menu, errors, and sound metadata

8. **Generate translations with Copilot AI** — Translate into Spanish (`es`), French (`fr`), German (`de`), Japanese (`ja`), Arabic (`ar`), Hebrew (`he`) — covering major user bases + RTL testing

### Approved Decisions

- **Translation method**: Copilot AI, one language at a time
- **Initial languages**: es, fr, de, ja, ar (RTL), he (RTL)
- **Fallback chain**: User preference → Browser → System → English
