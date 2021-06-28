# -*- coding: utf-8 -*-

import clr
clr.AddReference('PlatformSettings.dll')
clr.AddReference('dosymep.Bim4Everyone.dll')

from pyrevit import EXEC_PARAMS
from pyrevit import script
from pyrevit import forms
from pyrevit.loader import sessionmgr
from pyrevit.loader import sessioninfo
from pyrevit.userconfig import user_config
from Autodesk.Revit.ApplicationServices import LanguageType
from dosymep.Bim4Everyone.SharedParams import SharedParamsConfig

import PlatformSettings

def __selfinit__(script_cmp, ui_button_cmp, __rvt__):
    user_config.auto_update = True
    user_config.check_updates = True
    user_config.save_changes();
    
    if __rvt__.Application.Language == LanguageType.Russian:
        user_config.user_locale = 'ru'
        
    if __rvt__.Application.Language == LanguageType.English_USA:
        user_config.user_locale = 'en_us'
        
    SharedParamsConfig.LoadInstance(None)

def OpenPlatrormSettings():
    settings = PlatformSettings.PlatformSettingsCommand()
    result = settings.Execute(EXEC_PARAMS.command_data);

    if result:
        user_config.reload()
        
        logger = script.get_logger()
        results = script.get_results()

        # re-load pyrevit session.
        logger.info('Reloading....')
        sessionmgr.load_session()

        results.newsession = sessioninfo.get_session_uuid()

if __name__ == '__main__':
    OpenPlatrormSettings()
