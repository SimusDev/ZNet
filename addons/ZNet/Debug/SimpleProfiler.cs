using Godot;
using LiteNetLib;
using System;
using ZNet;

namespace ZNet.Debug;
public partial class SimpleProfiler : Control
{
	private ZNetMultiplayer _api;

	[Export] private Label _label;
	
	public override void _Ready()
	{

		if (_api == null)
		{
			_api = ZNetMultiplayer.Instance;
			SetMultiplayerApi(_api);
		}

	}

	public void SetMultiplayerApi(ZNetMultiplayer api)
	{
		if (_api != null)
			_api.OnNetworkStatisticsTickEvent -= OnNetworkStatisticsTick;

		_api = api;
		_api.OnNetworkStatisticsTickEvent += OnNetworkStatisticsTick;
	}

	private void OnNetworkStatisticsTick(NetStatistics statistics)
	{
		Render(statistics);
	}

	public void Render(NetStatistics statistics)
	{
		int ping = 0;
		if (_api.ServerPeer != null)
			ping = _api.ServerPeer.Ping;


		_label.Text = $"Fps: {Engine.GetFramesPerSecond()}   Ping: {ping}ms\nIn: {statistics.PacketsReceived}   " +
			$"{TrafficFormatter.FormatBytesPerSecond(statistics.BytesReceived)}\nOut: {statistics.PacketsSent}" +
			$"   {TrafficFormatter.FormatBytesPerSecond(statistics.BytesSent)}\nLoss: {statistics.PacketLossPercent}%   ID: {_api.UniqueId}";
	}


}
