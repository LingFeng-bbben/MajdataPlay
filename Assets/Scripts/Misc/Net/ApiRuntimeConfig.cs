using MajdataPlay.Settings;
using System;
using UnityEngine;
#nullable enable
namespace MajdataPlay.Net
{
	public class ApiRuntimeConfig
	{
		public Sprite? Avatar { get; set; }
		public string Username { get; set; } = string.Empty;
		public NetAuthMethodOption AuthMethod { get; set; } = NetAuthMethodOption.None;
		public string? AuthUsername { get; set; }
		public string? AuthPassword { get; set; }
	}
}