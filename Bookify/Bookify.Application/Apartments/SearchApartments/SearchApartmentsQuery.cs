using Bookify.Application.Abstractions.Messaging;

namespace Bookify.Application.Apartments.SearchApartments;

public sealed record SearchApartmentsQuery(
    DateTime StartDate,
    DateTime EndDate) : IQuery<IReadOnlyList<ApartmentResponse>>;
