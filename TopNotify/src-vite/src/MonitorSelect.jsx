import { Button, Divider, Select, Switch } from "@chakra-ui/react";

import { useTranslation } from "react-i18next";

export default function MonitorSelect(props) {
    const { t } = useTranslation();

    return (
        <Select className="monitorSelect" value={window.Config?.PreferredMonitor} onChange={(e) => { window.ChangeValue("PreferredMonitor", e.target.value); igniteView.commandBridge.RequestConfig(); } }>
            <option value="primary">{t("settings.primaryMonitor")}</option>
            {
                window.Config?.__MonitorData?.map((monitor) => {
                    return (<option key={monitor.ID} value={monitor.ID}>{monitor.DisplayName}</option>);
                })
            }
        </Select>
    );
}