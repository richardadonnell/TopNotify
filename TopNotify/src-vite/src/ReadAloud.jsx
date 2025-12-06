import { Switch } from "@chakra-ui/react";
import { useTranslation } from "react-i18next";

export default function ReadAloud() {
    const { t } = useTranslation();

    return (
        <div className="flexx facenter fillx gap20">
            <label htmlFor="readAloudSwitch">{t('settings.readNotificationsToMe')}</label>
            <Switch id="readAloudSwitch" onChange={(e) => ChangeSwitch("ReadAloud", e)} isChecked={Config.ReadAloud} style={{ marginInlineStart: "auto" }} size='lg' />
        </div>
    );
}