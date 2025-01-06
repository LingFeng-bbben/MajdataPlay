using MajSimaiDecode;
using System.Collections.Generic;
using System.Linq;
#nullable enable
namespace MajdataPlay.Extensions
{
    public static class SimaiProcessExtensions
    {
        public static void Scale(this SimaiProcess source,float timeScale)
        {
            var timingPoints = source.notelist;
            foreach(var timingPoint in timingPoints)
            {
                timingPoint.currentBpm *= timeScale;
                timingPoint.time /= timeScale;
                foreach(var note in timingPoint.noteList)
                {
                    note.holdTime /= timeScale;
                    note.slideStartTime /= timeScale;
                    note.slideTime /= timeScale;
                }
            }
        }
        public static void ConvertToBreak(this SimaiProcess source)
        {
            var timingPoints = source.notelist;
            foreach (var timingPoint in timingPoints)
            {
                foreach (var note in timingPoint.noteList)
                {
                    note.isBreak = true;
                    note.isSlideBreak = true;
                }
            }
        }
        public static void ConvertToEx(this SimaiProcess source)
        {
            var timingPoints = source.notelist;
            foreach (var timingPoint in timingPoints)
            {
                foreach (var note in timingPoint.noteList)
                    note.isEx = true;
            }
        }
        public static void ConvertToTouch(this SimaiProcess source)
        {
            var timingPoints = source.notelist;
            foreach (var timingPoint in timingPoints)
            {
                var notes = timingPoint.noteList;
                var touchNotes = notes.Where(x => x.noteType is SimaiNoteType.Touch or SimaiNoteType.TouchHold);
                var newNoteList = new List<SimaiNote>();
                var noteCount = notes.Count();
                for (var i = 0; i < noteCount; i++)
                {
                    var note = notes[i];
                    switch(note.noteType)
                    {
                        case SimaiNoteType.Touch:
                        case SimaiNoteType.TouchHold:
                            continue;
                        case SimaiNoteType.Slide:
                            {
                                if (note.isSlideNoHead)
                                {
                                    newNoteList.Add(note);
                                    continue;
                                }
                                note.isSlideNoHead = true;
                                var startKey = note.startPosition;
                                newNoteList.Add(note);
                                newNoteList.Add(new SimaiNote()
                                {
                                    noteType = SimaiNoteType.Touch,
                                    startPosition = startKey,
                                    touchArea = 'A',
                                    isBreak = note.isBreak,
                                    isEx = note.isEx
                                });
                            }
                            break;
                        case SimaiNoteType.Tap:
                            {
                                var startKey = note.startPosition;
                                newNoteList.Add(new SimaiNote()
                                {
                                    noteType = SimaiNoteType.Touch,
                                    startPosition = startKey,
                                    touchArea = 'A',
                                    isBreak = note.isBreak,
                                    isEx = note.isEx
                                });
                            }
                            break;
                        case SimaiNoteType.Hold:
                            {
                                var startKey = note.startPosition;
                                newNoteList.Add(new SimaiNote()
                                {
                                    noteType = SimaiNoteType.TouchHold,
                                    startPosition = startKey,
                                    holdTime = note.holdTime,
                                    touchArea = 'A',
                                    isBreak = note.isBreak,
                                    isEx = note.isEx
                                });
                            }
                            break;
                    }
                }
                foreach (var touch in touchNotes)
                    newNoteList.Add(touch);
                timingPoint.noteList = newNoteList;
            }
        }
    }
}