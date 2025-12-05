import { Button, Divider, Slider, SliderFilledTrack, SliderThumb, SliderTrack, Switch } from "@chakra-ui/react";

import { useState } from "react";
import { useTranslation } from "react-i18next";

export default function SoundInterceptionToggle() {
    const { t } = useTranslation();

    let [isInterceptionEnabled, setIsInterceptionEnabled] = useState(false);
    let [checkState, setCheckState] = useState("none");

    if (checkState == "none") {
        setCheckState("loading");
        setTimeout(async () => { 
            window.isInterceptionEnabled = await igniteView.commandBridge.IsSoundInstalledInRegistry();
            setIsInterceptionEnabled(window.isInterceptionEnabled);
            setCheckState("success");
            window.setRerender(rerender + 1); 
        }, 0);
    }

    let setEnabled = async (isChecked) => {
        if (isChecked) {
            await igniteView.commandBridge.InstallSoundInRegistry();
        }
        else {
            await igniteView.commandBridge.UninstallSoundInRegistry();
        }

        UploadConfig();

        setCheckState("none");
    };

    return (
        <div className="flexx facenter fillx gap20">
            <label>{t('settings.enableCustomSounds')}</label>
            <Switch onChange={(e) => setEnabled(e.target.checked)} isChecked={isInterceptionEnabled} style={{ marginInlineStart: "auto" }} size='lg' />
        </div>
    );
}
