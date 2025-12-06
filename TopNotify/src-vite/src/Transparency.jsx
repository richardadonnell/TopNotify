import { Button, Divider, Slider, SliderFilledTrack, SliderThumb, SliderTrack, Switch } from "@chakra-ui/react";

import { useTranslation } from "react-i18next";

export default function NotificationTransparency() {
    const { t } = useTranslation();

    return (
        <div className="flexy fillx gap10">
            <label>{t("settings.notificationTransparency")}</label>
            {
                //Slider Is In Uncontrolled Mode For Performance Reasons
                //So We Need To Wait For The Config To Load Before Setting The Default Value
                (window.Config.Location < 0) ?
                    (<></>) :
                    (
                        <Slider size="lg" onChangeEnd={ChangeTransparency} defaultValue={window.Config.Opacity * 20}>
                            <SliderTrack>
                                <SliderFilledTrack />
                            </SliderTrack>
                            <SliderThumb />
                        </Slider>
                    )
            }
        </div>
    );
}

function ChangeTransparency(opacity) {
    window.Config.Opacity = (opacity * 0.05);
    window.UploadConfig();
    window.setRerender(window.rerender + 1);
}