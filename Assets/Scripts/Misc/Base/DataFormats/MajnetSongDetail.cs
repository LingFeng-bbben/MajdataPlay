using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;

namespace MajdataPlay
{
    public partial class MajnetSongDetail
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Designer { get; set; }
        public string Description { get; set; }
        public string[] Levels { get; set; }
        public string Uploader { get; set; }
        public DateTime Timestamp { get; set; }
        public string Hash { get; set; }
        public string[] Tags { get; set; } = new string[0];
        public string[] PublicTags { get; set; } = new string[0];
    }
    public struct MajNetSongInteract
    {
        public bool IsLiked { get; init; }
        public int Plays { get; init; }
        public string[] Likes { get; init; }
        public int DisLikeCount { get; init; }
        public ChartCommentSummary[] Comments { get; init; }
    }
    public readonly struct ChartCommentSummary
    {
        public UserSummary Sender { get; init; }
        public string Content { get; init; }
        public DateTime Timestamp { get; init; }
        public ChartCommentSummary[] Layers { get; init; }
    }
    public readonly struct UserSummary
    {
        public string Username { get; init; }
        public string Email { get; init; }
        public DateTime JoinDate { get; init; }
    }
}
