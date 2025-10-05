using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace MajdataPlay
{
    [Preserve]
    public class MajnetSongDetail
    {
        [Preserve]
        public string Id { get; set; }
        [Preserve]
        public string Title { get; set; }
        [Preserve]
        public string Artist { get; set; }
        [Preserve]
        public string Designer { get; set; }
        [Preserve]
        public string Description { get; set; }
        [Preserve]
        public string[] Levels { get; set; }
        [Preserve]
        public string Uploader { get; set; }
        [Preserve]
        public DateTime Timestamp { get; set; }
        [Preserve]
        public string Hash { get; set; }
        [Preserve]
        public string[] Tags { get; set; } = Array.Empty<string>();
        [Preserve]
        public string[] PublicTags { get; set; } = Array.Empty<string>();
    }
    [Preserve]
    public struct MajNetSongInteract
    {
        [Preserve]
        public bool IsLiked { get; init; }
        [Preserve]
        public int Plays { get; init; }
        [Preserve]
        public string[] Likes { get; init; }
        [Preserve]
        public int DisLikeCount { get; init; }
        [Preserve]
        public ChartCommentSummary[] Comments { get; init; }
    }
    [Preserve]
    public readonly struct ChartCommentSummary
    {
        [Preserve]
        public UserSummary Sender { get; init; }
        [Preserve]
        public string Content { get; init; }
        [Preserve]
        public DateTime Timestamp { get; init; }
        [Preserve]
        public ChartCommentSummary[] Layers { get; init; }
    }
    [Preserve]
    public readonly struct UserSummary
    {
        [Preserve]
        public string Username { get; init; }
        [Preserve]
        public string Email { get; init; }
        [Preserve]
        public DateTime JoinDate { get; init; }
    }
}
