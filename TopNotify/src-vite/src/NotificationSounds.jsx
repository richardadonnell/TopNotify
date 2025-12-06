import "./CSS/NotificationSounds.css";

import { Button, Divider } from "@chakra-ui/react";
import {
    Drawer,
    DrawerBody,
    DrawerContent,
    DrawerFooter,
    DrawerHeader
} from "@chakra-ui/react";
import { Fragment, useState } from "react";
import { TbAlertTriangle, TbChevronDown, TbFolder, TbMusicPlus, TbPencil, TbVolume, TbX } from "react-icons/tb";

import React from "react";
import { useTranslation } from "react-i18next";

export default function ManageNotificationSounds() {
    const { t } = useTranslation();

    const [isOpen, _setIsOpen] = useState(false);
    const [isPickerOpen, _setIsPickerOpen] = useState(false);

    const setIsOpen = (v) => {

        if (v && window.rerender < 0) { return; }

        if (v) {
            setTimeout(() => window.setRerender(-9999999), 0);
        }
        else {
            setTimeout(() => window.setRerender(2), 0);
        }

        _setIsOpen(v);
    };

    const setIsPickerOpen = (v) => {
        _setIsOpen(!v);
        _setIsPickerOpen(v);
    };

    const applySound = (sound) => {

        for (let i = 0; i < window.Config.AppReferences.length; i++) {
            if (window.Config.AppReferences[i].ID === window.soundPickerReferenceID) {
                window.Config.AppReferences[i].SoundPath = sound.Path;
                window.Config.AppReferences[i].SoundDisplayName = sound.Name;
                break;
            }
        }

        window.UploadConfig();
        setIsPickerOpen(false);
    };

    return (
        <div className="flexx facenter fillx gap20 buttonContainer">
            <span data-greyed-out={(!window.isInterceptionEnabled).toString()}>{t("sounds.editNotificationSounds")}</span>
            <Button data-greyed-out={(!window.isInterceptionEnabled).toString()} style={{ marginInlineStart: "auto" }} className="iconButton" onClick={() => setIsOpen(true)}>
                <TbPencil/>
            </Button>
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

                    <DrawerHeader onMouseOver={window.igniteView.dragWindow}>{t("sounds.notificationSounds")}</DrawerHeader>

                    <DrawerBody>
                        <div className="errorMessage medium"><TbAlertTriangle/>{t("sounds.soundWarning")}</div>
                        {
                            window.Config.AppReferences.map((appReference) => {
                                return (
                                    <Fragment key={appReference.ID}>
                                        <Divider/>
                                        <AppReferenceSoundItem setIsPickerOpen={setIsPickerOpen} appReference={appReference}></AppReferenceSoundItem>
                                    </Fragment>
                                );
                            })
                        }
                        <Divider/>
                        <p>{t("sounds.captureHint")}</p>
                    </DrawerBody>

                    <DrawerFooter>
                        
                    </DrawerFooter>
                </DrawerContent>
            </Drawer>
            <SoundPicker applySound={applySound} setIsPickerOpen={setIsPickerOpen} key={window.soundPickerReferenceID + isPickerOpen || "soundPicker"} isOpen={isPickerOpen}></SoundPicker>
        </div>
    );
}

function AppReferenceSoundItem(props) {

    const pickSound = () => {
        window.soundPickerReferenceID = props.appReference.ID;
        props.setIsPickerOpen(true);
    };

    return (
        <div className="appReferenceSoundItem">
            <img src={props.appReference.DisplayIcon || "/Image/DefaultAppReferenceIcon.svg"} alt={props.appReference.DisplayName}></img>
            <h4>{props.appReference.DisplayName}</h4>
            <div className="selectSoundButton">
                <Button onClick={pickSound}>{props.appReference.SoundDisplayName}&nbsp;<TbPencil/></Button>
            </div>
        </div>
    );
}

function SoundPicker(props) {
    const { t } = useTranslation();
    const soundPacks = JSON.parse(igniteView.withReact(React).useCommandResult("FindSounds") || "[]");

    return (
        <Drawer
            blockScrollOnMount={false}
            isOpen={props.isOpen}
            placement='bottom'
            onClose={() => props.setIsPickerOpen(false)}
        >
            <DrawerContent>
                
                <div className="windowCloseButton">
                    <Button className="iconButton" onClick={() => props.setIsPickerOpen(false)}><TbX/></Button>
                </div>

                <DrawerHeader>{t("sounds.selectSound")}</DrawerHeader>

                <DrawerBody>
                    <div className="soundPackList">
                        {
                            soundPacks.map((soundPack) => {
                                return (<SoundPack applySound={props.applySound} soundPack={soundPack} key={soundPack.Name}></SoundPack>);
                            })
                        }
                    </div>
                </DrawerBody>

                <DrawerFooter>
                    
                </DrawerFooter>
            </DrawerContent>
        </Drawer>
    );
}

function SoundPack(props) {
    const { t } = useTranslation();
    const playSound = (sound) => igniteView.commandBridge.PreviewSound(sound.Path);

    return (
        <div className="soundPack">
            <h3>{props.soundPack.Name}</h3>
            <h4>{props.soundPack.Description}</h4>
            <Divider></Divider>
            <div className="soundList">
                {
                    props.soundPack.Sounds.map((sound) => {
                        return (
                            <div className="soundItem" key={sound.Path}>
                                <Button onClick={() => props.applySound(sound)} className="soundItemButton">
                                    <img src={sound.Icon} alt={sound.Name}></img>
                                </Button>
                                <h5>{sound.Name}&nbsp;<Button onClick={() => playSound(sound)} className="iconButton"><TbVolume/></Button></h5>
                            </div>
                        );
                    })
                }
                {
                    props.soundPack.Name === "Your Collection" && (
                        <div className="soundItem" key={"add"}>
                            <Button onClick={async () => {
                                const result = await igniteView.commandBridge.ImportSound();
                                if (result.length === 2) {
                                    props.applySound({ Path: result[0], Name: result[1] });
                                }
                            }} className="soundItemButton">
                                <TbMusicPlus/>
                            </Button>
                            <h5>{t("sounds.import")}&nbsp;<Button onClick={() => igniteView.commandBridge.OpenSoundFolder()} className="iconButton"><TbFolder/></Button></h5>
                        </div>
                    )
                }
            </div>
        </div>
    );
}
