using System;
using System.Collections.Generic;
using UnityEngine;

namespace WordGarden
{
    public enum TaskKind { Picture, Listen, Sentence }

    [Serializable]
    public class Challenge
    {
        public string id, unit, instruction, prompt, answer, spoken, illustration;
        public TaskKind kind;
        public string[] choices;
        public Challenge(string id, string unit, TaskKind kind, string instruction, string prompt,
            string illustration, string answer, string spoken, params string[] choices)
        {
            this.id=id; this.unit=unit; this.kind=kind; this.instruction=instruction;
            this.prompt=prompt; this.illustration=illustration; this.answer=answer;
            this.spoken=spoken; this.choices=choices;
        }
    }

    // Hand-authored scaffold; a curriculum and child testing are required before release.
    public static class Curriculum
    {
        public static readonly Challenge[] All = {
            new Challenge("w-cat", "01 • חיות", TaskKind.Picture, "בחרו את המילה שמתאימה לתמונה", "מי פגשנו בגינה?", "cat", "cat", "cat", "dog", "cat", "fox"),
            new Challenge("w-dog", "01 • חיות", TaskKind.Picture, "בחרו את המילה שמתאימה לתמונה", "מי פגשנו בגינה?", "dog", "dog", "dog", "fox", "cat", "dog"),
            new Challenge("w-fox", "01 • חיות", TaskKind.Picture, "בחרו את המילה שמתאימה לתמונה", "מי מדריך אותנו בגינה?", "fox", "fox", "fox", "cat", "dog", "fox"),
            new Challenge("w-sun", "02 • סביבנו", TaskKind.Picture, "בחרו מה מאיר את שביל הגינה", "מה מופיע בתמונה?", "sun", "sun", "sun", "moon", "sun", "star"),
            new Challenge("l-cat", "03 • מקשיבים", TaskKind.Listen, "לחצו על הקש לשמוע ובחרו את החיה", "איזו חיה שמעתם?", "", "cat", "cat", "fox", "dog", "cat"),
            new Challenge("l-dog", "03 • מקשיבים", TaskKind.Listen, "לחצו על הקש לשמוע ובחרו את החיה", "איזו חיה שמעתם?", "", "dog", "dog", "cat", "fox", "dog"),
            new Challenge("s-cat", "04 • משפט ראשון", TaskKind.Sentence, "סדרו את המילים למשפט", "החתול קטן", "cat", "The cat is small.", "The cat is small", "The", "cat", "is", "small."),
            new Challenge("s-dog", "04 • משפט ראשון", TaskKind.Sentence, "סדרו את המילים למשפט", "הכלב שמח", "dog", "The dog is happy.", "The dog is happy", "The", "dog", "is", "happy."),
            new Challenge("r-sun", "05 • חוזרים לגינה", TaskKind.Picture, "בוחרים שוב, כבר מכירים את השמש", "מה מאיר את הגינה?", "sun", "sun", "sun", "star", "moon", "sun"),
        };
    }

    [Serializable]
    public class MemoryState
    {
        public int lessonsCompleted;
        public List<string> mastered = new List<string>();
        public List<string> review = new List<string>();
        public int streak;
    }

    public sealed class LearningEngine
    {
        public MemoryState Memory { get; private set; }
        public LearningEngine(MemoryState memory) { Memory = memory ?? new MemoryState(); }
        public bool Complete => Memory.lessonsCompleted >= Curriculum.All.Length;
        public void Restart() { Memory.lessonsCompleted=0; Memory.review.Clear(); Memory.streak=0; }
        public Challenge Current
        {
            get
            {
                // A wrong item returns in a short loop, before new content.
                if (Memory.review.Count > 0 && Memory.lessonsCompleted % 2 == 0)
                {
                    var item = Array.Find(Curriculum.All, c => c.id == Memory.review[0]);
                    if (item != null) return item;
                }
                return Curriculum.All[Math.Min(Memory.lessonsCompleted, Curriculum.All.Length-1)];
            }
        }
        public bool Submit(Challenge challenge, string answer)
        {
            bool correct = String.Equals(challenge.answer, answer, StringComparison.Ordinal);
            if (correct)
            {
                if (!Memory.mastered.Contains(challenge.id)) Memory.mastered.Add(challenge.id);
                Memory.review.Remove(challenge.id);
                if (Memory.lessonsCompleted < Curriculum.All.Length &&
                    Curriculum.All[Memory.lessonsCompleted].id == challenge.id)
                    Memory.lessonsCompleted++;
                Memory.streak++;
            }
            else
            {
                if (!Memory.review.Contains(challenge.id)) Memory.review.Add(challenge.id);
                Memory.streak = 0;
            }
            return correct;
        }
        public static MemoryState Load()
        {
            var raw = PlayerPrefs.GetString("wg.progress.v1", "");
            if (String.IsNullOrEmpty(raw)) return new MemoryState();
            try { return JsonUtility.FromJson<MemoryState>(raw) ?? new MemoryState(); }
            catch { return new MemoryState(); }
        }
        public void Save() { PlayerPrefs.SetString("wg.progress.v1", JsonUtility.ToJson(Memory)); PlayerPrefs.Save(); }
    }

    [Serializable]
    public class DuelState
    {
        public int round, player, scoreA, scoreB;
        public bool finished;
        public readonly int maxRounds = 6;
        public Challenge Challenge => Curriculum.All[Math.Min(round, Curriculum.All.Length-1)];
        public void Play(string answer)
        {
            if (finished) return;
            if (String.Equals(answer, Challenge.answer, StringComparison.Ordinal))
            {
                if (player == 0) scoreA++; else scoreB++;
            }
            player = 1 - player;
            if (player == 0) round++;
            finished = round >= maxRounds;
        }
    }
}
