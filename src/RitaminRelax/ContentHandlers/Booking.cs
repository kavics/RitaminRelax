using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Schema;
using SenseNet.ContentRepository.Storage;

namespace RitaminRelax.ContentHandlers;

[ContentHandler]
public class Booking : GenericContent
{
    public Booking(Node parent) : base(parent) { }
    protected Booking(Node parent, string nodeTypeName) : base(parent, nodeTypeName) { }
    protected Booking(NodeToken nt) : base(nt) { }


    [RepositoryProperty(nameof(Customer),RepositoryDataType.Reference)]
    public User Customer
    {
        get => base.GetReference<User>(nameof(Customer));
        set => base.SetReference(nameof(Customer), value);
    }

    [RepositoryProperty(nameof(BookingTime), RepositoryDataType.DateTime)]
    public DateTime BookingTime
    {
        get => base.GetProperty<DateTime>(nameof(BookingTime));
        set => base.SetProperty(nameof(BookingTime), value);
    }

    [RepositoryProperty(nameof(BookingPeriod), RepositoryDataType.Int)]
    public int BookingPeriod
    {
        get => base.GetProperty<int>(nameof(BookingPeriod));
        set => base.SetProperty(nameof(BookingPeriod), value);
    }

    [RepositoryProperty(nameof(BookingType), RepositoryDataType.String)]
    public string BookingType
    {
        get => base.GetProperty<string>(nameof(BookingType));
        set => base.SetProperty(nameof(BookingType), value);
    }

    [RepositoryProperty(nameof(Accepted), RepositoryDataType.Int)]
    public bool Accepted
    {
        get => base.GetProperty<int>(nameof(Accepted)) != 0;
        set => base.SetProperty(nameof(Accepted), value ? 1 : 0);
    }

    public override object? GetProperty(string name)
    {
        switch (name)
        {
            case nameof(Customer): return Customer;
            case nameof(BookingTime): return BookingTime;
            case nameof(BookingPeriod): return BookingPeriod;
            case nameof(BookingType): return BookingType;
            case nameof(Accepted): return Accepted;
            default: return base.GetProperty(name);
        }
    }

    public override void SetProperty(string name, object value)
    {
        switch (name)
        {
            case nameof(Customer): Customer = (User)value; break;
            case nameof(BookingTime): BookingTime = (DateTime)value; break;
            case nameof(BookingPeriod): BookingPeriod = (int)value; break;
            case nameof(BookingType): BookingType = (string)value; break;
            case nameof(Accepted): Accepted = (bool)value; break;
            default: base.SetProperty(name, value); break;
        }
    }

    public override System.Threading.Tasks.Task SaveAsync(NodeSaveSettings settings, CancellationToken cancel)
    {
        return base.SaveAsync(settings, cancel);
    }
}
