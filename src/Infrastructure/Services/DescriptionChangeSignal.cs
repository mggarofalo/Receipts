using System.Threading.Channels;
using Application.Interfaces.Services;

namespace Infrastructure.Services;

public class DescriptionChangeSignal : IDescriptionChangeSignal
{
	private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });

	public void NotifyDirty() => _channel.Writer.TryWrite(true);

	public ChannelReader<bool> Reader => _channel.Reader;
}
