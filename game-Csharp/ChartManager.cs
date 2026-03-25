using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class SongData { public SongInfo song; }
[Serializable] public class SongInfo { public string song; public float bpm; public List<Section> notes; }

[Serializable]
public class Section { 
    public List<NoteWrapper> sectionNotes; // [[...]] を [{"row":[...]}] に見せかける
    public bool mustHitSection; 
}

[Serializable] public class NoteWrapper { public float[] row; }

public class ChartManager : MonoBehaviour {
    public TextAsset jsonFile;
    public GameObject notePrefab;
    public AudioSource bgm;

    private List<NoteData> noteList = new List<NoteData>();

    [Serializable]
    public class NoteData {
        public float spawnTime;
        public int lane;
        public int width;
        public bool isSpawned = false;
    }

    void Start() {
        if (jsonFile == null) return;
        
        // --- 強制フォーマット変換 ---
        string jsonText = jsonFile.text;
        // 二次元配列をJsonUtilityが読めるオブジェクト配列に置換
        jsonText = jsonText.Replace("[[", "[{\"row\":[");
        jsonText = jsonText.Replace("],[", "]},{\"row\":[");
        jsonText = jsonText.Replace("]]", "]]}");

        SongData data = JsonUtility.FromJson<SongData>(jsonText);
        
        if (data?.song?.notes != null) {
            foreach (var sec in data.song.notes) {
                foreach (var wrapper in sec.sectionNotes) {
                    float[] arr = wrapper.row;
                    if (arr.Length >= 4) { // インデックスエラー防止
                        noteList.Add(new NoteData {
                            spawnTime = arr[0] / 1000f,
                            lane = (int)arr[1],
                            width = (int)arr[3]
                        });
                    }
                }
            }
        }

        noteList.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
        if (bgm != null) bgm.Play();
    }

    void Update() {
        if (bgm == null || !bgm.isPlaying) return;
        float currentTime = bgm.time;

        foreach (var note in noteList) {
            // 判定の2秒前に生成（降ってくる時間を確保）
            if (!note.isSpawned && currentTime >= note.spawnTime - 2.0f) {
                note.isSpawned = true;
                CreateNote(note);
            }
        }
    }

    void CreateNote(NoteData data) {
        GameObject obj = Instantiate(notePrefab);
        // ここでノーツに lane や width を渡す処理を塾で書く！
        Debug.Log($"【生成】Time:{data.spawnTime}s / Lane:{data.lane} / Width:{data.width}");
    }
}
