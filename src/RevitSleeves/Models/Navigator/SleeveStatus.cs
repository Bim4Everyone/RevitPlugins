namespace RevitSleeves.Models.Navigator;
internal enum SleeveStatus {
    Invalid,
    TooBigDiameter,
    TooSmallDiameter,
    IntersectSleeve,
    Empty,
    OutsideOfStructure,
    BeyondOpening,
    EndFaceOutsideOfStructure,
    EndFaceInsideStructure,
    IrrelevantSleeve,
    AxisNotParallelToMepElement,
    AxisDistanceTooBig
}
