﻿using Discord.Helpers;
using System;
using System.Threading.Tasks;

namespace Discord
{
    public partial class DiscordClient
	{
        public Task JoinVoiceServer(Channel channel)
			=> JoinVoiceServer(channel.ServerId, channel.Id);
		public async Task JoinVoiceServer(string serverId, string channelId)
		{
			CheckReady(checkVoice: true);
			if (serverId == null) throw new ArgumentNullException(nameof(serverId));
			if (channelId == null) throw new ArgumentNullException(nameof(channelId));

			await LeaveVoiceServer().ConfigureAwait(false);
			_voiceSocket.SetServer(serverId);
			_dataSocket.SendJoinVoice(serverId, channelId);
			//await _voiceSocket.WaitForConnection().ConfigureAwait(false);
			//TODO: Add another ManualResetSlim to wait on here, base it off of DiscordClient's setup
		}
		public async Task LeaveVoiceServer()
		{
			CheckReady(checkVoice: true);

			if (_voiceSocket.State != Net.WebSockets.WebSocketState.Disconnected)
			{
				await _voiceSocket.Disconnect().ConfigureAwait(false);
				await TaskHelper.CompletedTask.ConfigureAwait(false);
				_dataSocket.SendLeaveVoice();
			}
		}

		/// <summary> Sends a PCM frame to the voice server. Will block until space frees up in the outgoing buffer. </summary>
		/// <param name="data">PCM frame to send. This must be a single or collection of uncompressed 48Kz monochannel 20ms PCM frames. </param>
		/// <param name="count">Number of bytes in this frame. </param>
		public void SendVoicePCM(byte[] data, int count)
		{
			CheckReady(checkVoice: true);
			if (data == null) throw new ArgumentException(nameof(data));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count == 0) return;
			
			_voiceSocket.SendPCMFrames(data, count);
		}
		/// <summary> Clears the PCM buffer. </summary>
		public void ClearVoicePCM()
		{
			CheckReady(checkVoice: true);

			_voiceSocket.ClearPCMFrames();
		}

		/// <summary> Returns a task that completes once the voice output buffer is empty. </summary>
		public async Task WaitVoice()
		{
			CheckReady(checkVoice: true);

			_voiceSocket.Wait();
			await TaskHelper.CompletedTask.ConfigureAwait(false);
		}
	}
}
