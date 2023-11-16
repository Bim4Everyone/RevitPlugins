# coding=utf-8

import clr
clr.AddReference("dosymep.Revit.dll")
clr.AddReference("dosymep.Bim4Everyone.dll")

import dosymep
clr.ImportExtensions(dosymep.Revit)
clr.ImportExtensions(dosymep.Bim4Everyone)

from Autodesk.Revit.DB import *

from pyrevit import forms
from pyrevit import revit
from pyrevit import script
from pyrevit import EXEC_PARAMS

from dosymep_libs.bim4everyone import *


@notification()
@log_plugin(EXEC_PARAMS.command_name)
def script_execute(plugin_logger):
    forms.alert("Привет ревит!")


script_execute()
