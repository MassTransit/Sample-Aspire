namespace Sample.Components;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using MassTransit;
using MassTransit.Internals;


public class CustomerNumberHeaderFilter<T> :
    IFilter<SendContext<T>>,
    IFilter<PublishContext<T>>
    where T : class
{
    static readonly ReadOnlyProperty<T, string>? _property;

    static CustomerNumberHeaderFilter()
    {
        if (IsCustomerMessage(out var propertyInfo))
            _property = new ReadOnlyProperty<T, string>(propertyInfo);
    }

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        SetPartitionKey(context);

        return next.Send(context);
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        SetPartitionKey(context);

        return next.Send(context);
    }

    public void Probe(ProbeContext context)
    {
    }

    static void SetPartitionKey(SendContext<T> context)
    {
        if (_property == null)
            return;

        var customerNumber = _property.GetProperty(context.Message);

        if (!string.IsNullOrWhiteSpace(customerNumber))
            context.TrySetPartitionKey(customerNumber);
    }

    static bool IsCustomerMessage([NotNullWhen(true)] out PropertyInfo? propertyInfo)
    {
        propertyInfo = typeof(T).GetProperty("CustomerNumber");

        return propertyInfo != null;
    }
}