using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class SongData { public SongInfo song; }
[Serializable] public class SongInfo { public string song; public float bpm; public List<Section> notes; }
[Serializable] public class Section { public List<NoteWrapper> sectionNotes; public bool mustHitSection; }
[Serializable] public class NoteWrapper { public float[] row; }

public class ChartManager : MonoBehaviour {
    [Header("ファイル設定")]
    public TextAsset jsonFile;      // エディタで作ったJSON
    public GameObject notePrefab;   // ノーツのPrefab
    public AudioSource bgm;         // BGM

    [Header("ゲーム設定")]
    public float noteSpeed = 15.0f; // 落ちる速度
    public float spawnOffset = 2.0f; // 何秒前に出現させるか

    private List<NoteData> noteList = new List<NoteData>();

    [Serializable]
    public class NoteData {
        public float targetTime; // 叩くべき秒
        public int lane;
        public int width;
        public bool isSpawned = false;
    }

    void Start() {
        if (jsonFile == null) return;
        
        // --- [[...]] を JsonUtility が読める形式に置換ハック ---
        string jsonText = jsonFile.text;
        jsonText = jsonText.Replace("[[", "[{\"row\":[");
        jsonText = jsonText.Replace("],[", "]},{\"row\":[");
        jsonText = jsonText.Replace("]]", "]]}");

        SongData data = JsonUtility.FromJson<SongData>(jsonText);
        
        if (data?.song?.notes != null) {
            foreach (var sec in data.song.notes) {
                foreach (var wrapper in sec.sectionNotes) {
                    float[] arr = wrapper.row;
                    if (arr.Length >= 4) {
                        noteList.Add(new NoteData {
                            targetTime = arr[0] / 1000f, // msを秒に
                            lane = (int)arr[1],
                            width = (int)arr[3]
                        });
                    }
                }
            }
        }

        // 時間順に並び替え
        noteList.Sort((a, b) => a.targetTime.CompareTo(b.targetTime));
        
        if (bgm != null) bgm.Play();
    }

    void Update() {
        if (bgm == null || !bgm.isPlaying) return;

        float currentTime = bgm.time;

        foreach (var note in noteList) {
            // 指定時間の「spawnOffset秒前」になったら生成メッセージ
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
            // ノーツ自身に「初期位置・幅・目標時間・速度」を伝える
            noteScript.Init(data.lane, data.width, data.targetTime, noteSpeed);
        }
    }
}
