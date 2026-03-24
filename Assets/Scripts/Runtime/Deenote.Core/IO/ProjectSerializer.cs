#nullable enable

using CommunityToolkit.HighPerformance.Buffers;
using Deenote.CoreB.Models;
using Deenote.CoreB.Models.Charts;
using Deenote.CoreB.Models.Notes;
using Deenote.CoreB.Models.Projects;
using Deenote.Library.Collections;
using Deenote.Library.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deenote.CoreB.IO
{
    internal sealed class ProjectSerializer : IProjectSerializer
    {
        public static ProjectSerializer Instance { get; } = new ProjectSerializer();

        public const byte FileVersionMark = 1;

        public Task<ProjectModel?> DeserializeAsync(FileStream stream, CancellationToken cancellationToken = default)
        {
            using var reader = new BinaryReader(stream);
            var header = reader.ReadUInt16();
            if (header != ProjectIO.DeenoteProjectFileHeader)
                return Task.FromResult<ProjectModel?>(null);

            var version = reader.ReadByte();
            if (version != FileVersionMark)
                return Task.FromResult<ProjectModel?>(null);

            var project = Task.Run(() => ReadProject(reader, cancellationToken));
            return project!;
        }

        public async Task SerializeAsync(ProjectModel project, string path, CancellationToken cancellationToken = default)
        {
            var tmpPath = $"{path}.tmp";
            try {
                using (var fs = File.OpenWrite(tmpPath)) {
                    using var writer = new BinaryWriter(fs);
                    writer.Write(ProjectIO.DeenoteProjectFileHeader);
                    writer.Write(FileVersionMark);
                    await Task.Run(() => WriteProject(writer, project, cancellationToken));
                }
            } catch (OperationCanceledException) {
                File.Delete(tmpPath);
                return;
            }

            if (File.Exists(path)) {
                File.Delete(path);
            }
            File.Move(tmpPath, path);
        }

        private void WriteProject(BinaryWriter writer, ProjectModel project, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            writer.Write(project.MusicName);
            writer.Write(project.Composer);
            writer.Write(project.ChartDesigner);
            writer.Write(project.AudioFileRelativePath);
            writer.WriteArrayWithLengthPrefix(project.AudioFileData);

            cancellationToken.ThrowIfCancellationRequested();

            writer.Write(project.Charts.Count);
            foreach (var chart in project.Charts) {
                WriteChart(writer, chart);
            }

            cancellationToken.ThrowIfCancellationRequested();

            writer.Write(project.Tempos.Count);
            foreach (var tempo in project.Tempos) {
                writer.Write(tempo);
            }
        }

        private static ProjectModel ReadProject(BinaryReader reader, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var musicName = reader.ReadString();
            var composer = reader.ReadString();
            var chartDesigner = reader.ReadString();
            var audioFileRelativePath = reader.ReadString();
            var audioFileData = reader.ReadArrayWithLengthPrefix();

            cancellationToken.ThrowIfCancellationRequested();

            var proj = new ProjectModel {
                AudioFileData = audioFileData,
                AudioFileRelativePath = audioFileRelativePath,
                MusicName = musicName,
                Composer = composer,
                ChartDesigner = chartDesigner,
            };

            var chartCount = reader.ReadInt32();
            proj.Charts.EnsureCapacity(chartCount);
            for (int i = 0; i < chartCount; i++)
                proj.Charts.Add(ReadChart(reader));

            cancellationToken.ThrowIfCancellationRequested();

            var tempoCount = reader.ReadInt32();
            proj.Tempos.EnsureCapacity(tempoCount);
            for (int i = 0; i < tempoCount; i++)
                proj.Tempos.Add(reader.Read<Tempo>());

            return proj;
        }

        private void WriteChart(BinaryWriter writer, ChartModel chart)
        {
            writer.Write(chart.Name);
            writer.Write(chart.Difficulty);
            writer.Write(chart.Level);
            writer.Write(chart.Speed);
            writer.Write(chart.RemapMinVolume);
            writer.Write(chart.RemapMaxVolume);

            var linksLookup = new Dictionary<NoteData, int>();
            int noteIndex = 0;

            foreach (var note in chart.Notes) {
                if (note.NextLink is not null)
                    linksLookup.Add(note, noteIndex);
                noteIndex++;
            }

            foreach (var note in chart.BackgroundNotes) {
                if (note._data.NextLink is not null)
                    linksLookup.Add(note._data, noteIndex);
                noteIndex++;
            }

            foreach (var note in chart.WarningNotes) {
                if (note._data.NextLink is not null)
                    linksLookup.Add(note._data, noteIndex);
                noteIndex++;
            }

            writer.Write(chart.Notes.Count);
            foreach (var note in chart.Notes) {
                WriteNote(writer, note, linksLookup);
            }

            writer.Write(chart.BackgroundNotes.Count);
            foreach (var note in chart.BackgroundNotes) {
                WriteBackgroundNote(writer, note, linksLookup);
            }

            writer.Write(chart.WarningNotes.Count);
            foreach (var note in chart.WarningNotes) {
                WriteWarningNote(writer, note, linksLookup);
            }

            writer.Write(chart.SpeedLines.Count);
            foreach (var line in chart.SpeedLines) {
                WriteSpeedLine(writer, line);
            }
        }

        private static ChartModel ReadChart(BinaryReader reader)
        {
            var name = reader.ReadString();
            var difficulty = reader.Read<Difficulty>();
            var level = reader.ReadString();

            var speed = reader.ReadSingle();
            var remapvmin = reader.ReadInt32();
            var remapvmax = reader.ReadInt32();

            var chart = new ChartModel(speed, remapvmin, remapvmax) {
                Name = name,
                Difficulty = difficulty,
                Level = level,
            };

            var noteCount = reader.ReadInt32();
            chart.Notes.EnsureCapacity(noteCount);
            using var so_links = SpanOwner<int>.Allocate(noteCount);
            var links = so_links.Span;
            for (int i = 0; i < noteCount; i++) {
                chart.Notes.Add(ReadNote(reader, out var prevLink));
                links[i] = prevLink;
            }

            var soundCount = reader.ReadInt32();
            chart.BackgroundNotes.EnsureCapacity(soundCount);
            for (int i = 0; i < soundCount; i++) {
                chart.BackgroundNotes.Add(ReadBackgroundNote(reader));
            }

            var warningCount = reader.ReadInt32();
            chart.WarningNotes.EnsureCapacity(warningCount);
            for (int i = 0; i < warningCount; i++) {
                chart.WarningNotes.Add(ReadWarningNote(reader));
            }

            var lineCount = reader.ReadInt32();
            chart.SpeedLines.EnsureCapacity(lineCount);
            for (int i = 0; i < lineCount; i++) {
                chart.SpeedLines.Add(ReadSpeedLine(reader));
            }

            for (int i = 0; i < noteCount; i++) {
                var prevLink = links[i];
                var note = chart.Notes[i];
                NoteData prevNote;
                if (prevLink < chart.Notes.Count)
                    prevNote = chart.Notes[prevLink];
                else if (prevLink < noteCount + soundCount)
                    prevNote = chart.BackgroundNotes[prevLink - noteCount]._data;
                else
                    prevNote = chart.WarningNotes[prevLink - soundCount - noteCount]._data;

                prevNote._nextLink = note;
                note._prevLink = prevNote;
            }

            //chart.Notes.Sort(NoteComparers.ViaTime);
            //chart.BackgroundNotes.Sort(NoteComparers.ViaTime);
            //chart.WarningNotes.Sort(NoteComparers.ViaTime);
            //chart.SpeedLines.Sort

            return chart;
        }

        private void WriteNote(BinaryWriter writer, NoteData note, Dictionary<NoteData, int> linksLookup)
        {
            writer.Write(note.Position);
            writer.Write(note.Time);
            writer.Write(note.Size);
            writer.Write(note.Duration);
            writer.Write(note.Kind);
            writer.Write(note.Speed);
            writer.Write(note.Sounds.Count);
            foreach (var sound in note.Sounds) {
                WriteSound(writer, sound);
            }
            writer.Write(note.Shift);
            writer.Write(note.EventId);
            writer.Write(note.WarningType);
            writer.Write(note.Vibrate);
            if (note.PrevLink is not null)
                writer.Write(linksLookup[note]);
            else
                writer.Write(-1);
        }

        private static NoteData ReadNote(BinaryReader reader, out int prevLink)
        {
            var note = new NoteData();
            note.Position = reader.ReadSingle();
            note.Time = reader.ReadSingle();
            note.Size = reader.ReadSingle();
            note.Duration = reader.ReadSingle();
            note.SetKind(reader.Read<NoteKind>());
            note.Speed = reader.ReadSingle();
            var soundsLen = reader.ReadInt32();
            note.Sounds.EnsureCapacity(soundsLen);
            for (int i = 0; i < soundsLen; i++) {
                note.Sounds.Add(ReadSound(reader));
            }
            note.Shift = reader.ReadSingle();
            note.EventId = reader.ReadString();
            note.WarningType = reader.Read<WarningType>();
            note.Vibrate = reader.ReadBoolean();
            prevLink = reader.ReadInt32();
            return note;
        }

        private void WriteBackgroundNote(BinaryWriter writer, BackgroundNoteModel note, Dictionary<NoteData, int> linksLookup)
        {
            writer.Write(note.Time);
            writer.Write(note.Sounds.Count);
            foreach (var sound in note.Sounds) {
                WriteSound(writer, sound);
            }
        }

        private static BackgroundNoteModel ReadBackgroundNote(BinaryReader reader)
        {
            var time = reader.ReadSingle();
            var len = reader.ReadInt32();
            if (len > 512) {
                using var so_sounds = SpanOwner<PianoSoundData>.Allocate(len);
                var sounds = so_sounds.Span;
                for (var i = 0; i < len; i++) {
                    sounds[i] = ReadSound(reader);
                }
                return new BackgroundNoteModel(time, sounds);
            }
            else {
                var sounds = (stackalloc PianoSoundData[len]);
                for (var i = 0; i < len; i++) {
                    sounds[i] = ReadSound(reader);
                }
                return new BackgroundNoteModel(time, sounds);
            }
        }

        private void WriteWarningNote(BinaryWriter writer, WarningNoteModel note, Dictionary<NoteData, int> linksLookup)
        {
            writer.Write(note.Time);
        }

        private static WarningNoteModel ReadWarningNote(BinaryReader reader)
        {
            var time = reader.ReadSingle();
            return new WarningNoteModel(time);
        }

        private void WriteSpeedLine(BinaryWriter writer, SpeedLineData line)
        {
            writer.Write(line.StartTime);
            writer.Write(line.Speed);
            writer.Write(line.WarningType);
        }

        private static SpeedLineData ReadSpeedLine(BinaryReader reader)
        {
            var start = reader.ReadSingle();
            var speed = reader.ReadSingle();
            var warningType = reader.Read<WarningType>();
            return new SpeedLineData(start, speed, warningType);
        }

        private void WriteSound(BinaryWriter writer, PianoSoundData sound)
        {
            writer.Write(sound.Delay);
            writer.Write(sound.Duration);
            writer.Write(sound.Pitch);
            writer.Write(sound.Velocity);
        }

        private static PianoSoundData ReadSound(BinaryReader reader)
        {
            var delay = reader.ReadSingle();
            var duration = reader.ReadSingle();
            var pitch = reader.ReadInt32();
            var velocity = reader.ReadInt32();
            return new PianoSoundData(delay, duration, pitch, velocity);
        }

    }
}
