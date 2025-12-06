import "./CSS/About.css";

import {TbBrandGithub, TbGlobe, TbLicense, TbShoppingBag, TbStar, TbStarFilled, TbWorld, TbX} from "react-icons/tb";
import { useEffect, useState } from "react";

import { Button } from "@chakra-ui/react";
import { useTranslation } from "react-i18next";

export default function About() {
    const { t } = useTranslation();
    const [version, setVersion] = useState(" ...");

    useEffect(() => {
        async function fetchVersion() {
            setVersion(await igniteView.commandBridge.GetVersion());
        }
        fetchVersion();
    }, []);

    return (
        <div className={"app loaded about"}>
            <div data-webview-drag className="draggableHeader">
                <h2>{t("about.title")}</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => window.close()}><TbX/></Button>
            </div>

            <img src="/Image/IconSmall.png" alt="TopNotify logo"></img>
            <h4>{t("app.title")}{version}</h4>
            <h6>{t("about.developedBy")}</h6>

            <div className="aboutButtons">
                <Button onClick={() => window.open("https://www.samsidparty.com/software/topnotify")}><TbWorld/> {t("about.officialWebsite")}</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify")}><TbBrandGithub/> {t("about.github")}</Button>
                <Button onClick={() => window.open("ms-windows-store://pdp/?ProductId=9pfmdk0qhkqj")}><TbShoppingBag/> {t("about.microsoftStore")}</Button>
                <Button onClick={() => window.open("ms-windows-store://review/?ProductId=9pfmdk0qhkqj")}><TbStar/> {t("about.leaveReview")}</Button>
                <Button onClick={() => window.open("https://github.com/SamsidParty/TopNotify/blob/main/LICENSE")}><TbLicense/> {t("about.license")}</Button>
            </div>
        </div>
    );
}