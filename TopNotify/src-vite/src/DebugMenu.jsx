import {
    Button,
    Divider,
    Drawer,
    DrawerBody,
    DrawerContent,
    DrawerFooter,
    DrawerHeader,
    Switch
} from "@chakra-ui/react";
import {TbChevronDown, TbExternalLink} from "react-icons/tb";

import { useState } from "react";
import { useTranslation } from "react-i18next";

export function DebugMenu() {
    const { t } = useTranslation();

    const [drawerOpen, setDrawerOpen] = useState(false);

    const setIsOpen = (v) => {

        if (v && window.rerender < 0) { return; }

        if (v) {
            setTimeout(() => window.setRerender(-9999999), 0);
        }
        else {
            setTimeout(() => window.setRerender(2), 0);
        }

        setDrawerOpen(v);
    };

    window.openDebugMenu = () => setIsOpen(true);

    return (
        <Drawer
            blockScrollOnMount={false}
            isOpen={drawerOpen}
            placement='bottom'
            onClose={() => setIsOpen(false)}
        >
            <DrawerContent>

                <div className="windowCloseButton">
                    <Button className="iconButton" onClick={() => setIsOpen(false)}><TbChevronDown/></Button>
                </div>

                <DrawerHeader>{t("debug.title")}</DrawerHeader>

                <DrawerBody>
                    <div className="flexx facenter fillx gap20 buttonContainer">
                        <span>{t("debug.openAppFolder")}</span>
                        <Button style={{ marginInlineStart: "auto" }} className="iconButton" onClick={() => { igniteView.commandBridge.OpenAppFolder(); }}>
                            <TbExternalLink/>
                        </Button>
                    </div>

                    <Divider />

                    <div className="flexx facenter fillx gap20">
                        <span>{t("debug.forceFallbackInterceptor")}</span>
                        <Switch onChange={(e) => window.ChangeSwitch("EnableDebugForceFallbackMode", e)} isChecked={window.Config.EnableDebugForceFallbackMode} style={{ marginInlineStart: "auto" }} size="lg" />
                    </div>

                    <Divider />

                    <div className="flexx facenter fillx gap20">
                        <span>{t("debug.disableBoundsCorrection")}</span>
                        <Switch onChange={(e) => window.ChangeSwitch("EnableDebugRemoveBoundsCorrection", e)} isChecked={window.Config.EnableDebugRemoveBoundsCorrection} style={{ marginInlineStart: "auto" }} size="lg" />
                    </div>
                </DrawerBody>

                <DrawerFooter>

                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    );
}

addEventListener("keydown", (e) => {
    if (e.key === "F2") {
        window.openDebugMenu();
        e.preventDefault();
    }
});