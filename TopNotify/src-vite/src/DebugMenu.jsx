import { Button, Divider, Slider, SliderFilledTrack, SliderThumb, SliderTrack, Switch } from "@chakra-ui/react";
import {
    Drawer,
    DrawerBody,
    DrawerCloseButton,
    DrawerContent,
    DrawerFooter,
    DrawerHeader,
    DrawerOverlay
} from "@chakra-ui/react";
import {TbChevronDown, TbExternalLink, TbX} from "react-icons/tb";

import { useState } from "react";
import { useTranslation } from "react-i18next";

export function DebugMenu() {
    const { t } = useTranslation();

    let [isOpen, _setIsOpen] = useState(false);

    let setIsOpen = (v) => {

        if (v && rerender < 0) { return; }

        if (v) {
            setTimeout(() => setRerender(-9999999), 0);
        }
        else {
            setTimeout(() => setRerender(2), 0);
        }

        _setIsOpen(v);
    };

    window.openDebugMenu = () => setIsOpen(true);

    return (
        <>
            <Drawer
                blockScrollOnMount={false}
                isOpen={isOpen}
                placement='bottom'
                onClose={() => setIsOpen(false)}
            >
                <DrawerContent>
                    
                    <div className="windowCloseButton">
                        <Button className="iconButton" onClick={() => setIsOpen(false)}><TbChevronDown/></Button>
                    </div>

                    <DrawerHeader>{t('debug.title')}</DrawerHeader>

                    <DrawerBody>
                        <div className="flexx facenter fillx gap20 buttonContainer">
                            <label>{t('debug.openAppFolder')}</label>
                            <Button style={{ marginInlineStart: "auto" }} className="iconButton" onClick={() => { igniteView.commandBridge.OpenAppFolder(); }}>
                                <TbExternalLink/>
                            </Button>
                        </div>

                        <Divider />

                        <div className="flexx facenter fillx gap20">
                            <label>{t('debug.forceFallbackInterceptor')}</label>
                            <Switch onChange={(e) => ChangeSwitch("EnableDebugForceFallbackMode", e)} isChecked={Config.EnableDebugForceFallbackMode} style={{ marginInlineStart: "auto" }} size='lg' />
                        </div>

                        <Divider />

                        <div className="flexx facenter fillx gap20">
                            <label>{t('debug.disableBoundsCorrection')}</label>
                            <Switch onChange={(e) => ChangeSwitch("EnableDebugRemoveBoundsCorrection", e)} isChecked={Config.EnableDebugRemoveBoundsCorrection} style={{ marginInlineStart: "auto" }} size='lg' />
                        </div>
                    </DrawerBody>

                    <DrawerFooter>
                        
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
        </>
    );
}

addEventListener("keydown", (e) => {
    if (e.key == "F2") {
        window.openDebugMenu();
        e.preventDefault();
    }
});