using System;
using AutoBogus;
using Bogus;
using FakeItEasy.AutoFakeIt;
using NUnit.Framework;

namespace A55.Extensions.Configuration.Tests;

public class GlobalSetup
{
    [OneTimeSetUp]
    public void SetUpOneTimeBase()
    {
        Randomizer.Seed = new Random(8675309);
    }
}

public class TestBase
{
    protected AutoFakeIt autoFake;
    protected Faker faker = new("pt_BR");
    protected HttpTest httpTest;


    [SetUp]
    public void SetUpBase()
    {
        autoFake = new AutoFake().ComFakeHttpClient();
        httpTest = new HttpTest();
        faker = ;
    }

    [TearDown]
    public void TearDownBase()
    {
        autoFake.Dispose();
    }
}
