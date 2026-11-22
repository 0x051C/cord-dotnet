using Cord.Core;

namespace Cord.Tests;

public class WaybillImportTests
{
    private sealed class FakeStore : IWaybillStore
    {
        public List<IReadOnlyCollection<Waybill>> Calls { get; } = [];

        public Task UpsertAsync(IReadOnlyCollection<Waybill> waybills, CancellationToken cancellationToken = default)
        {
            Calls.Add(waybills);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Saves_only_the_valid_rows()
    {
        var store = new FakeStore();
        await using var file = File.OpenRead(SampleRows.Fixture("ttn_mock.csv"));

        var result = await new WaybillImport(store).ImportAsync("ttn_mock.csv", file, TestContext.Current.CancellationToken);

        Assert.Equal(8, result.InvalidRows);
        Assert.Equal(32, Assert.Single(store.Calls).Count);
    }

    [Fact]
    public async Task Does_not_call_the_store_when_nothing_is_valid()
    {
        var store = new FakeStore();
        using var file = new MemoryStream("header\nbroken^row\n"u8.ToArray());

        var result = await new WaybillImport(store).ImportAsync("ttn_bad.csv", file, TestContext.Current.CancellationToken);

        Assert.Equal(1, result.InvalidRows);
        Assert.Empty(store.Calls);
    }
}
