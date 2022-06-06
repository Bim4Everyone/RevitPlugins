using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models.Utils {
    internal class UnitTypeUtils {
#if D2020 || R2020
        public static UnitType GetUnitType(BuiltInParameter parameter) {
            switch(parameter) {
                case BuiltInParameter.ANALYTICAL_ABSORPTANCE:
                return UnitType.UT_Number;
                case BuiltInParameter.ANALYTICAL_HEAT_TRANSFER_COEFFICIENT:
                return UnitType.UT_HVAC_CoefficientOfHeatTransfer;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FX:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FY:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FZ:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MX:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MY:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MZ:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FX:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FY:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FZ:
                return UnitType.UT_Force;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MX:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MY:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MZ:
                return UnitType.UT_Moment;
                case BuiltInParameter.ANALYTICAL_MODEL_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.ANALYTICAL_MODEL_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.ANALYTICAL_MODEL_PERIMETER:
                return UnitType.UT_Length;
                case BuiltInParameter.ANALYTICAL_MODEL_ROTATION:
                return UnitType.UT_Angle;
                case BuiltInParameter.ANALYTICAL_SOLAR_HEAT_GAIN_COEFFICIENT:
                return UnitType.UT_Number;
                case BuiltInParameter.ANALYTICAL_THERMAL_MASS:
                return UnitType.UT_HVAC_ThermalMass;
                case BuiltInParameter.ANALYTICAL_THERMAL_RESISTANCE:
                return UnitType.UT_HVAC_ThermalResistance;
                case BuiltInParameter.ANALYTICAL_VISUAL_LIGHT_TRANSMITTANCE:
                return UnitType.UT_Number;
                case BuiltInParameter.BUILDINGPAD_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.CABLETRAY_MINBENDMULTIPLIER_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.CASEWORK_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.CONTINUOUS_FOOTING_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.CONTINUOUS_FOOTING_TOP_HEEL:
                return UnitType.UT_Length;
                case BuiltInParameter.CONTINUOUS_FOOTING_TOP_TOE:
                return UnitType.UT_Length;
                case BuiltInParameter.CONTINUOUS_FOOTING_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.COUPLER_COUPLED_ENGAGEMENT:
                return UnitType.UT_Length;
                case BuiltInParameter.COUPLER_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.COUPLER_MAIN_ENGAGEMENT:
                return UnitType.UT_Length;
                case BuiltInParameter.COUPLER_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.COUPLER_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.CURTAIN_WALL_PANELS_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.CURVE_ELEM_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.DOOR_COST:
                return UnitType.UT_Currency;
                case BuiltInParameter.DPART_AREA_COMPUTED:
                return UnitType.UT_Area;
                case BuiltInParameter.DPART_HEIGHT_COMPUTED:
                return UnitType.UT_Length;
                case BuiltInParameter.DPART_LAYER_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.DPART_LENGTH_COMPUTED:
                return UnitType.UT_Length;
                case BuiltInParameter.DPART_VOLUME_COMPUTED:
                return UnitType.UT_Volume;
                case BuiltInParameter.END_EXTENSION:
                return UnitType.UT_Length;
                case BuiltInParameter.END_JOIN_CUTBACK:
                return UnitType.UT_Length;
                case BuiltInParameter.END_Y_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.END_Z_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.FABRIC_PARAM_CUT_OVERALL_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_PARAM_CUT_OVERALL_WIDTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_PARAM_CUT_SHEET_MASS:
                return UnitType.UT_Mass;
                case BuiltInParameter.FABRIC_PARAM_MAJOR_LAPSPLICE_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_PARAM_MINOR_LAPSPLICE_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_PARAM_TOTAL_SHEET_MASS:
                return UnitType.UT_Mass;
                case BuiltInParameter.FABRIC_SHEET_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_SHEET_MAJOR_END_OVERHANG:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_MAJOR_REINFORCEMENT_AREA:
                return UnitType.UT_Reinforcement_Area_per_Unit_Length;
                case BuiltInParameter.FABRIC_SHEET_MAJOR_SPACING:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_MAJOR_START_OVERHANG:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_MASS:
                return UnitType.UT_Mass;
                case BuiltInParameter.FABRIC_SHEET_MASSUNIT:
                return UnitType.UT_MassPerUnitArea;
                case BuiltInParameter.FABRIC_SHEET_MINOR_END_OVERHANG:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_MINOR_REINFORCEMENT_AREA:
                return UnitType.UT_Reinforcement_Area_per_Unit_Length;
                case BuiltInParameter.FABRIC_SHEET_MINOR_SPACING:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_MINOR_START_OVERHANG:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.FABRIC_SHEET_OVERALL_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_SHEET_OVERALL_WIDTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRIC_SHEET_WIDTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.FABRICATION_BOTTOM_OF_PART:
                return UnitType.UT_Length;
                case BuiltInParameter.FABRICATION_PART_ANGLE:
                return UnitType.UT_Angle;
                case BuiltInParameter.FABRICATION_PART_DEPTH_IN:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_DEPTH_OUT:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_DIAMETER_IN:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_DIAMETER_OUT:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_DOUBLEWALL_MATERIAL_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.FABRICATION_PART_DOUBLEWALL_MATERIAL_THICKNESS:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_INSULATION_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.FABRICATION_PART_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.FABRICATION_PART_LINING_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.FABRICATION_PART_MATERIAL_THICKNESS:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_SHEETMETAL_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.FABRICATION_PART_WEIGHT:
                return UnitType.UT_PipeMass;
                case BuiltInParameter.FABRICATION_PART_WIDTH_IN:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_PART_WIDTH_OUT:
                return UnitType.UT_Pipe_Dimension;
                case BuiltInParameter.FABRICATION_TOP_OF_PART:
                return UnitType.UT_Length;
                case BuiltInParameter.FAMILY_HEIGHT_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.FBX_LIGHT_BALLAST_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_EFFICACY:
                return UnitType.UT_Electrical_Efficacy;
                case BuiltInParameter.FBX_LIGHT_ILLUMINANCE:
                return UnitType.UT_Electrical_Illuminance;
                case BuiltInParameter.FBX_LIGHT_INITIAL_COLOR_TEMPERATURE:
                return UnitType.UT_Color_Temperature;
                case BuiltInParameter.FBX_LIGHT_LAMP_LUMEN_DEPR:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_LAMP_TILT_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_LIMUNOUS_FLUX:
                return UnitType.UT_Electrical_Luminous_Flux;
                case BuiltInParameter.FBX_LIGHT_LIMUNOUS_INTENSITY:
                return UnitType.UT_Electrical_Luminous_Intensity;
                case BuiltInParameter.FBX_LIGHT_LUMENAIRE_DIRT:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_SURFACE_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_TEMPERATURE_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_TOTAL_LIGHT_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_VOLTAGE_LOSS:
                return UnitType.UT_Number;
                case BuiltInParameter.FBX_LIGHT_WATTAGE:
                return UnitType.UT_Electrical_Wattage;
                case BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.FURNITURE_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.HOST_AREA_COMPUTED:
                return UnitType.UT_Area;
                case BuiltInParameter.HOST_PERIMETER_COMPUTED:
                return UnitType.UT_Length;
                case BuiltInParameter.HOST_VOLUME_COMPUTED:
                return UnitType.UT_Volume;
                case BuiltInParameter.INSTANCE_ELEVATION_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.INSTANCE_LENGTH_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.JOIST_SYSTEM_SPACING_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.LEVEL_DATA_FLOOR_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.LEVEL_DATA_FLOOR_PERIMETER:
                return UnitType.UT_Length;
                case BuiltInParameter.LEVEL_DATA_SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.LEVEL_DATA_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.LEVEL_ELEV:
                return UnitType.UT_Length;
                case BuiltInParameter.LOAD_AREA_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.LOAD_AREA_FORCE_FX1:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FX2:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FX3:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FY1:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FY2:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FY3:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FZ1:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FZ2:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_AREA_FORCE_FZ3:
                return UnitType.UT_AreaForce;
                case BuiltInParameter.LOAD_FORCE_FX:
                return UnitType.UT_Force;
                case BuiltInParameter.LOAD_FORCE_FY:
                return UnitType.UT_Force;
                case BuiltInParameter.LOAD_FORCE_FZ:
                return UnitType.UT_Force;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FX1:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FX2:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FY1:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FY2:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FZ1:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_FORCE_FZ2:
                return UnitType.UT_LinearForce;
                case BuiltInParameter.LOAD_LINEAR_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.LOAD_MOMENT_MX:
                return UnitType.UT_Moment;
                case BuiltInParameter.LOAD_MOMENT_MX1:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.LOAD_MOMENT_MX2:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.LOAD_MOMENT_MY:
                return UnitType.UT_Moment;
                case BuiltInParameter.LOAD_MOMENT_MY1:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.LOAD_MOMENT_MY2:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.LOAD_MOMENT_MZ:
                return UnitType.UT_Moment;
                case BuiltInParameter.LOAD_MOMENT_MZ1:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.LOAD_MOMENT_MZ2:
                return UnitType.UT_LinearMoment;
                case BuiltInParameter.MASS_DATA_MASS_EXTERIOR_WALL_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_MASS_INTERIOR_WALL_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_MASS_OPENING_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_MASS_ROOF_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_MASS_SKYLIGHT_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_MASS_WINDOW_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_DATA_PERCENTAGE_GLAZING:
                return UnitType.UT_Number;
                case BuiltInParameter.MASS_DATA_PERCENTAGE_SKYLIGHTS:
                return UnitType.UT_Number;
                case BuiltInParameter.MASS_DATA_SHADE_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.MASS_DATA_SILL_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.MASS_DATA_SKYLIGHT_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.MASS_GROSS_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_GROSS_SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_GROSS_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.MASS_ZONE_FLOOR_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.MASS_ZONE_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.MEP_EQUIPMENT_CALC_PIPINGFLOW_PARAM:
                return UnitType.UT_Piping_Flow;
                case BuiltInParameter.MEP_EQUIPMENT_CALC_PIPINGPRESSUREDROP_PARAM:
                return UnitType.UT_Piping_Pressure;
                case BuiltInParameter.PATH_OF_TRAVEL_SPEED:
                return UnitType.UT_Speed;
                case BuiltInParameter.PATH_OF_TRAVEL_TIME:
                return UnitType.UT_TimeInterval;
                case BuiltInParameter.PATH_REIN_LENGTH_1:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.PATH_REIN_LENGTH_2:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.PATH_REIN_SPACING:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.PEAK_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.PEAK_COOLING_LOAD_PARAM:
                return UnitType.UT_HVAC_Cooling_Load;
                case BuiltInParameter.PEAK_HEATING_LOAD_PARAM:
                return UnitType.UT_HVAC_Heating_Load;
                case BuiltInParameter.PHY_MATERIAL_PARAM_BENDING:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_BENDING_REINFORCEMENT:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_COMPRESSION_PARALLEL:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_COMPRESSION_PERPENDICULAR:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_CONCRETE_COMPRESSION:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF:
                return UnitType.UT_ThermalExpansion;
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF1:
                return UnitType.UT_ThermalExpansion;
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF2:
                return UnitType.UT_ThermalExpansion;
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF3:
                return UnitType.UT_ThermalExpansion;
                case BuiltInParameter.PHY_MATERIAL_PARAM_MINIMUM_TENSILE_STRENGTH:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_MINIMUM_YIELD_STRESS:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD1:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD2:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD3:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_REDUCTION_FACTOR:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_RESISTANCE_CALC_STRENGTH:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD1:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD2:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD3:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_PARALLEL:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_PERPENDICULAR:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_REINFORCEMENT:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_STRENGTH_REDUCTION:
                return UnitType.UT_Number;
                case BuiltInParameter.PHY_MATERIAL_PARAM_STRUCTURAL_DENSITY:
                return UnitType.UT_MassDensity;
                case BuiltInParameter.PHY_MATERIAL_PARAM_UNIT_WEIGHT:
                return UnitType.UT_UnitWeight;
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD1:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD2:
                return UnitType.UT_Stress;
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD3:
                return UnitType.UT_Stress;
                case BuiltInParameter.PROJECTED_SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.PROPERTY_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.PROPERTY_SEGMENT_BEARING:
                return UnitType.UT_SiteAngle;
                case BuiltInParameter.PROPERTY_SEGMENT_DISTANCE:
                return UnitType.UT_Length;
                case BuiltInParameter.PROPERTY_SEGMENT_RADIUS:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_ADDITIONAL_FLOW:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.RBS_CABLETRAY_BENDRADIUS:
                return UnitType.UT_Electrical_CableTraySize;
                case BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM:
                return UnitType.UT_Electrical_CableTraySize;
                case BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM:
                return UnitType.UT_Electrical_CableTraySize;
                case BuiltInParameter.RBS_CONDUIT_BENDRADIUS:
                return UnitType.UT_Electrical_ConduitSize;
                case BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM:
                return UnitType.UT_Electrical_ConduitSize;
                case BuiltInParameter.RBS_CONDUIT_INNER_DIAM_PARAM:
                return UnitType.UT_Electrical_ConduitSize;
                case BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM:
                return UnitType.UT_Electrical_ConduitSize;
                case BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_CTC_TOP_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_CURVE_DIAMETER_PARAM:
                return UnitType.UT_HVAC_DuctSize;
                case BuiltInParameter.RBS_CURVE_HEIGHT_PARAM:
                return UnitType.UT_HVAC_DuctSize;
                case BuiltInParameter.RBS_CURVE_SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.RBS_CURVE_WIDTH_PARAM:
                return UnitType.UT_HVAC_DuctSize;
                case BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_DUCT_FLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.RBS_DUCT_STATIC_PRESSURE:
                return UnitType.UT_HVAC_Pressure;
                case BuiltInParameter.RBS_DUCT_TOP_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEA:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEB:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEC:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_MAINS:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEA:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEB:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEC:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEA:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEB:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEC:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEA_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEB_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEC_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEA:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEB:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEC:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEA:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEB:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEC:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_MCB_RATING_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_NEUTRAL_RATING_PARAM:
                return UnitType.UT_Electrical_Demand_Factor;
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_CONNECTED_CURRENT_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_CURRENT_PARAM:
                return UnitType.UT_Electrical_Current;
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_FACTOR_PARAM:
                return UnitType.UT_Electrical_Demand_Factor;
                case BuiltInParameter.RBS_ELEC_PANEL_TOTALESTLOAD_PARAM:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM:
                return UnitType.UT_Electrical_Apparent_Power;
                case BuiltInParameter.RBS_ELEC_ROOM_AVERAGE_ILLUMINATION:
                return UnitType.UT_Electrical_Illuminance;
                case BuiltInParameter.RBS_ELEC_ROOM_CAVITY_RATIO:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_ELEC_ROOM_LIGHTING_CALC_LUMINAIREPLANE:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_ELEC_ROOM_LIGHTING_CALC_WORKPLANE:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_CEILING:
                return UnitType.UT_HVAC_Factor;
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_FLOOR:
                return UnitType.UT_HVAC_Factor;
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_WALLS:
                return UnitType.UT_HVAC_Factor;
                case BuiltInParameter.RBS_EQ_DIAMETER_PARAM:
                return UnitType.UT_HVAC_DuctSize;
                case BuiltInParameter.RBS_FP_SPRINKLER_K_FACTOR_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_FP_SPRINKLER_ORIFICE_SIZE_PARAM:
                return UnitType.UT_PipeSize;
                case BuiltInParameter.RBS_FP_SPRINKLER_TEMPERATURE_RATING_PARAM:
                return UnitType.UT_Piping_Temperature;
                case BuiltInParameter.RBS_FRICTION:
                return UnitType.UT_HVAC_Friction;
                case BuiltInParameter.RBS_GBXML_SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM:
                return UnitType.UT_HVAC_DuctSize;
                case BuiltInParameter.RBS_INSULATION_LINING_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_DUCT:
                return UnitType.UT_HVAC_DuctInsulationThickness;
                case BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_PIPE:
                return UnitType.UT_PipeInsulationThickness;
                case BuiltInParameter.RBS_LINING_THICKNESS_FOR_DUCT:
                return UnitType.UT_HVAC_DuctLiningThickness;
                case BuiltInParameter.RBS_LOSS_COEFFICIENT:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_MAX_FLOW:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.RBS_MIN_FLOW:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.RBS_PIPE_ADDITIONAL_FLOW_PARAM:
                return UnitType.UT_Piping_Flow;
                case BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_PIPE_CWFU_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_DIAMETER_PARAM:
                return UnitType.UT_PipeSize;
                case BuiltInParameter.RBS_PIPE_FIXTURE_UNITS_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_FLOW_PARAM:
                return UnitType.UT_Piping_Flow;
                case BuiltInParameter.RBS_PIPE_FLUID_DENSITY_PARAM:
                return UnitType.UT_Piping_Density;
                case BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM:
                return UnitType.UT_Piping_Temperature;
                case BuiltInParameter.RBS_PIPE_FLUID_VISCOSITY_PARAM:
                return UnitType.UT_Piping_Viscosity;
                case BuiltInParameter.RBS_PIPE_FRICTION_FACTOR_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_FRICTION_PARAM:
                return UnitType.UT_Piping_Friction;
                case BuiltInParameter.RBS_PIPE_HWFU_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM:
                return UnitType.UT_PipeSize;
                case BuiltInParameter.RBS_PIPE_INVERT_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_PIPE_OUTER_DIAMETER:
                return UnitType.UT_PipeSize;
                case BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM:
                return UnitType.UT_Piping_Pressure;
                case BuiltInParameter.RBS_PIPE_RELATIVE_ROUGHNESS_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_REYNOLDS_NUMBER_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_ROUGHNESS_PARAM:
                return UnitType.UT_Piping_Roughness;
                case BuiltInParameter.RBS_PIPE_STATIC_PRESSURE:
                return UnitType.UT_Piping_Pressure;
                case BuiltInParameter.RBS_PIPE_SYSTEM_FIXTURE_UNIT_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PIPE_TOP_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.RBS_PIPE_VELOCITY_PARAM:
                return UnitType.UT_Piping_Velocity;
                case BuiltInParameter.RBS_PIPE_VOLUME_PARAM:
                return UnitType.UT_Piping_Volume;
                case BuiltInParameter.RBS_PIPE_WFU_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_PRESSURE_DROP:
                return UnitType.UT_HVAC_Pressure;
                case BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS:
                return UnitType.UT_HVAC_DuctInsulationThickness;
                case BuiltInParameter.RBS_REFERENCE_LINING_THICKNESS:
                return UnitType.UT_HVAC_DuctLiningThickness;
                case BuiltInParameter.RBS_REYNOLDSNUMBER_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_ROOM_COEFFICIENT_UTILIZATION:
                return UnitType.UT_Number;
                case BuiltInParameter.RBS_VELOCITY:
                return UnitType.UT_HVAC_Velocity;
                case BuiltInParameter.RBS_VELOCITY_PRESSURE:
                return UnitType.UT_HVAC_Pressure;
                case BuiltInParameter.REBAR_BAR_DIAMETER:
                return UnitType.UT_Bar_Diameter;
                case BuiltInParameter.REBAR_ELEM_BAR_SPACING:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.REBAR_ELEM_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.REBAR_ELEM_TOTAL_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER:
                return UnitType.UT_Bar_Diameter;
                case BuiltInParameter.REBAR_MAX_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.REBAR_MIN_LENGTH:
                return UnitType.UT_Reinforcement_Length;
                case BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_1_GENERIC:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_2_GENERIC:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_1_GENERIC:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_2_GENERIC:
                return UnitType.UT_Reinforcement_Spacing;
                case BuiltInParameter.REIN_EST_BAR_VOLUME:
                return UnitType.UT_Reinforcement_Volume;
                case BuiltInParameter.REINFORCEMENT_VOLUME:
                return UnitType.UT_Reinforcement_Volume;
                case BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOF_SLOPE:
                return UnitType.UT_Slope;
                case BuiltInParameter.ROOM_ACTUAL_EXHAUST_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_ACTUAL_LIGHTING_LOAD_PARAM:
                return UnitType.UT_Electrical_Power;
                case BuiltInParameter.ROOM_ACTUAL_LIGHTING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_ACTUAL_POWER_LOAD_PARAM:
                return UnitType.UT_Electrical_Power;
                case BuiltInParameter.ROOM_ACTUAL_POWER_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_ACTUAL_RETURN_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_AIR_CHANGES_PER_HOUR_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.ROOM_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.ROOM_AREA_PER_PERSON_PARAM:
                return UnitType.UT_Area;
                case BuiltInParameter.ROOM_CALCULATED_COOLING_LOAD_PARAM:
                return UnitType.UT_HVAC_Cooling_Load;
                case BuiltInParameter.ROOM_CALCULATED_COOLING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Cooling_Load_Divided_By_Area;
                case BuiltInParameter.ROOM_CALCULATED_HEATING_LOAD_PARAM:
                return UnitType.UT_HVAC_Heating_Load;
                case BuiltInParameter.ROOM_CALCULATED_HEATING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Heating_Load_Divided_By_Area;
                case BuiltInParameter.ROOM_CALCULATED_SUPPLY_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_CALCULATED_SUPPLY_AIRFLOW_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.ROOM_DESIGN_COOLING_LOAD_PARAM:
                return UnitType.UT_HVAC_Cooling_Load;
                case BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM:
                return UnitType.UT_HVAC_Heating_Load;
                case BuiltInParameter.ROOM_DESIGN_LIGHTING_LOAD_PARAM:
                return UnitType.UT_Electrical_Power;
                case BuiltInParameter.ROOM_DESIGN_LIGHTING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_DESIGN_MECHANICAL_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_DESIGN_OTHER_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_DESIGN_POWER_LOAD_PARAM:
                return UnitType.UT_Electrical_Power;
                case BuiltInParameter.ROOM_DESIGN_POWER_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.ROOM_DESIGN_RETURN_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOM_LOWER_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOM_NUMBER_OF_PEOPLE_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.ROOM_OUTDOOR_AIR_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.ROOM_OUTDOOR_AIR_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_OUTDOOR_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ROOM_PEOPLE_LATENT_HEAT_GAIN_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_HeatGain;
                case BuiltInParameter.ROOM_PEOPLE_SENSIBLE_HEAT_GAIN_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_HeatGain;
                case BuiltInParameter.ROOM_PEOPLE_TOTAL_HEAT_GAIN_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_HeatGain;
                case BuiltInParameter.ROOM_PERIMETER:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOM_PLENUM_LIGHTING_PARAM:
                return UnitType.UT_HVAC_Factor;
                case BuiltInParameter.ROOM_UPPER_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.ROOM_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.SPACE_AIR_CHANGES_PER_HOUR:
                return UnitType.UT_Number;
                case BuiltInParameter.SPACE_AIRFLOW_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.SPACE_AREA_PER_PERSON_PARAM:
                return UnitType.UT_Area;
                case BuiltInParameter.SPACE_INFILTRATION_AIRFLOW_PER_AREA:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.SPACE_LIGHTING_LOAD_PARAM:
                return UnitType.UT_HVAC_Power;
                case BuiltInParameter.SPACE_LIGHTING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW_PER_AREA:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW_PER_PERSON:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.SPACE_PEOPLE_LATENT_HEAT_GAIN_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_HeatGain;
                case BuiltInParameter.SPACE_PEOPLE_LOAD_PARAM:
                return UnitType.UT_HVAC_Power;
                case BuiltInParameter.SPACE_PEOPLE_SENSIBLE_HEAT_GAIN_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_HeatGain;
                case BuiltInParameter.SPACE_POWER_LOAD_PARAM:
                return UnitType.UT_HVAC_Power;
                case BuiltInParameter.SPACE_POWER_LOAD_PER_AREA_PARAM:
                return UnitType.UT_Electrical_Power_Density;
                case BuiltInParameter.STAIRS_ACTUAL_RISER_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_ACTUAL_TREAD_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_ATTR_MAX_RISER_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_ATTR_MINIMUM_TREAD_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_ATTR_TREAD_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_LANDINGTYPE_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RAILING_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RAILING_HEIGHT_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RAILING_PLACEMENT_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RUN_ACTUAL_RISER_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RUN_ACTUAL_RUN_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RUN_ACTUAL_TREAD_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RUN_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_RUNTYPE_STRUCTURAL_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_SUPPORTTYPE_STRUCTURAL_DEPTH_ON_LANDING:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_SUPPORTTYPE_STRUCTURAL_DEPTH_ON_RUN:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_SUPPORTTYPE_TOTAL_DEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STAIRS_SUPPORTTYPE_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.START_EXTENSION:
                return UnitType.UT_Length;
                case BuiltInParameter.START_JOIN_CUTBACK:
                return UnitType.UT_Length;
                case BuiltInParameter.START_Y_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.START_Z_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_ANCHOR_TOTAL_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_BOLT_GRIP_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_BOLT_GRIP_LENGTH_INCREASE:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_BOLT_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_BOLT_TOTAL_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_CUT_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_EXACT_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_PAINT_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.STEEL_ELEM_PATTERN_EDGE_DISTANCE_X:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_EDGE_DISTANCE_Y:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_INTERMEDIATE_DISTANCE_X:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_INTERMEDIATE_DISTANCE_Y:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_RADIUS:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_TOTAL_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PATTERN_TOTAL_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PLATE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.STEEL_ELEM_PLATE_EXACT_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_PLATE_JUSTIFICATION:
                return UnitType.UT_Number;
                case BuiltInParameter.STEEL_ELEM_PLATE_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PLATE_PAINT_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.STEEL_ELEM_PLATE_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PLATE_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.STEEL_ELEM_PLATE_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_PLATE_WIDTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PROFILE_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_PROFILE_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.STEEL_ELEM_SHEARSTUD_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_SHEARSTUD_TOTAL_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_WEIGHT:
                return UnitType.UT_Mass;
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_EFFECTIVETHROAT:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_PREPDEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_ROOTOPENING:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_EFFECTIVETHROAT:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_PREPDEPTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_ROOTOPENING:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STEEL_ELEM_WELD_PITCH:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE:
                return UnitType.UT_Angle;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM_CORE:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM_SURVEY:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP_CORE:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP_SURVEY:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_FOUNDATION_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_FRAME_CUT_LENGTH:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_REFERENCE_LEVEL_ELEVATION:
                return UnitType.UT_Length;
                case BuiltInParameter.STRUCTURAL_SECTION_AREA:
                return UnitType.UT_Section_Area;
                case BuiltInParameter.STRUCTURAL_SECTION_BOTTOM_CUT_HEIGHT:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_BOTTOM_CUT_WIDTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_CANTILEVER_HEIGHT:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_CANTILEVER_LENGTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ALPHA:
                return UnitType.UT_Angle;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_DIAMETER:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ELASTIC_MODULUS_STRONG_AXIS:
                return UnitType.UT_Section_Modulus;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ELASTIC_MODULUS_WEAK_AXIS:
                return UnitType.UT_Section_Modulus;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_MOMENT_OF_INERTIA_STRONG_AXIS:
                return UnitType.UT_Moment_of_Inertia;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_MOMENT_OF_INERTIA_WEAK_AXIS:
                return UnitType.UT_Moment_of_Inertia;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_NOMINAL_WEIGHT:
                return UnitType.UT_Weight_per_Unit_Length;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PERIMETER:
                return UnitType.UT_Surface_Area;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PLASTIC_MODULUS_STRONG_AXIS:
                return UnitType.UT_Section_Modulus;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PLASTIC_MODULUS_WEAK_AXIS:
                return UnitType.UT_Section_Modulus;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_SHEAR_AREA_STRONG_AXIS:
                return UnitType.UT_Section_Area;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_SHEAR_AREA_WEAK_AXIS:
                return UnitType.UT_Section_Area;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_TORSIONAL_MODULUS:
                return UnitType.UT_Section_Modulus;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_TORSIONAL_MOMENT_OF_INERTIA:
                return UnitType.UT_Moment_of_Inertia;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_WARPING_CONSTANT:
                return UnitType.UT_Warping_Constant;
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_CPROFILE_FOLD_LENGTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_HSS_INNERFILLET:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_HSS_OUTERFILLET:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_DIAMETER:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_BETWEEN_ROWS:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_TWO_ROWS:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_WEB:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_CLEAR_WEB_HEIGHT:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGE_TOE_OF_FILLET:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS_LOCATION:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEB_TOE_OF_FILLET:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBHEIGHT:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS_LOCATION:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGETHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGEWIDTH:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGETHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGEWIDTH:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_DIAMETER_LONGER_FLANGE:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_DIAMETER_SHORTER_FLANGE:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_1_LONGER_FLANGE:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_2_LONGER_FLANGE:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_SHORTER_FLANGE:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_LPROFILE_LIP_LENGTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLDESIGNTHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLNOMINALTHICKNESS:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_BEND_WIDTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_MIDDLE_BEND_WIDTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_TOP_BEND_WIDTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_SLOPED_FLANGE_ANGLE:
                return UnitType.UT_Angle;
                case BuiltInParameter.STRUCTURAL_SECTION_SLOPED_WEB_ANGLE:
                return UnitType.UT_Angle;
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_CUT_HEIGHT:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_CUT_WIDTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_WEB_FILLET:
                return UnitType.UT_Section_Property;
                case BuiltInParameter.STRUCTURAL_SECTION_ZPROFILE_BOTTOM_FLANGE_LENGTH:
                return UnitType.UT_Section_Dimension;
                case BuiltInParameter.SUPPORT_HAND_CLEARANCE:
                return UnitType.UT_Length;
                case BuiltInParameter.SUPPORT_HEIGHT:
                return UnitType.UT_Length;
                case BuiltInParameter.SURFACE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.VOLUME_CUT:
                return UnitType.UT_Volume;
                case BuiltInParameter.VOLUME_FILL:
                return UnitType.UT_Volume;
                case BuiltInParameter.VOLUME_NET:
                return UnitType.UT_Volume;
                case BuiltInParameter.WALL_ATTR_WIDTH_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.WALL_BASE_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.WALL_TOP_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.WALL_USER_HEIGHT_PARAM:
                return UnitType.UT_Length;
                case BuiltInParameter.WINDOW_THICKNESS:
                return UnitType.UT_Length;
                case BuiltInParameter.Y_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.Z_OFFSET_VALUE:
                return UnitType.UT_Length;
                case BuiltInParameter.ZONE_AREA:
                return UnitType.UT_Area;
                case BuiltInParameter.ZONE_AREA_GROSS:
                return UnitType.UT_Area;
                case BuiltInParameter.ZONE_CALCULATED_AREA_PER_COOLING_LOAD_PARAM:
                return UnitType.UT_HVAC_Area_Divided_By_Cooling_Load;
                case BuiltInParameter.ZONE_CALCULATED_AREA_PER_HEATING_LOAD_PARAM:
                return UnitType.UT_HVAC_Area_Divided_By_Heating_Load;
                case BuiltInParameter.ZONE_CALCULATED_COOLING_LOAD_PARAM:
                return UnitType.UT_HVAC_Cooling_Load;
                case BuiltInParameter.ZONE_CALCULATED_COOLING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Cooling_Load_Divided_By_Area;
                case BuiltInParameter.ZONE_CALCULATED_HEATING_LOAD_PARAM:
                return UnitType.UT_HVAC_Heating_Load;
                case BuiltInParameter.ZONE_CALCULATED_HEATING_LOAD_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Heating_Load_Divided_By_Area;
                case BuiltInParameter.ZONE_CALCULATED_SUPPLY_AIRFLOW_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ZONE_CALCULATED_SUPPLY_AIRFLOW_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.ZONE_COIL_BYPASS_PERCENTAGE_PARAM:
                return UnitType.UT_HVAC_Factor;
                case BuiltInParameter.ZONE_COOLING_AIR_TEMPERATURE_PARAM:
                return UnitType.UT_HVAC_Temperature;
                case BuiltInParameter.ZONE_COOLING_SET_POINT_PARAM:
                return UnitType.UT_HVAC_Temperature;
                case BuiltInParameter.ZONE_DEHUMIDIFICATION_SET_POINT_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.ZONE_HEATING_AIR_TEMPERATURE_PARAM:
                return UnitType.UT_HVAC_Temperature;
                case BuiltInParameter.ZONE_HEATING_SET_POINT_PARAM:
                return UnitType.UT_HVAC_Temperature;
                case BuiltInParameter.ZONE_HUMIDIFICATION_SET_POINT_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.ZONE_LEVEL_OFFSET:
                return UnitType.UT_Length;
                case BuiltInParameter.ZONE_OA_RATE_PER_ACH_PARAM:
                return UnitType.UT_Number;
                case BuiltInParameter.ZONE_OUTSIDE_AIR_PER_AREA_PARAM:
                return UnitType.UT_HVAC_Airflow_Density;
                case BuiltInParameter.ZONE_OUTSIDE_AIR_PER_PERSON_PARAM:
                return UnitType.UT_HVAC_Airflow;
                case BuiltInParameter.ZONE_PERIMETER:
                return UnitType.UT_Length;
                case BuiltInParameter.ZONE_VOLUME:
                return UnitType.UT_Volume;
                case BuiltInParameter.ZONE_VOLUME_GROSS:
                return UnitType.UT_Volume;
                default:
                return UnitType.UT_Undefined;
            }
        }
#else
        public static ForgeTypeId GetUnitType(BuiltInParameter parameter) {
            switch(parameter) {
                case BuiltInParameter.ALL_MODEL_COST:
                return new ForgeTypeId("autodesk.spec.measurable:currency-2.0.0");
                case BuiltInParameter.ANALYTICAL_ABSORPTANCE:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ANALYTICAL_HEAT_TRANSFER_COEFFICIENT:
                return new ForgeTypeId("autodesk.spec.aec.energy:heatTransferCoefficient-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FX:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FY:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_FZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MX:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MY:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_END_MZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FX:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FY:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_FZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MX:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MY:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MEMBER_FORCE_START_MZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.ANALYTICAL_MODEL_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.ANALYTICAL_MODEL_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ANALYTICAL_MODEL_PERIMETER:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ANALYTICAL_MODEL_ROTATION:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.ANALYTICAL_SOLAR_HEAT_GAIN_COEFFICIENT:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ANALYTICAL_THERMAL_MASS:
                return new ForgeTypeId("autodesk.spec.aec.energy:heatCapacityPerArea-2.0.0");
                case BuiltInParameter.ANALYTICAL_THERMAL_RESISTANCE:
                return new ForgeTypeId("autodesk.spec.aec.energy:thermalResistance-2.0.0");
                case BuiltInParameter.ANALYTICAL_VISUAL_LIGHT_TRANSMITTANCE:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.BUILDINGPAD_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CABLETRAY_MINBENDMULTIPLIER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.CEILING_HEIGHTABOVELEVEL_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CONTINUOUS_FOOTING_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CONTINUOUS_FOOTING_TOP_HEEL:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CONTINUOUS_FOOTING_TOP_TOE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CONTINUOUS_FOOTING_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.COUPLER_COUPLED_ENGAGEMENT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.COUPLER_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.COUPLER_MAIN_ENGAGEMENT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.COUPLER_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.COUPLER_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CURTAIN_WALL_PANELS_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.CURVE_ELEM_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.DPART_AREA_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.DPART_HEIGHT_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.DPART_LAYER_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.DPART_LENGTH_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.DPART_VOLUME_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.END_EXTENSION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.END_JOIN_CUTBACK:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.END_Y_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.END_Z_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_CUT_OVERALL_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_CUT_OVERALL_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_CUT_SHEET_MASS:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_MAJOR_LAPSPLICE_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_MINOR_LAPSPLICE_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_PARAM_TOTAL_SHEET_MASS:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MAJOR_END_OVERHANG:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MAJOR_REINFORCEMENT_AREA:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementAreaPerUnitLength-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MAJOR_SPACING:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MAJOR_START_OVERHANG:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MASS:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MASSUNIT:
                return new ForgeTypeId("autodesk.spec.aec.structural:massPerUnitArea-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MINOR_END_OVERHANG:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MINOR_REINFORCEMENT_AREA:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementAreaPerUnitLength-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MINOR_SPACING:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_MINOR_START_OVERHANG:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_OVERALL_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_OVERALL_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRIC_SHEET_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.FABRICATION_BOTTOM_OF_PART:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FABRICATION_PART_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DEPTH_IN:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DEPTH_OUT:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DIAMETER_IN:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DIAMETER_OUT:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DOUBLEWALL_MATERIAL_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.FABRICATION_PART_DOUBLEWALL_MATERIAL_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_INSULATION_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.FABRICATION_PART_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FABRICATION_PART_LINING_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.FABRICATION_PART_MATERIAL_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_SHEETMETAL_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.FABRICATION_PART_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.piping:mass-2.0.0");
                case BuiltInParameter.FABRICATION_PART_WIDTH_IN:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_PART_WIDTH_OUT:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeDimension-2.0.0");
                case BuiltInParameter.FABRICATION_TOP_OF_PART:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FAMILY_ROUGH_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FAMILY_ROUGH_WIDTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FAMILY_WIDTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FBX_LIGHT_BALLAST_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_EFFICACY:
                return new ForgeTypeId("autodesk.spec.aec.electrical:efficacy-2.0.0");
                case BuiltInParameter.FBX_LIGHT_ILLUMINANCE:
                return new ForgeTypeId("autodesk.spec.aec.electrical:illuminance-2.0.0");
                case BuiltInParameter.FBX_LIGHT_INITIAL_COLOR_TEMPERATURE:
                return new ForgeTypeId("autodesk.spec.aec.electrical:colorTemperature-2.0.0");
                case BuiltInParameter.FBX_LIGHT_LAMP_LUMEN_DEPR:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_LAMP_TILT_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_LIMUNOUS_FLUX:
                return new ForgeTypeId("autodesk.spec.aec.electrical:luminousFlux-2.0.0");
                case BuiltInParameter.FBX_LIGHT_LIMUNOUS_INTENSITY:
                return new ForgeTypeId("autodesk.spec.aec.electrical:luminousIntensity-2.0.0");
                case BuiltInParameter.FBX_LIGHT_LUMENAIRE_DIRT:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_SURFACE_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_TEMPERATURE_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_TOTAL_LIGHT_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_VOLTAGE_LOSS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.FBX_LIGHT_WATTAGE:
                return new ForgeTypeId("autodesk.spec.aec.electrical:wattage-2.0.0");
                case BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.FLOOR_HEIGHTABOVELEVEL_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.GENERIC_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.GENERIC_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.HOST_AREA_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.HOST_PERIMETER_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.HOST_VOLUME_COMPUTED:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.INFRASTRUCTURE_ALIGNMENT_DISPLAYED_END_STATION:
                return new ForgeTypeId("autodesk.spec.aec.infrastructure:stationing-2.0.0");
                case BuiltInParameter.INFRASTRUCTURE_ALIGNMENT_DISPLAYED_START_STATION:
                return new ForgeTypeId("autodesk.spec.aec.infrastructure:stationing-2.0.0");
                case BuiltInParameter.INSTANCE_ELEVATION_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.INSTANCE_LENGTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.JOIST_SYSTEM_SPACING_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.LEVEL_DATA_FLOOR_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.LEVEL_DATA_FLOOR_PERIMETER:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.LEVEL_DATA_SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.LEVEL_DATA_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.LEVEL_ELEV:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.LOAD_AREA_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FX1:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FX2:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FX3:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FY1:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FY2:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FY3:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FZ1:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FZ2:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_AREA_FORCE_FZ3:
                return new ForgeTypeId("autodesk.spec.aec.structural:areaForce-2.0.0");
                case BuiltInParameter.LOAD_FORCE_FX:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.LOAD_FORCE_FY:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.LOAD_FORCE_FZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:force-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FX1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FX2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FY1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FY2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FZ1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_FORCE_FZ2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearForce-2.0.0");
                case BuiltInParameter.LOAD_LINEAR_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MX:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MX1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MX2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MY:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MY1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MY2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:moment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MZ1:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.LOAD_MOMENT_MZ2:
                return new ForgeTypeId("autodesk.spec.aec.structural:linearMoment-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_EXTERIOR_WALL_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_INTERIOR_WALL_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_OPENING_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_ROOF_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_SKYLIGHT_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_MASS_WINDOW_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_DATA_PERCENTAGE_GLAZING:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.MASS_DATA_PERCENTAGE_SKYLIGHTS:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.MASS_DATA_SHADE_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.MASS_DATA_SILL_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.MASS_DATA_SKYLIGHT_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.MASS_GROSS_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_GROSS_SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_GROSS_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.MASS_ZONE_FLOOR_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.MASS_ZONE_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.MEP_EQUIPMENT_CALC_PIPINGFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:flow-2.0.0");
                case BuiltInParameter.MEP_EQUIPMENT_CALC_PIPINGPRESSUREDROP_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:pressure-2.0.0");
                case BuiltInParameter.PATH_OF_TRAVEL_SPEED:
                return new ForgeTypeId("autodesk.spec.aec:speed-2.0.0");
                case BuiltInParameter.PATH_OF_TRAVEL_TIME:
                return new ForgeTypeId("autodesk.spec.aec:time-2.0.0");
                case BuiltInParameter.PATH_REIN_LENGTH_1:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.PATH_REIN_LENGTH_2:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.PATH_REIN_SPACING:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.PEAK_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.PEAK_COOLING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoad-2.0.0");
                case BuiltInParameter.PEAK_HEATING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoad-2.0.0");
                case BuiltInParameter.PEAK_LATENT_COOLING_LOAD:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoad-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_BENDING:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_BENDING_REINFORCEMENT:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_COMPRESSION_PARALLEL:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_COMPRESSION_PERPENDICULAR:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_CONCRETE_COMPRESSION:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF:
                return new ForgeTypeId("autodesk.spec.aec.structural:thermalExpansionCoefficient-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF1:
                return new ForgeTypeId("autodesk.spec.aec.structural:thermalExpansionCoefficient-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF2:
                return new ForgeTypeId("autodesk.spec.aec.structural:thermalExpansionCoefficient-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_EXP_COEFF3:
                return new ForgeTypeId("autodesk.spec.aec.structural:thermalExpansionCoefficient-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_MINIMUM_TENSILE_STRENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_MINIMUM_YIELD_STRESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD1:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD2:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_POISSON_MOD3:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_REDUCTION_FACTOR:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_RESISTANCE_CALC_STRENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD1:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD2:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_MOD3:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_PARALLEL:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_PERPENDICULAR:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_REINFORCEMENT:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_SHEAR_STRENGTH_REDUCTION:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_STRUCTURAL_DENSITY:
                return new ForgeTypeId("autodesk.spec.aec:massDensity-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_UNIT_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:unitWeight-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD1:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD2:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PHY_MATERIAL_PARAM_YOUNG_MOD3:
                return new ForgeTypeId("autodesk.spec.aec.structural:stress-2.0.0");
                case BuiltInParameter.PROJECTED_SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.PROPERTY_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.PROPERTY_SEGMENT_BEARING:
                return new ForgeTypeId("autodesk.spec.aec:siteAngle-2.0.0");
                case BuiltInParameter.PROPERTY_SEGMENT_DISTANCE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.PROPERTY_SEGMENT_RADIUS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_ADDITIONAL_FLOW:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.RBS_CABLETRAY_BENDRADIUS:
                return new ForgeTypeId("autodesk.spec.aec.electrical:cableTraySize-2.0.0");
                case BuiltInParameter.RBS_CABLETRAY_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:cableTraySize-2.0.0");
                case BuiltInParameter.RBS_CABLETRAY_WIDTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:cableTraySize-2.0.0");
                case BuiltInParameter.RBS_CONDUIT_BENDRADIUS:
                return new ForgeTypeId("autodesk.spec.aec.electrical:conduitSize-2.0.0");
                case BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:conduitSize-2.0.0");
                case BuiltInParameter.RBS_CONDUIT_INNER_DIAM_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:conduitSize-2.0.0");
                case BuiltInParameter.RBS_CONDUIT_OUTER_DIAM_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:conduitSize-2.0.0");
                case BuiltInParameter.RBS_CTC_BOTTOM_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_CTC_TOP_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_CURVE_DIAMETER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductSize-2.0.0");
                case BuiltInParameter.RBS_CURVE_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductSize-2.0.0");
                case BuiltInParameter.RBS_CURVE_SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.RBS_CURVE_WIDTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductSize-2.0.0");
                case BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_DUCT_FLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.RBS_DUCT_STATIC_PRESSURE:
                return new ForgeTypeId("autodesk.spec.aec.hvac:pressure-2.0.0");
                case BuiltInParameter.RBS_DUCT_TOP_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEA:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEB:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_APPARENT_LOAD_PHASEC:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_MAINS:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEA:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEB:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_APPARENT_LOAD_PHASEC:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEA:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEB:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_BRANCH_CIRCUIT_CURRENT_PHASEC:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEB_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_CURRENT_PHASEC_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEA:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEB:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_APPARENT_LOAD_PHASEC:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEA:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEB:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_FEED_THRU_LUGS_CURRENT_PHASEC:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_MCB_RATING_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_NEUTRAL_RATING_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:demandFactor-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_CONNECTED_CURRENT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_CURRENT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:current-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_TOTAL_DEMAND_FACTOR_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:demandFactor-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_TOTALESTLOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_PANEL_TOTALLOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:apparentPower-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_AVERAGE_ILLUMINATION:
                return new ForgeTypeId("autodesk.spec.aec.electrical:illuminance-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_CAVITY_RATIO:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_LIGHTING_CALC_LUMINAIREPLANE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_LIGHTING_CALC_WORKPLANE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_CEILING:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_FLOOR:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.RBS_ELEC_ROOM_REFLECTIVITY_WALLS:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.RBS_EQ_DIAMETER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductSize-2.0.0");
                case BuiltInParameter.RBS_FP_SPRINKLER_K_FACTOR_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_FP_SPRINKLER_ORIFICE_SIZE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeSize-2.0.0");
                case BuiltInParameter.RBS_FP_SPRINKLER_TEMPERATURE_RATING_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:temperature-2.0.0");
                case BuiltInParameter.RBS_FRICTION:
                return new ForgeTypeId("autodesk.spec.aec.hvac:friction-2.0.0");
                case BuiltInParameter.RBS_GBXML_SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.RBS_HYDRAULIC_DIAMETER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductSize-2.0.0");
                case BuiltInParameter.RBS_INSULATION_LINING_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_DUCT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductInsulationThickness-2.0.0");
                case BuiltInParameter.RBS_INSULATION_THICKNESS_FOR_PIPE:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeInsulationThickness-2.0.0");
                case BuiltInParameter.RBS_LINING_THICKNESS_FOR_DUCT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductLiningThickness-2.0.0");
                case BuiltInParameter.RBS_LOSS_COEFFICIENT:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_MAX_FLOW:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.RBS_MIN_FLOW:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.RBS_PIPE_ADDITIONAL_FLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:flow-2.0.0");
                case BuiltInParameter.RBS_PIPE_BOTTOM_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_PIPE_CWFU_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_DIAMETER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeSize-2.0.0");
                case BuiltInParameter.RBS_PIPE_FIXTURE_UNITS_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_FLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:flow-2.0.0");
                case BuiltInParameter.RBS_PIPE_FLUID_DENSITY_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:density-2.0.0");
                case BuiltInParameter.RBS_PIPE_FLUID_TEMPERATURE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:temperature-2.0.0");
                case BuiltInParameter.RBS_PIPE_FLUID_VISCOSITY_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:viscosity-2.0.0");
                case BuiltInParameter.RBS_PIPE_FRICTION_FACTOR_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_FRICTION_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:friction-2.0.0");
                case BuiltInParameter.RBS_PIPE_HWFU_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_INNER_DIAM_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeSize-2.0.0");
                case BuiltInParameter.RBS_PIPE_INVERT_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_PIPE_OUTER_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.piping:pipeSize-2.0.0");
                case BuiltInParameter.RBS_PIPE_PRESSUREDROP_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:pressure-2.0.0");
                case BuiltInParameter.RBS_PIPE_RELATIVE_ROUGHNESS_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_REYNOLDS_NUMBER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_ROUGHNESS_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:roughness-2.0.0");
                case BuiltInParameter.RBS_PIPE_STATIC_PRESSURE:
                return new ForgeTypeId("autodesk.spec.aec.piping:pressure-2.0.0");
                case BuiltInParameter.RBS_PIPE_SYSTEM_FIXTURE_UNIT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PIPE_TOP_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.RBS_PIPE_VELOCITY_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:velocity-2.0.0");
                case BuiltInParameter.RBS_PIPE_VOLUME_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.piping:volume-2.0.0");
                case BuiltInParameter.RBS_PIPE_WFU_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_PRESSURE_DROP:
                return new ForgeTypeId("autodesk.spec.aec.hvac:pressure-2.0.0");
                case BuiltInParameter.RBS_REFERENCE_INSULATION_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductInsulationThickness-2.0.0");
                case BuiltInParameter.RBS_REFERENCE_LINING_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.hvac:ductLiningThickness-2.0.0");
                case BuiltInParameter.RBS_REYNOLDSNUMBER_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_ROOM_COEFFICIENT_UTILIZATION:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.RBS_VELOCITY:
                return new ForgeTypeId("autodesk.spec.aec.hvac:velocity-2.0.0");
                case BuiltInParameter.RBS_VELOCITY_PRESSURE:
                return new ForgeTypeId("autodesk.spec.aec.hvac:pressure-2.0.0");
                case BuiltInParameter.REBAR_BAR_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:barDiameter-2.0.0");
                case BuiltInParameter.REBAR_ELEM_BAR_SPACING:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.REBAR_ELEM_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.REBAR_ELEM_TOTAL_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.REBAR_HOOK_ROTATION_AT_END_SCHEDULES_TAGS_FILTERS:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.REBAR_HOOK_ROTATION_AT_START_SCHEDULES_TAGS_FILTERS:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.REBAR_INSTANCE_BEND_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:barDiameter-2.0.0");
                case BuiltInParameter.REBAR_MAX_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
                case BuiltInParameter.REBAR_MIN_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementLength-2.0.0");
#if D2022 || R2022
                case BuiltInParameter.REBAR_MODEL_BAR_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:barDiameter-2.0.0");
#endif
                case BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_1_GENERIC:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.REBAR_SYSTEM_SPACING_BOTTOM_DIR_2_GENERIC:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_1_GENERIC:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.REBAR_SYSTEM_SPACING_TOP_DIR_2_GENERIC:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementSpacing-2.0.0");
                case BuiltInParameter.REIN_EST_BAR_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementVolume-2.0.0");
                case BuiltInParameter.REINFORCEMENT_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec.structural:reinforcementVolume-2.0.0");
                case BuiltInParameter.ROOF_ATTR_THICKNESS_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOF_LEVEL_OFFSET_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOF_SLOPE:
                return new ForgeTypeId("autodesk.spec.aec:slope-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_EXHAUST_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_LIGHTING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:power-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_LIGHTING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_POWER_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:power-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_POWER_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_RETURN_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_ACTUAL_SUPPLY_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_AIR_CHANGES_PER_HOUR_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ROOM_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.ROOM_AREA_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_COOLING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoad-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_COOLING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoadDividedByArea-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_HEATING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoad-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_HEATING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoadDividedByArea-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_SUPPLY_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_CALCULATED_SUPPLY_AIRFLOW_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_COOLING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoad-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_EXHAUST_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_HEATING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoad-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_LIGHTING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:power-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_LIGHTING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_MECHANICAL_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_OTHER_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_POWER_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:power-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_POWER_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_RETURN_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_DESIGN_SUPPLY_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOM_LOWER_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOM_NUMBER_OF_PEOPLE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ROOM_OUTDOOR_AIR_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.ROOM_OUTDOOR_AIR_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_OUTDOOR_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ROOM_PEOPLE_LATENT_HEAT_GAIN_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatGain-2.0.0");
                case BuiltInParameter.ROOM_PEOPLE_SENSIBLE_HEAT_GAIN_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatGain-2.0.0");
                case BuiltInParameter.ROOM_PEOPLE_TOTAL_HEAT_GAIN_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatGain-2.0.0");
                case BuiltInParameter.ROOM_PERIMETER:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOM_PLENUM_LIGHTING_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.ROOM_UPPER_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ROOM_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.SCHEDULE_BASE_LEVEL_OFFSET_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.SCHEDULE_TOP_LEVEL_OFFSET_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.SPACE_AIR_CHANGES_PER_HOUR:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.SPACE_AIRFLOW_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
#if D2022 || R2022
                case BuiltInParameter.SPACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
#endif
                case BuiltInParameter.SPACE_AREA_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.SPACE_COOLING_SET_POINT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.SPACE_DEHUMIDIFICATION_SET_POINT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.SPACE_HEATING_SET_POINT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.SPACE_HUMIDIFICATION_SET_POINT:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.SPACE_INFILTRATION_AIRFLOW_PER_AREA:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.SPACE_LIGHTING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:power-2.0.0");
                case BuiltInParameter.SPACE_LIGHTING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW_PER_AREA:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.SPACE_OUTDOOR_AIRFLOW_PER_PERSON:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.SPACE_PEOPLE_LATENT_HEAT_GAIN_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatGain-2.0.0");
                case BuiltInParameter.SPACE_PEOPLE_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:power-2.0.0");
                case BuiltInParameter.SPACE_PEOPLE_SENSIBLE_HEAT_GAIN_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatGain-2.0.0");
                case BuiltInParameter.SPACE_POWER_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:power-2.0.0");
                case BuiltInParameter.SPACE_POWER_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.electrical:powerDensity-2.0.0");
#if D2022 || R2022
                case BuiltInParameter.SPACE_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
#endif
                case BuiltInParameter.STAIRS_ACTUAL_RISER_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_ACTUAL_TREAD_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_ATTR_MAX_RISER_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_ATTR_MINIMUM_TREAD_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_ATTR_TREAD_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_ATTR_TREAD_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_LANDINGTYPE_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RAILING_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RAILING_HEIGHT_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RAILING_PLACEMENT_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RUN_ACTUAL_RISER_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RUN_ACTUAL_RUN_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RUN_ACTUAL_TREAD_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RUN_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_RUNTYPE_STRUCTURAL_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_SUPPORTTYPE_STRUCTURAL_DEPTH_ON_LANDING:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_SUPPORTTYPE_STRUCTURAL_DEPTH_ON_RUN:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_SUPPORTTYPE_TOTAL_DEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STAIRS_SUPPORTTYPE_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.START_EXTENSION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.START_JOIN_CUTBACK:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.START_Y_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.START_Z_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_ANCHOR_TOTAL_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_BOLT_GRIP_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_BOLT_GRIP_LENGTH_INCREASE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_BOLT_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_BOLT_TOTAL_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_CUT_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_EXACT_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PAINT_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_EDGE_DISTANCE_X:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_EDGE_DISTANCE_Y:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_INTERMEDIATE_DISTANCE_X:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_INTERMEDIATE_DISTANCE_Y:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_RADIUS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_TOTAL_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PATTERN_TOTAL_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_EXACT_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_JUSTIFICATION:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_PAINT_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PLATE_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PROFILE_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_PROFILE_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.STEEL_ELEM_SHEARSTUD_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_SHEARSTUD_TOTAL_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:mass-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_EFFECTIVETHROAT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_PREPDEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_ROOTOPENING:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_DOUBLE_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_EFFECTIVETHROAT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_PREPDEPTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_ROOTOPENING:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_MAIN_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STEEL_ELEM_WELD_PITCH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_BEAM_END0_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_BEAM_END1_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_BEND_DIR_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM_CORE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_BOTTOM_SURVEY:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP_CORE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_ELEVATION_AT_TOP_SURVEY:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_FLOOR_CORE_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_FOUNDATION_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_FRAME_CUT_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_REFERENCE_LEVEL_ELEVATION:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_AREA:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionArea-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_BOTTOM_CUT_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_BOTTOM_CUT_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_CANTILEVER_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_CANTILEVER_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ALPHA:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_HORIZ:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_CENTROID_VERTICAL:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ELASTIC_MODULUS_STRONG_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionModulus-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_ELASTIC_MODULUS_WEAK_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionModulus-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_MOMENT_OF_INERTIA_STRONG_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:momentOfInertia-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_MOMENT_OF_INERTIA_WEAK_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:momentOfInertia-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_NOMINAL_WEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:weightPerUnitLength-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PERIMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:surfaceAreaPerUnitLength-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PLASTIC_MODULUS_STRONG_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionModulus-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_PLASTIC_MODULUS_WEAK_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionModulus-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_SHEAR_AREA_STRONG_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionArea-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_SHEAR_AREA_WEAK_AXIS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionArea-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_TORSIONAL_MODULUS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionModulus-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_TORSIONAL_MOMENT_OF_INERTIA:
                return new ForgeTypeId("autodesk.spec.aec.structural:momentOfInertia-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_WARPING_CONSTANT:
                return new ForgeTypeId("autodesk.spec.aec.structural:warpingConstant-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_COMMON_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_CPROFILE_FOLD_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_HSS_INNERFILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_HSS_OUTERFILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_DIAMETER:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_BETWEEN_ROWS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_TWO_ROWS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_BOLT_SPACING_WEB:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_CLEAR_WEB_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGE_TOE_OF_FILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGEFILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_FLANGETHICKNESS_LOCATION:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEB_TOE_OF_FILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBFILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBHEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ISHAPE_WEBTHICKNESS_LOCATION:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGETHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_BOTTOMFLANGEWIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGETHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_IWELDED_TOPFLANGEWIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_DIAMETER_LONGER_FLANGE:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_DIAMETER_SHORTER_FLANGE:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_1_LONGER_FLANGE:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_2_LONGER_FLANGE:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LANGLE_BOLT_SPACING_SHORTER_FLANGE:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_LPROFILE_LIP_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLDESIGNTHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_PIPESTANDARD_WALLNOMINALTHICKNESS:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_BEND_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_MIDDLE_BEND_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_SIGMA_PROFILE_TOP_BEND_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_SLOPED_FLANGE_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_SLOPED_WEB_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_CUT_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_CUT_WIDTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_TOP_WEB_FILLET:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionProperty-2.0.0");
                case BuiltInParameter.STRUCTURAL_SECTION_ZPROFILE_BOTTOM_FLANGE_LENGTH:
                return new ForgeTypeId("autodesk.spec.aec.structural:sectionDimension-2.0.0");
                case BuiltInParameter.SUPPORT_HAND_CLEARANCE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.SUPPORT_HEIGHT:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.SURFACE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.VOLUME_CUT:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.VOLUME_FILL:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.VOLUME_NET:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.WALL_ATTR_WIDTH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.WALL_BASE_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.WALL_SINGLE_SLANT_ANGLE_FROM_VERTICAL:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
#if D2022 || R2022
                case BuiltInParameter.WALL_TAPERED_EXTERIOR_INWARD_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.WALL_TAPERED_INTERIOR_INWARD_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.WALL_TAPERED_WIDTH_AT_BOTTOM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.WALL_TAPERED_WIDTH_AT_TOP:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
#endif
                case BuiltInParameter.WALL_TOP_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
#if D2022 || R2022
                case BuiltInParameter.WALL_TYPE_DEFAULT_TAPERED_EXTERIOR_INWARD_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
                case BuiltInParameter.WALL_TYPE_DEFAULT_TAPERED_INTERIOR_INWARD_ANGLE:
                return new ForgeTypeId("autodesk.spec.aec:angle-2.0.0");
#endif
                case BuiltInParameter.WALL_USER_HEIGHT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.WINDOW_THICKNESS:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.Y_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.Z_OFFSET_VALUE:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ZONE_AREA:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.ZONE_AREA_GROSS:
                return new ForgeTypeId("autodesk.spec.aec:area-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_AREA_PER_COOLING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:areaDividedByCoolingLoad-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_AREA_PER_HEATING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:areaDividedByHeatingLoad-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_COOLING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoad-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_COOLING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:coolingLoadDividedByArea-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_HEATING_LOAD_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoad-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_HEATING_LOAD_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:heatingLoadDividedByArea-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_SUPPLY_AIRFLOW_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ZONE_CALCULATED_SUPPLY_AIRFLOW_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.ZONE_COIL_BYPASS_PERCENTAGE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:factor-2.0.0");
                case BuiltInParameter.ZONE_COOLING_AIR_TEMPERATURE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.ZONE_COOLING_SET_POINT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.ZONE_DEHUMIDIFICATION_SET_POINT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ZONE_HEATING_AIR_TEMPERATURE_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.ZONE_HEATING_SET_POINT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:temperature-2.0.0");
                case BuiltInParameter.ZONE_HUMIDIFICATION_SET_POINT_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ZONE_LEVEL_OFFSET:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ZONE_OA_RATE_PER_ACH_PARAM:
                return new ForgeTypeId("autodesk.spec.aec:number-2.0.0");
                case BuiltInParameter.ZONE_OUTSIDE_AIR_PER_AREA_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlowDensity-2.0.0");
                case BuiltInParameter.ZONE_OUTSIDE_AIR_PER_PERSON_PARAM:
                return new ForgeTypeId("autodesk.spec.aec.hvac:airFlow-2.0.0");
                case BuiltInParameter.ZONE_PERIMETER:
                return new ForgeTypeId("autodesk.spec.aec:length-2.0.0");
                case BuiltInParameter.ZONE_VOLUME:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                case BuiltInParameter.ZONE_VOLUME_GROSS:
                return new ForgeTypeId("autodesk.spec.aec:volume-2.0.0");
                default:
                return new ForgeTypeId();
            }
        }
#endif
            }
        }
