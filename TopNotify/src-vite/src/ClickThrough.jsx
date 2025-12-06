import { Switch } from "@chakra-ui/react";
import { useTranslation } from "react-i18next";

export default function ClickThrough() {
    const { t } = useTranslation();

    return (
        <div className="flexx facenter fillx gap20">
            <label>{t("settings.enableClickThrough")}</label>
            <Switch onChange={(e) => window.ChangeSwitch("EnableClickThrough", e)} isChecked={window.Config.EnableClickThrough} style={{ marginInlineStart: "auto" }} size="lg" />
        </div>
    );
}