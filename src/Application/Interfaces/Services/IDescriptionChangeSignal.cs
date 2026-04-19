using System.Threading.Channels;

namespace Application.Interfaces.Services;

public interface IDescriptionChangeSignal
{
	void NotifyDirty();
	ChannelReader<bool> Reader { get; }
}
