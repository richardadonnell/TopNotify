import { Button } from "@chakra-ui/react";
import { TbExternalLink } from "react-icons/tb";
import { useTranslation } from "react-i18next";

export default function TestNotification() {
    const { t } = useTranslation();

    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <span>{t("settings.spawnTestNotification")}</span>
            <Button style={{ marginInlineStart: "auto" }} className="iconButton" onClick={() => igniteView.commandBridge.SpawnTestNotification()}>
                <TbExternalLink />
            </Button>
        </div>
    );
}