using MajdataPlay.Types;
using MajSimai;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class SimaiProcessExtensions
    {
        public static void Scale(this SimaiChart source,float timeScale)
        {
            var timingPoints = source.NoteTimings;
            foreach(var timingPoint in timingPoints)
            {
                timingPoint.Bpm *= timeScale;
                timingPoint.Timing /= timeScale;
                foreach(var note in timingPoint.Notes)
                {
                    note.HoldTime /= timeScale;
                    note.SlideStartTime /= timeScale;
                    note.SlideTime /= timeScale;
                }
            }
        }
        public static void ConvertToBreak(this SimaiChart source)
        {
            var timingPoints = source.NoteTimings;
            foreach (var timingPoint in timingPoints)
            {
                foreach (var note in timingPoint.Notes)
                {
                    note.IsBreak = true;
                    note.IsSlideBreak = true;
                }
            }
        }
        public static void ConvertToEx(this SimaiChart source)
        {
            var timingPoints = source.NoteTimings;
            foreach (var timingPoint in timingPoints)
            {
                foreach (var note in timingPoint.Notes)
                    note.IsEx = true;
            }
        }
        public static void ConvertToTouch(this SimaiChart source)
        {
            var timingPoints = source.NoteTimings;
            foreach (var timingPoint in timingPoints)
            {
                var notes = timingPoint.Notes;
                var touchNotes = notes.Where(x => x.Type is SimaiNoteType.Touch or SimaiNoteType.TouchHold);
                var newNoteList = new List<SimaiNote>();
                var noteCount = notes.Count();
                for (var i = 0; i < noteCount; i++)
                {
                    var note = notes[i];
                    switch(note.Type)
                    {
                        case SimaiNoteType.Touch:
                        case SimaiNoteType.TouchHold:
                            continue;
                        case SimaiNoteType.Slide:
                            {
                                if (note.IsSlideNoHead)
                                {
                                    newNoteList.Add(note);
                                    continue;
                                }
                                note.IsSlideNoHead = true;
                                var startKey = note.StartPosition;
                                newNoteList.Add(note);
                                newNoteList.Add(new SimaiNote()
                                {
                                    Type = SimaiNoteType.Touch,
                                    StartPosition = startKey,
                                    TouchArea = 'A',
                                    IsBreak = note.IsBreak,
                                    IsEx = note.IsEx
                                });
                            }
                            break;
                        case SimaiNoteType.Tap:
                            {
                                var startKey = note.StartPosition;
                                newNoteList.Add(new SimaiNote()
                                {
                                    Type = SimaiNoteType.Touch,
                                    StartPosition = startKey,
                                    TouchArea = 'A',
                                    IsBreak = note.IsBreak,
                                    IsEx = note.IsEx
                                });
                            }
                            break;
                        case SimaiNoteType.Hold:
                            {
                                var startKey = note.StartPosition;
                                newNoteList.Add(new SimaiNote()
                                {
                                    Type = SimaiNoteType.TouchHold,
                                    StartPosition = startKey,
                                    HoldTime = note.HoldTime,
                                    TouchArea = 'A',
                                    IsBreak = note.IsBreak,
                                    IsEx = note.IsEx
                                });
                            }
                            break;
                    }
                }
                foreach (var touch in touchNotes)
                    newNoteList.Add(touch);
                timingPoint.Notes = newNoteList.ToArray();
            }
        }
        public static void Clamp(this SimaiChart source,Range<long> noteIndexRange)
        {
            List<SimaiTimingPoint> newTimingList = new();
            var currentIndex = 0;
            for (var i = 0; i < source.NoteTimings.Length; i++)
            {
                var noteTiming = source.NoteTimings[i];
                List<SimaiNote> newNoteList = new();
                for (var j = 0; j < noteTiming.Notes.Length; j++)
                {
                    var note = noteTiming.Notes[j];
                    switch (note.Type)
                    {
                        case SimaiNoteType.Tap:
                        case SimaiNoteType.Hold:
                        case SimaiNoteType.Touch:
                        case SimaiNoteType.TouchHold:
                            if (noteIndexRange.InRange(currentIndex))
                            {
                                newNoteList.Add(note);
                            }
                            break;
                        case SimaiNoteType.Slide:
                            if (note.IsSlideNoHead)
                            {
                                if (noteIndexRange.InRange(currentIndex))
                                {
                                    newNoteList.Add(note);
                                }
                            }
                            else
                            {
                                if (noteIndexRange.InRange(currentIndex + 1))
                                {
                                    newNoteList.Add(note);
                                    currentIndex++;
                                }
                                else if (noteIndexRange.InRange(currentIndex))
                                {
                                    newNoteList.Add(new()
                                    {
                                        Type = SimaiNoteType.Tap,
                                        IsForceStar = true,
                                        IsFakeRotate = true,
                                        IsBreak = note.IsBreak,
                                        IsEx = note.IsEx,
                                        RawContent = $"{note.StartPosition}",
                                        StartPosition = note.StartPosition,
                                    });
                                }
                            }
                            break;
                    }
                    currentIndex++;
                }
                if (newNoteList.Count != 0) 
                {
                    var newTimingPoint = new SimaiTimingPoint(noteTiming.Timing,
                                                              newNoteList.ToArray(),
                                                              noteTiming.RawTextPositionX,
                                                              noteTiming.RawTextPositionY ,
                                                              noteTiming.RawContent,
                                                              noteTiming.Bpm, 
                                                              noteTiming.HSpeed);
                    newTimingList.Add(newTimingPoint);
                }
            }
            source.NoteTimings = newTimingList.ToArray();
        }
        public static void Clamp(this SimaiChart source, Range<double> timestampRange)
        {
            List<SimaiTimingPoint> newTimingList = new();
            foreach(var noteTiming in source.NoteTimings)
            {
                if(timestampRange.InRange(noteTiming.Timing))
                {
                    newTimingList.Add(noteTiming);
                }
            }
            source.NoteTimings = newTimingList.ToArray();
        }
    }
}