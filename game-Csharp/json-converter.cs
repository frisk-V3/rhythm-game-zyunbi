using System;
using System.Collections.Generic;
using UnityEngine;

// --- JSON構造の定義（Unity標準で読み込めるようにラッパーを挟む） ---
[Serializable] public class SongData { public SongInfo song; }
[Serializable] public class SongInfo { public string song; public float bpm; public float speed; public List<Section> notes; }

[Serializable]
public class Section { 
    public List<NoteWrapper> sectionNotes; // ここをWrapperクラスに変更
    public bool mustHitSection; 
}

// float[] を "row" という名前で包むためのクラス
[Serializable]
public class NoteWrapper {
    public float[] row;
}

public class ChartManager : MonoBehaviour {
    [Header("設定")]
    public TextAsset jsonFile;      // 読み込むJSON
    public GameObject notePrefab;   // 生成するノーツ
    public AudioSource bgm;         // BGM再生用

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
        
        // --- 【裏技】JSONの [[ ... ]] を [{"row":[ ... ]}] に無理やり置換する ---
        string jsonText = jsonFile.text;
        jsonText = jsonText.Replace("[[", "[{\"row\":[");
        jsonText = jsonText.Replace("],[", "]},{\"row\":[");
        jsonText = jsonText.Replace("]]", "]]}");

        // 置換したテキストなら JsonUtility で読み込める！
        SongData data = JsonUtility.FromJson<SongData>(jsonText);
        
        if (data != null && data.song != null) {
            foreach (var sec in data.song.notes) {
                foreach (var wrapper in sec.sectionNotes) {
                    float[] arr = wrapper.row; // ここで の中身が取れる
                    
                    noteList.Add(new NoteData {
                        spawnTime = arr[0] / 1000f, // ミリ秒を秒に
                        lane = (int)arr[1],
                        width = (int)arr[3]
                    });
                }
            }
        }

        // 時間順に並び替え
        noteList.Sort((a, b) => a.spawnTime.CompareTo(b.spawnTime));
        
        if (bgm != null) bgm.Play();
    }

    void Update() {
        if (bgm == null || !bgm.isPlaying) return;

        float currentTime = bgm.time;

        foreach (var note in noteList) {
            // 判定ラインの2秒前に出現させる
            if (!note.isSpawned && currentTime >= note.spawnTime - 2.0f) {
                note.isSpawned = true;
                CreateNote(note);
            }
        }
    }

    void CreateNote(NoteData data) {
        // ノーツを生成（座標などは調整してね！）
        GameObject obj = Instantiate(notePrefab);
        Debug.Log($"【生成】Time:{data.spawnTime}s / Lane:{data.lane} / Width:{data.width}");
    }
}
