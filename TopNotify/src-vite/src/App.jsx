import { Button, Container, Divider } from "@chakra-ui/react";
import {TbAlertTriangle, TbCurrencyDollar, TbInfoCircle, TbX} from "react-icons/tb";

import ClickThrough from "./ClickThrough";
import { DebugMenu } from "./DebugMenu";
import LanguageSelector from "./LanguageSelector";
import ManageNotificationSounds from "./NotificationSounds";
import MonitorSelect from "./MonitorSelect";
import NotificationTransparency from "./Transparency";
import Preview from "./Preview";
import ReadAloud from "./ReadAloud";
import SoundInterceptionToggle from "./SoundInterceptionToggle";
import TestNotification from "./TestNotification";
import { useFirstRender } from "./Helper";
import { useState } from "react";
import { useTranslation } from "react-i18next";

window.Config = {
    Location: -1,
    Opacity: 0,
    ReadAloud: false,
    AppReferences: []
};

// Called By C#, Sets The window.Config Object To The Saved Config File
window.SetConfig = async (config) => {
    window.Config = JSON.parse(config);
    window.setRerender(window.rerender + 1);
};

window.UploadConfig = () => {

    if (window.Config.Location == -1) {
        //Config Hasn't Loaded Yet
        return;
    }

    igniteView.commandBridge.WriteConfigFile(JSON.stringify(window.Config));
    window.setRerender(window.rerender + 1);
};

window.ChangeSwitch = function (key, e) {
    window.Config[key] = e.target.checked;
    window.UploadConfig();
    window.setRerender(window.rerender + 1);
};

window.ChangeValue = function (key, e) {
    window.Config[key] = e;
    window.UploadConfig();
    window.setRerender(window.rerender + 1);
};

function App() {
    const { t } = useTranslation();

    let [rerender, setRerender] = useState(0);
    window.rerender = rerender;
    window.setRerender = setRerender;

    if (useFirstRender()) {
        igniteView.commandBridge.invoke("RequestConfig");
    }

    return (
        <div className={"app" + ((rerender > 0) ? " loaded" : "")}>

            <DebugMenu></DebugMenu>

            <div data-webview-drag className="draggableHeader">
                <img src="/Image/IconTiny.png"></img>
                <h2>{t("app.title")}</h2>
            </div>

            <div className="windowCloseButton">
                <Button className="iconButton" onClick={() => { window.close(); }}><TbX /></Button>
            </div>

            <TestNotification></TestNotification>

            <Preview></Preview>

            <MonitorSelect></MonitorSelect>

            {
                window.errorList?.map((error, i) => {
                    return (<ErrorMessage key={i} error={error}></ErrorMessage>);
                })
            }

            <Container>
                <ClickThrough></ClickThrough>
                <Divider />
                <NotificationTransparency></NotificationTransparency>
                <Divider />
                <LanguageSelector></LanguageSelector>
            </Container>

            <Container>
                <ReadAloud></ReadAloud>
                <Divider />
                <SoundInterceptionToggle></SoundInterceptionToggle>
                <Divider />
                <ManageNotificationSounds></ManageNotificationSounds>
            </Container>

            <div className="aboutButtons">
                <Button onClick={() => igniteView.commandBridge.About()}><TbInfoCircle/>{t("app.about")}</Button>
                <Button onClick={() => igniteView.commandBridge.Donate()}><TbCurrencyDollar/>{t("app.donate")}</Button>
            </div>
        </div>
    );
}

function ErrorMessage(props) {
    return (
        <div className="errorMessage"><TbAlertTriangle/>{props.error.Text}</div>
    );
}


export default App;
