using System;
using System.ComponentModel;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashCommentViewModel : BaseViewModel, IEquatable<ClashCommentViewModel> {
    private readonly RevitRepository _repo;
    private readonly ClashComment _comment;
    private string _body;
    private string _author;
    private DateTime _date;

    public ClashCommentViewModel(RevitRepository repo, ClashComment comment) {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _comment = comment ?? throw new ArgumentNullException(nameof(comment));
        Id = _comment.Id;
        Body = _comment.Body;
        Author = _comment.Author;
        Date = _comment.CreatedDate;

        PropertyChanged += ContentChangedHandler;
    }

    public int Id { get; }

    public string Body {
        get => _body;
        set => RaiseAndSetIfChanged(ref _body, value);
    }

    public string Author {
        get => _author;
        set => RaiseAndSetIfChanged(ref _author, value);
    }

    public DateTime Date {
        get => _date;
        set => RaiseAndSetIfChanged(ref _date, value);
    }

    public ClashComment GetComment() {
        _comment.Author = Author;
        _comment.Body = Body;
        _comment.CreatedDate = Date;
        return _comment;
    }

    public bool Equals(ClashCommentViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_comment, other._comment);
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((ClashCommentViewModel) obj);
    }

    public override int GetHashCode() {
        return (_comment != null ? _comment.GetHashCode() : 0);
    }

    private void ContentChangedHandler(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Body)) {
            Author = _repo.UiApplication.Application.Username;
            Date = DateTime.Now;
        }
    }
}
