using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class SongData { public SongInfo song; }
[Serializable] public class SongInfo { public string song; public float bpm; public List<Section> notes; }
[Serializable] public class Section { public List<NoteWrapper> sectionNotes; public bool mustHitSection; }
[Serializable] public class NoteWrapper { public float[] row; }

public class ChartManager : MonoBehaviour {
    public TextAsset jsonFile;
    public GameObject notePrefab;
    public AudioSource bgm;

    public float noteSpeed = 10.0f; // 落ちる速さ
    public float spawnOffset = 2.0f; // 2秒前に出現

    private List<NoteData> noteList = new List<NoteData>();

    [Serializable]
    public class NoteData {
        public float targetTime;
        public int lane;
        public int width;
        public bool isSpawned = false;
    }

    void Start() {
        if (jsonFile == null) return;
        
        string jsonText = jsonFile.text;
        jsonText = jsonText.Replace("[[", "[{\"row\":[");
        jsonText = jsonText.Replace("],[", "]},{\"row\":[");
        jsonText = jsonText.Replace("]]", "]]}");

        SongData data = JsonUtility.FromJson<SongData>(jsonText);
        
        if (data?.song?.notes != null) {
            foreach (var sec in data.song.notes) {
                foreach (var wrapper in sec.sectionNotes) {
                    float[] arr = wrapper.row;
                    noteList.Add(new NoteData {
                        targetTime = arr[0] / 1000f,
                        lane = (int)arr[1],
                        width = (int)arr[3]
                    });
                }
            }
        }
        noteList.Sort((a, b) => a.targetTime.CompareTo(b.targetTime));
        if (bgm != null) bgm.Play();
    }

    void Update() {
        if (bgm == null || !bgm.isPlaying) return;
        float currentTime = bgm.time;

        foreach (var note in noteList) {
            if (!note.isSpawned && currentTime >= note.targetTime - spawnOffset) {
                note.isSpawned = true;
                SpawnNote(note);
            }
        }
    }

    void SpawnNote(NoteData data) {
        GameObject obj = Instantiate(notePrefab);
        Note noteScript = obj.GetComponent<Note>();
        if (noteScript != null) {
            // 2Dなので「現在のBGM時間」も渡して同期させます
            noteScript.Init(data.lane, data.width, data.targetTime, noteSpeed, bgm);
        }
    }
}
