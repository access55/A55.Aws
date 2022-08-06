using System.Collections.Generic;
using System.Linq;
using AutoBogus;
using Bogus;
using FakeItEasy;

namespace A55.Extensions.Configuration.Tests;

public static class Extensions
{
    public static T GenerateFake<T>(this Faker<T> faker) where T : class
    {
        var fake = A.Fake<T>();
        faker.Populate(fake);
        return fake;
    }

    public static IReadOnlyCollection<T> GenerateFake<T>(this Faker<T> faker, int qtd) where T : class =>
        Enumerable.Range(0, qtd)
            .Select(_ => faker.GenerateFake())
            .ToList();
}
