namespace RevitLintelPlacement.Models.Interfaces;

internal interface IResultHandler {
    ResultCode Code { get; set; }
    void Handle();
}
