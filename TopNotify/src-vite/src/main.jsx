import "./CSS/App.css";
import "./CSS/ChakraOverrides.css";
import "./i18n.js";

import { Suspense, useEffect } from "react";

import About from "./About.jsx";
import App from "./App.jsx";
import { ChakraProvider } from "@chakra-ui/react";
import ReactDOM from "react-dom/client";
import { useTranslation } from "react-i18next";

// Initialize i18next


window.serverURL = "http://" + window.location.host + "/";

//Chakra UI Color Mode
let defaultTheme = (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) ? "dark" : "light";
localStorage.setItem("chakra-ui-color-mode", defaultTheme);
document.documentElement.setAttribute("data-theme", defaultTheme);
document.body.setAttribute("chakra-ui-theme", "chakra-ui-" + defaultTheme);

window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", event => {
    location.reload();
});



ReactDOM.createRoot(document.getElementById("root")).render(RootComponent());

function RootComponent() {
    return (
        <ChakraProvider>
            <Suspense fallback={<div></div>}>
                <Dispatcher/>
            </Suspense>
        </ChakraProvider>
    );
}

function Dispatcher() {
    const { i18n } = useTranslation();
    
    // Handle RTL/LTR direction changes based on current language
    useEffect(() => {
        const dir = i18n.dir(i18n.resolvedLanguage);
        document.documentElement.dir = dir;
        document.documentElement.lang = i18n.resolvedLanguage || "en";
    }, [i18n, i18n.resolvedLanguage]);
    
    let MainMethod = App;

    if (window.location.search.includes("about")) {
        MainMethod = About;
    }
    
    return (
        <MainMethod></MainMethod>
    );
}