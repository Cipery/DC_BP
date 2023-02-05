using System;
using WireMock.Server;

namespace User.Test.Unit;

public class IszrApiServerFixture : IDisposable
{
    public WireMockServer Server { get; private set; }

    public IszrApiServerFixture()
    {
        Server = WireMockServer.Start();
    }

    public void Dispose()
    {
        Server.Stop();
    }

    public IszrApiServerFixture GetResetInstance()
    {
        Server.Reset();
        return this;
    }
}